using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordOSPLBot
{
    public class Match
    {
        // Readable match status strings.
        public enum States { Blank = 0, Created = 1, Accepted = 2, Reported = 3, Confirmed = 4, TimedOut = 5, Cancelled = 6 }

        public int ID { get; set; }
        public string Challenger { get; set; }
        public string Opponent { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public States State { get; set; }
        public int ChallengerScore { get; set; }
        public int OpponentScore { get; set; }

        public Match()
        {
            ID = 0;
            Created = DateTime.Now;
            Modified = DateTime.MinValue;
            State = States.Blank;
            ChallengerScore = 0;
            OpponentScore = 0;
        }

        public string Status
        {
            get
            {
                switch (State)
                {
                    case States.Accepted:
                        return "Accepted";

                    case States.Reported:
                        return "Reported";

                    case States.Confirmed:
                        return "Confirmed";

                    case States.TimedOut:
                        return "Timed out";

                    case States.Cancelled:
                        return "Cancelled";

                    default:
                        return "Created";
                }
            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                DateTime ends = Created;
                ends = DateTime.SpecifyKind(ends, DateTimeKind.Utc);
                ends = ends.AddHours(Convert.ToInt32(ConfigurationManager.AppSettings["MatchTimeoutHours"]));

                DateTime utcDateTime = DateTime.UtcNow;
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

                if (utcDateTime > ends) return new TimeSpan(0, 0, 0);

                return ends - utcDateTime;
            }
        }

        public bool HasTimedOut
        {
            get { return (TimeLeft == new TimeSpan(0, 0, 0)); }
        }
    }

    public class MatchHandler
    {
        public static string UserIdFromSockerUser(SocketUser user)
        {
            return user.Username + "#" + user.Discriminator;
        }

        public static string UserName(string discordId)
        {
            if (discordId.Contains("#"))
            {
                string[] temp = discordId.Split("#".ToCharArray());
                return Misc.Caps(temp[0]);
            }
            else
            {
                return Misc.Caps(discordId);
            }
        }

        public static string Discriminator(string discordId)
        {
            if (discordId.Contains("#"))
            {
                string[] temp = discordId.Split("#".ToCharArray());
                return temp[1];
            }
            else
            {
                return discordId;
            }
        }

        public static List<Match> GetMatches(int count, Match.States state, string discordId = null)
        {
            lock (User.lockDb)
            {
                if (count > 0) CheckForTimeouts();
                else count = -count;

                List<Match> pending = new List<Match>();

                SqlCommand command = new SqlCommand();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    string topCount = (count < 0) ? "" : $"top ({count})";

                    if (state == Match.States.Blank) command.CommandText = $"select {topCount} * from OSPL_Matches";
                    else command.CommandText = $"select {topCount} * from OSPL_Matches where (State = {(int)state})";

                    if (!String.IsNullOrEmpty(discordId))
                    {
                        // Only get matches for a specific user.
                        if (command.CommandText.Contains(" where (State")) command.CommandText += " and ";
                        else command.CommandText += " where ";
                        command.CommandText += "((Challenger = @discordId) or (Opponent = @discordId))";
                        command.Parameters.AddWithValue("@discordId", discordId);
                    }

                    // Sort by date with newest first.
                    if (state == Match.States.Created) command.CommandText += " order by Created desc";
                    if (state == Match.States.Accepted) command.CommandText += " order by Modified desc";
                    if (state == Match.States.Reported) command.CommandText += " order by Modified desc";
                    if (state == Match.States.Confirmed) command.CommandText += " order by Modified desc";
                    if (state == Match.States.Cancelled) command.CommandText += " order by Modified desc";

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Match match = new Match();

                        match.ID = Convert.ToInt32(reader["ID"].ToString());

                        match.Challenger = reader["Challenger"].ToString();
                        match.Opponent = reader["Opponent"].ToString();

                        match.State = (Match.States)Convert.ToInt32(reader["State"].ToString());

                        match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                        match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);

                        match.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
                        match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                        match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                        match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                        pending.Add(match);
                    }

                    return pending;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.GetMatches()", "Unable to get matches", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Find(int id)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                Match match = new Match();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    // Calculate next ID.
                    command.CommandText = $"select * from OSPL_Matches where (ID = @ID)";
                    command.Parameters.AddWithValue("@ID", id);

                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            match.ID = Convert.ToInt32(reader["ID"].ToString());
                            match.State = (Match.States)Convert.ToInt32(reader["State"].ToString());

                            // Check for time out.
                            if ((match.State == Match.States.Created) && match.HasTimedOut)
                            {
                                reader.Close();

                                match.State = Match.States.Cancelled;

                                command.CommandText = $"update OSPL_Matches set State = @State, Modified = @Modified where (ID = @ID)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@ID", match.ID);
                                command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(DateTime.UtcNow));

                                command.ExecuteNonQuery();

                                match.ID = 0;

                                return match;
                            }

                            match.Challenger = Misc.Caps(reader["Challenger"].ToString());
                            match.Opponent = Misc.Caps(reader["Opponent"].ToString());

                            match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                            match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);

                            match.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
                            match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                            match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                            match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                            reader.Close();

                            return match;
                        }
                    }
                    catch// (Exception ex)
                    {
                        reader.Close();
                    }

                    return match;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Find()", $"Unable to find match {id}", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Find(string pilot1, string pilot2, Match.States state)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                Match match = new Match();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    // Calculate next ID.
                    command.CommandText = $"select * from OSPL_Matches where ((Challenger = @Pilot1) and ((Opponent = @Pilot2)) or ((Challenger = @Pilot2) and (Opponent = @Pilot1))) and (State = @State)";
                    command.Parameters.AddWithValue("@Pilot1", pilot1);
                    command.Parameters.AddWithValue("@Pilot2", pilot2);
                    command.Parameters.AddWithValue("@State", (int)state);

                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        if (reader.Read())
                        {
                            match.ID = Convert.ToInt32(reader["ID"].ToString());
                            match.State = (Match.States)Convert.ToInt32(reader["State"].ToString());

                            // Check for time out.
                            if ((match.State == Match.States.Created) && match.HasTimedOut)
                            {
                                reader.Close();

                                match.State = Match.States.Cancelled;

                                command.CommandText = $"update OSPL_Matches set State = @State, Modified = @Modified where (ID = @ID)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@ID", match.ID);
                                command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(DateTime.UtcNow));

                                command.ExecuteNonQuery();

                                match.ID = 0;

                                return match;
                            }

                            match.Challenger = Misc.Caps(reader["Challenger"].ToString());
                            match.Opponent = Misc.Caps(reader["Opponent"].ToString());

                            match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                            match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);

                            match.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
                            match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                            match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                            match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                            reader.Close();

                            return match;
                        }
                    }
                    catch (Exception ex)
                    {
                        reader.Close();
                    }

                    return match;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Find()", $"Unable to find match for {pilot1} and {pilot2}", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        private static void CheckForTimeouts()
        {
            List<Match> created = GetMatches(-9999, Match.States.Created);
            foreach (Match m in created) if (m.HasTimedOut) try { Cancel(m); } catch { }
        }

        private static void Cancel(Match match)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.CommandText = $"update OSPL_Matches set State = {(int)Match.States.Cancelled}, Modified = @Modified where ID = @ID";
                    command.Parameters.AddWithValue("@ID", match.ID);
                    command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(DateTime.UtcNow));

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Cancel()", $"Unable to cancel match {match.ID}", ex));
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Create(string challenger, string opponent)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                Match match = new Match();
                match.Challenger = challenger;
                match.Opponent = opponent;

                // All dates in the database are stored as UTC dates.
                match.Created = Misc.UTC(DateTime.Now);
                match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);
                match.Modified = match.Created;
                match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    // Calculate next ID.
                    int id = 1;
                    command.CommandText = "select max(ID) from OSPL_Matches";

                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        if (reader.Read()) id = reader.GetInt32(0) + 1;
                    }
                    catch
                    {
                    }

                    reader.Close();

                    command.CommandText = "insert into OSPL_Matches " +
                                            "(ID,   Challenger,  Opponent,  State,  Created,  Modified, ChallengerScore, OpponentScore) " +
                                          "values " +
                                            "(@ID, @Challenger, @Opponent, @State, @Created, @Modified, @ChallengerScore, @OpponentScore)";

                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@Challenger", match.Challenger);
                    command.Parameters.AddWithValue("@Opponent", match.Opponent);
                    command.Parameters.AddWithValue("@State", Match.States.Created);
                    command.Parameters.AddWithValue("@Created", Misc.SqlDateTime(match.Created));
                    command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(match.Modified));
                    command.Parameters.AddWithValue("@ChallengerScore", match.ChallengerScore);
                    command.Parameters.AddWithValue("@OpponentScore", match.OpponentScore);

                    int rows = command.ExecuteNonQuery();
                    if (rows == 1)
                    {
                        command.Transaction.Commit();
                        command.Transaction = null;
                        match.ID = id;

                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Create()", $"New match created for {challenger} vs {opponent}."));
                    }

                    return match;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Create()", "Unable to create match", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Cancel(string challenger, string opponent)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                Match noMatch = new Match();
                noMatch.Challenger = challenger;
                noMatch.Opponent = opponent;

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    command.CommandText = $"select top (1) * from OSPL_Matches " +
                                          $"where (Challenger = @Challenger) and (Opponent = @Opponent) and ((State = {(int)Match.States.Created}) or (State = {(int)Match.States.Accepted}))";

                    command.Parameters.AddWithValue("@Challenger", challenger);
                    command.Parameters.AddWithValue("@Opponent", opponent);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        Match match = new Match();

                        match.ID = Convert.ToInt32(reader["ID"].ToString());
                        match.Challenger = reader["Challenger"].ToString();
                        match.Opponent = reader["Opponent"].ToString();
                        match.State = Convert.ToInt32(reader["State"].ToString()) == ((int)Match.States.Created) ? Match.States.Created : Match.States.Accepted;

                        match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                        match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);

                        match.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
                        match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                        match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                        match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                        reader.Close();

                        command.CommandText = $"update OSPL_Matches set State = {(int)Match.States.Cancelled}, Modified=@Modified where (ID = @ID)";

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@ID", match.ID);
                        match.Modified = Misc.UTC(DateTime.Now);
                        command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(match.Modified));

                        int rows = command.ExecuteNonQuery();
                        if (rows == 1)
                        {
                            command.Transaction.Commit();
                            command.Transaction = null;

                            Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Cancel()", $"Match for {challenger} vs {opponent} has been cancelled."));

                            match.State = Match.States.Cancelled;
                            return match;
                        }
                    }

                    command.Transaction.Rollback();
                    command.Transaction = null;

                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Cancel()", $"No match found for {challenger} vs {opponent}."));

                    return noMatch;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Cancel()", "Unable to cancel match", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Accept(string challenger, string opponent)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                Match noMatch = new Match();
                noMatch.Challenger = challenger;
                noMatch.Opponent = opponent;

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    command.CommandText = $"select top (1) * from OSPL_Matches where (Challenger = @Challenger) and (Opponent = @Opponent) and (State = {(int)Match.States.Created})";

                    command.Parameters.AddWithValue("@Challenger", challenger);
                    command.Parameters.AddWithValue("@Opponent", opponent);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        Match match = new Match();

                        reader.Read();

                        match.ID = Convert.ToInt32(reader["ID"].ToString());
                        match.State = (Match.States)Convert.ToInt32(reader["State"].ToString());

                        // Check for time out.
                        if ((match.State == Match.States.Created) && match.HasTimedOut)
                        {
                            reader.Close();

                            match.State = Match.States.Cancelled;

                            command.CommandText = $"update OSPL_Matches set State = @State, Modified = @Modified where (ID = @ID)";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@ID", match.ID);
                            command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(DateTime.UtcNow));

                            command.ExecuteNonQuery();

                            match.ID = 0;

                            command.Transaction.Commit();
                            command.Transaction = null;

                            return match;
                        }

                        match.Challenger = Misc.Caps(reader["Challenger"].ToString());
                        match.Opponent = Misc.Caps(reader["Opponent"].ToString());

                        match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                        match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);

                        match.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
                        match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                        match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                        match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                        reader.Close();

                        // Check for timeout.
                        bool timedOut = match.HasTimedOut;

                        if (match.HasTimedOut == false)
                        {
                            // Accept the match.
                            command.CommandText = $"update OSPL_Matches set State = {(int)Match.States.Accepted}, Modified=@Modified where (ID = @ID)";
                        }
                        else
                        {
                            // Cancel the match.
                            command.CommandText = $"update OSPL_Matches set State = {(int)Match.States.Cancelled}, Modified=@Modified where (ID = @ID)";
                        }

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@ID", match.ID);
                        match.Modified = Misc.UTC(DateTime.Now);
                        command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(match.Modified));

                        int rows = command.ExecuteNonQuery();
                        if (rows == 1)
                        {
                            command.Transaction.Commit();
                            command.Transaction = null;

                            if (timedOut == false)
                            {
                                Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Accept()", $"Match with {challenger} vs {opponent} has been accepted."));
                                match.State = Match.States.Accepted;
                            }
                            else
                            {
                                Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Accept()", $"Match with {challenger} vs {opponent} has been auto-cancelled."));
                                match.State = Match.States.Cancelled;
                                match.ID *= -1;
                            }

                            return match;
                        }
                    }

                    reader.Close();

                    command.Transaction.Rollback();
                    command.Transaction = null;

                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Accept()", $"No match found for {challenger} vs {opponent}."));

                    return noMatch;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Accept()", "Unable to cancel a match", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Report(int id, int score1, int score2)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                Match noMatch = new Match();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    command.CommandText = $"select * from OSPL_Matches where (ID = @ID) and ((State = {(int)Match.States.Accepted}) or (State = {(int)Match.States.Reported}))";
                    command.Parameters.AddWithValue("@ID", id);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        Match match = new Match();

                        match.ID = Convert.ToInt32(reader["ID"].ToString());

                        match.Challenger = Misc.Caps(reader["Challenger"].ToString());
                        match.Opponent = Misc.Caps(reader["Opponent"].ToString());

                        match.State = Match.States.Reported;

                        match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                        match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);

                        match.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
                        match.Modified = DateTime.SpecifyKind(match.Modified, DateTimeKind.Utc);

                        match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                        match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                        reader.Close();

                        match.ChallengerScore = score1;
                        match.OpponentScore = score2;

                        command.CommandText = $"update OSPL_Matches set State = {(int)Match.States.Reported}, ChallengerScore = @Score1, OpponentScore = @Score2, Modified = @Modified where (ID = @ID)";

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@ID", match.ID);
                        match.Modified = Misc.UTC(DateTime.Now);
                        command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(match.Modified));
                        command.Parameters.AddWithValue("@Score1", score1);
                        command.Parameters.AddWithValue("@Score2", score2);

                        int rows = command.ExecuteNonQuery();
                        if (rows == 1)
                        {
                            command.Transaction.Commit();
                            command.Transaction = null;

                            Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Report()", $"Results reported for match {id}."));

                            match.State = Match.States.Reported;
                            return match;
                        }
                    }

                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Report()", $"No match found with ID = {id}."));

                    return noMatch;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "Match.Report()", "Unable to confirm a match", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static Match Confirm(Match match)
        {
            lock (User.lockDb)
            {
                User winner, looser;

                if (match.ChallengerScore > match.OpponentScore)
                {
                    winner = User.Find(match.Challenger);
                    looser = User.Find(match.Opponent);
                }
                else
                {
                    winner = User.Find(match.Opponent);
                    looser = User.Find(match.Challenger);
                }

                if ((winner == null) || (looser == null))
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Confirm()", $"Cannot get user info for either {match.Challenger} or {match.Opponent}."));
                    match.ID *= -1;
                    return match;
                }

                SqlCommand command = new SqlCommand();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    // Archive match results.

                    command.CommandText = "insert into OSPL_MatchHistory " +
                                             "(ID,   Challenger,  Opponent,  Created, ChallengerScore, OpponentScore) " +
                                           "values " +
                                             "(@ID, @Challenger, @Opponent, @Created, @ChallengerScore, @OpponentScore)";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@ID", match.ID);
                    command.Parameters.AddWithValue("@Challenger", match.Challenger);
                    command.Parameters.AddWithValue("@Opponent", match.Opponent);
                    command.Parameters.AddWithValue("@Created", Misc.SqlDateTime(DateTime.UtcNow));
                    command.Parameters.AddWithValue("@ChallengerScore", match.ChallengerScore);
                    command.Parameters.AddWithValue("@OpponentScore", match.OpponentScore);

                    int rows = command.ExecuteNonQuery();
                    if (rows != 1)
                    {
                        command.Transaction.Rollback();
                        command.Transaction = null;
                        match.ID *= -1;

                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Confirm()", $"Unable to archive match history for match {match.ID}."));
                    }

                    match.Modified = Misc.UTC(DateTime.Now);

                    // Update user stats.

                    command.CommandText = $"update OSPL_Users set Wins = Wins + 1, LastPlayed = @LastPlayed where (ID = @ID)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@ID", winner.ID);
                    command.Parameters.AddWithValue("@LastPlayed", Misc.SqlDateTime(DateTime.UtcNow));
                    rows = command.ExecuteNonQuery();

                    command.CommandText = $"update OSPL_Users set Defeats = Defeats + 1, LastPlayed = @LastPlayed where (ID = @ID)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@ID", looser.ID);
                    command.Parameters.AddWithValue("@LastPlayed", Misc.SqlDateTime(DateTime.UtcNow));
                    rows = command.ExecuteNonQuery();

                    // Update ranks.

                    command.CommandText = $"update OSPL_Matches set State = {(int)Match.States.Confirmed}, Modified=@Modified where (ID = @ID)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@ID", match.ID);
                    command.Parameters.AddWithValue("@Modified", Misc.SqlDateTime(match.Modified));
                    rows = command.ExecuteNonQuery();

                    // Update ladder.

                    // No change to ladder if winners rank is above loosers rank.
                    if (winner.Rank < looser.Rank)
                    {
                        command.Transaction.Commit();
                        command.Transaction = null;

                        match.State = Match.States.Confirmed;
                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Confirm()", $"No ladder change for match {match.ID} as winner was ranked higher than looser."));

                        MiscModule.SendMessage("dev-debugging", $"No ladder change for match {match.ID} as winner was ranked higher than looser.");

                        return match;
                    }

                    // Check if winner and looser are next to each other.
                    if (Math.Abs(winner.Rank - looser.Rank) == 1)
                    {
                        // Switch ladder position.
                        command.CommandText = "update OSPL_Users set Rank = @Rank where ID = @ID";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Rank", looser.Rank);
                        command.Parameters.AddWithValue("@ID", winner.ID);
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Rank", winner.Rank);
                        command.Parameters.AddWithValue("@ID", looser.ID);
                        command.ExecuteNonQuery();

                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Confirm()", $"Winner {winner.ID} and looser {looser.ID} exchange ranks."));

                        MiscModule.SendMessage("dev-debugging", $"Winner { winner.ID} and looser {looser.ID}. exchange ranks.");
                    }
                    else
                    {
                        // Calculate new position for winner and then move his/her ladder position.
                        int distance = Math.Abs(winner.Rank - looser.Rank) / 2;
                        int currentRank = winner.Rank;
                        int newRank = winner.Rank - distance;

                        // First shift other pilots down in the ladder.
                        command.CommandText = "update OSPL_Users set Rank = Rank + 1 where (Rank >= @NewRank) and (Rank < @CurrentRank) ";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@NewRank", newRank);
                        command.Parameters.AddWithValue("@CurrentRank", currentRank);
                        command.ExecuteNonQuery();

                        // Then set rank or winning pilot.
                        command.CommandText = "update OSPL_Users set Rank = @NewRank where ID = @ID";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@NewRank", newRank);
                        command.Parameters.AddWithValue("@ID", winner.ID);
                        command.ExecuteNonQuery();

                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Confirm()", $"Winner now ranked {newRank} (was {currentRank} before)."));

                        MiscModule.SendMessage("dev-debugging", $"Winner now ranked { newRank} (was {currentRank} before).");
                    }

                    command.Transaction.Commit();
                    command.Transaction = null;

                    match.State = Match.States.Confirmed;

                    return match;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.Confirm()", "SQL exception", ex));
                    match.ID *= -1;
                    return match;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static void ClearHistory()
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();
                    command.CommandText = "delete from OSPL_MatchHistory";
                    command.ExecuteNonQuery();
                }
                catch
                {
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static void ClearMatches()
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();
                    command.CommandText = "delete from OSPL_Matches";
                    command.ExecuteNonQuery();
                }
                catch
                {
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        /// <summary>
        /// Lower a users rank.
        /// </summary>
        /// <param name="user">Discord user</param>
        internal static void LowerRank(User user)
        {
            lock (User.lockDb)
            {
                SqlCommand command = new SqlCommand();

                int oldRank = user.Rank;
                int newRank = user.Rank + 1;

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    command.CommandText = $"select * from OSPL_Users where (Rank = @Rank)";
                    command.Parameters.AddWithValue("@Rank", newRank);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            string userBelow = reader["ID"].ToString();
                            reader.Close();

                            // Move user below up one position.
                            command.CommandText = "update OSPL_Users set Rank = @Rank where ID = @ID";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@Rank", oldRank);
                            command.Parameters.AddWithValue("@ID", userBelow);
                            command.ExecuteNonQuery();

                            // Move the user down one position.
                            command.CommandText = "update OSPL_Users set Rank = @Rank where ID = @ID";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@Rank", newRank);
                            command.Parameters.AddWithValue("@ID", user.ID);
                            command.ExecuteNonQuery();

                            Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.LowerRank()", $"User {user.ID} moved to rank {newRank} (was {oldRank} before), user {userBelow} now ranked {oldRank}."));

                            MiscModule.SendMessage("dev-debugging", $"User {user.ID} moved to rank {newRank} (was {oldRank} before), user {userBelow} now ranked {oldRank}.");

                            command.Transaction.Commit();
                            command.Transaction = null;

                            return;
                        }
                    }

                    MiscModule.SendMessage("dev-debugging", $"No change in rank for user {user.ID}.");
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "Match.LowerRank()", "SQL exception", ex));
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }
    }
}
