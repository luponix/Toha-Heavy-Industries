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
    public class User
    {
        internal static object lockDb = new object();

        public string ID { get; set; }
        public int Rank { get; set; }
        public string TimeZone { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public string Options { get; set; }
        public DateTime LastPlayed { get; set; }
        public int Wins { get; set; }
        public int Defeats { get; set; }

        public User()
        {
            ID = "";
            Rank = 0;
            TimeZone = "CET";
            Notes = "";
            Email = "";
            Options = "";
            Wins = 0;
            Defeats = 0;
            LastPlayed = DateTime.UtcNow;
        }

        internal static User CreateOrFind(string id)
        {
            User user = Find(id);
            if ((user != null) && String.IsNullOrEmpty(user.ID)) user = Create(id);
            return user;
        }

        internal static User CreateOrFind(SocketUser user)
        {
            if (user.IsBot) return null;
            return CreateOrFind(user.Username + "#" + user.Discriminator);
        }

        internal static string GetTimeZone(string userId)
        {
            User user = CreateOrFind(userId);
            if (user == null) return "UTC";
            return user.TimeZone.Trim();
        }

        internal static bool SetTimeZone(string id, string timeZone)
        {
            User user = CreateOrFind(id);
            if ((user == null) || String.IsNullOrEmpty(user.ID)) return false;

            SqlCommand command = new SqlCommand();
            try
            {
                command.Connection = new SqlConnection(Misc.ConnectionString);
                command.Connection.Open();

                command.CommandText = "update OSPL_Users Set TimeZone = @TimeZone where ID = @ID";
                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@TimeZone", timeZone);

                int rows = command.ExecuteNonQuery();
                return (rows == 1);
            }
            catch (Exception ex)
            {
                Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.SetTimeZone()", $"Error setting time zone {timeZone} for user {id}", ex));
            }
            finally
            {
                Misc.SqlCleanUp(command);
            }

            return false;
        }

        internal static bool SetRank(string id, int rank)
        {
            lock (lockDb)
            {
                User user = CreateOrFind(id);
                if ((user == null) || String.IsNullOrEmpty(user.ID)) return false;

                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.CommandText = "update OSPL_Users Set Rank = @Rank where ID = @ID";
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@Rank", rank);

                    int rows = command.ExecuteNonQuery();
                    return (rows == 1);
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.SetRank()", $"Error setting rank {rank} for user {id}", ex));
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }

                return false;
            }
        }

        internal static bool Delete(string id)
        {
            lock (lockDb)
            {
                SqlCommand command = new SqlCommand();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.CommandText = "delete from OSPL_Users where ID = @ID";
                    command.Parameters.AddWithValue("@ID", id);

                    int rows = command.ExecuteNonQuery();
                    return (rows == 1);
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.SetRank()", $"Error deleting user {id}", ex));
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }

                return false;
            }
        }

        internal static bool DeleteAll()
        {
            lock (lockDb)
            {
                SqlCommand command = new SqlCommand();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.CommandText = "delete from OSPL_Users";

                    int rows = command.ExecuteNonQuery();
                    return (rows == 1);
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.SetRank()", $"Error deleting all users", ex));
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }

                return false;
            }
        }

        internal static User Find(string id)
        {
            lock (lockDb)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.CommandText = "select * from OSPL_Users where ID = @ID";
                    command.Parameters.AddWithValue("@ID", id);

                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            User user = new User();

                            user.ID = reader["ID"].ToString();
                            user.TimeZone = reader["TimeZone"].ToString().Trim();
                            user.Rank = Convert.ToInt32(reader["Rank"].ToString());
                            user.Email = reader["Email"].ToString().Trim(); ;
                            user.Notes = reader["Notes"].ToString().Trim(); ;
                            user.Options = reader["Options"].ToString().Trim(); ;

                            reader.Close();

                            return user;
                        }
                    }
                    catch
                    {
                        reader.Close();
                    }

                    return new User();
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.Find()", "Error looking up user", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static List<User> GetLadder(int count = 20)
        {
            lock (lockDb)
            {
                SqlCommand command = new SqlCommand();
                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.CommandText = $"select top ({count})* from OSPL_Users where (Rank > 0) order by Rank";

                    SqlDataReader reader = command.ExecuteReader();

                    List<User> ladder = new List<User>();

                    try
                    {
                        while (reader.Read())
                        {
                            User user = new User();
                            user.ID = reader["ID"].ToString();
                            user.TimeZone = reader["TimeZone"].ToString();
                            user.Rank = Convert.ToInt32(reader["Rank"].ToString());
                            ladder.Add(user);
                        }
                    }
                    catch
                    {
                        ladder = null;
                    }

                    reader.Close();

                    return ladder;
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.Find()", "Error looking up user", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal static async Task SendDirect(SocketUser user, string message)
        {
            await Discord.UserExtensions.SendMessageAsync(user, message);
        }

        internal static List<User> Users
        {
            get
            {
                lock (lockDb)
                {
                    SqlCommand command = new SqlCommand();

                    try
                    {
                        command.Connection = new SqlConnection(Misc.ConnectionString);
                        command.Connection.Open();

                        command.CommandText = $"select * from OSPL_Users order by Rank";

                        SqlDataReader reader = command.ExecuteReader();

                        List<User> users = new List<User>();

                        try
                        {
                            while (reader.Read())
                            {
                                User user = new User();
                                user.ID = reader["ID"].ToString();
                                user.TimeZone = reader["TimeZone"].ToString();
                                user.Rank = Convert.ToInt32(reader["Rank"].ToString());
                                users.Add(user);
                            }
                        }
                        catch
                        {
                            users = null;
                        }

                        reader.Close();

                        return users;
                    }
                    catch (Exception ex)
                    {
                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.Users", "Error looking users", ex));
                        return null;
                    }
                    finally
                    {
                        Misc.SqlCleanUp(command);
                    }
                }
            }
        }

        internal DateTime CustomDateTime(DateTime utcDateTime)
        {
            switch (TimeZone.Trim())
            {
                case "UTC":
                    return utcDateTime;

                case "EST":
                    return Misc.EST(utcDateTime);

                case "MST":
                    return Misc.MST(utcDateTime);

                case "PST":
                    return Misc.PST(utcDateTime);

                case "CET":
                    return Misc.CET(utcDateTime);

                case "GMT":
                    return Misc.GMT(utcDateTime);

                default:
                    DateTime dateTimeUnspecified = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified);
                    dateTimeUnspecified += new TimeSpan(0, Convert.ToInt32(TimeZone), 0);
                    return dateTimeUnspecified;
            }
        }

        internal static User Create(string id)
        {
            lock (lockDb)
            {
                SqlCommand command = new SqlCommand();

                try
                {
                    command.Connection = new SqlConnection(Misc.ConnectionString);
                    command.Connection.Open();

                    command.Transaction = command.Connection.BeginTransaction();

                    command.CommandText = "select max(rank) from OSPL_Users";
                    SqlDataReader reader = command.ExecuteReader();

                    int rank = 1;
                    try
                    {
                        if (reader.Read()) rank = reader.GetInt32(0) + 1;
                    }
                    catch
                    {
                    }

                    reader.Close();

                    command.CommandText = "insert into OSPL_Users (ID, TimeZone, Rank, Email, Notes, Options, LastPlayed, Wins, Defeats) " +
                                          "values (@ID, @TimeZone, @Rank, @Email, @Notes, @Options, @LastPlayed, @Wins, @Defeats)";

                    User user = new User();

                    user.ID = id;
                    user.Rank = rank;

                    command.Parameters.AddWithValue("@ID", user.ID);
                    command.Parameters.AddWithValue("@TimeZone", user.TimeZone.Trim());
                    command.Parameters.AddWithValue("@Rank", user.Rank);
                    command.Parameters.AddWithValue("@Email", user.Email.Trim());
                    command.Parameters.AddWithValue("@Notes", user.Notes.Trim());
                    command.Parameters.AddWithValue("@Options", user.Options.Trim());
                    command.Parameters.AddWithValue("@LastPlayed", Misc.SqlDateTime(DateTime.UtcNow));
                    command.Parameters.AddWithValue("@Wins", 0);
                    command.Parameters.AddWithValue("@Defeats", 0);

                    int rows = command.ExecuteNonQuery();
                    if (rows == 1)
                    {
                        command.Transaction.Commit();
                        command.Transaction = null;

                        Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "User.Create()", $"New user {id} created."));
                        return user;
                    }

                    command.Transaction.Rollback();
                    command.Transaction = null;

                    return new User();
                }
                catch (Exception ex)
                {
                    Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.Create()", $"Unable to create user {id}", ex));
                    return null;
                }
                finally
                {
                    Misc.SqlCleanUp(command);
                }
            }
        }

        internal List<Match> PlayedMatches(int count = 10)
        {
            List<Match> history = new List<Match>();
            SqlCommand command = new SqlCommand();

            try
            {
                command.Connection = new SqlConnection(Misc.ConnectionString);
                command.Connection.Open();

                string topCount = (count < 0) ? "" : $"top ({count})";

                command.CommandText = $"select {topCount} * from OSPL_MatchHistory where (Challenger = @ID) or (Opponent = @ID) order by Created desc";
                command.Parameters.AddWithValue("@ID", ID);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Match match = new Match();

                    match.ID = Convert.ToInt32(reader["ID"].ToString());
                    match.ID = match.ID; // Uppercase fix.

                    match.Challenger = Misc.Caps(reader["Challenger"].ToString());
                    match.Opponent = Misc.Caps(reader["Opponent"].ToString());
                    
                    match.State = Match.States.Confirmed;

                    match.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
                    match.Created = DateTime.SpecifyKind(match.Created, DateTimeKind.Utc);
                    match.Modified = match.Created;

                    match.ChallengerScore = Convert.ToInt32(reader["ChallengerScore"].ToString());
                    match.OpponentScore = Convert.ToInt32(reader["OpponentScore"].ToString());

                    history.Add(match);
                }

                return history;
            }
            catch (Exception ex)
            {
                Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "User.GetResults()", $"Unable to get user results for {ID}", ex));
                return null;
            }
            finally
            {
                Misc.SqlCleanUp(command);
            }
        }
    }
}
