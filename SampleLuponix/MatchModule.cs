using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordOSPLBot
{
    public class MatchModule : ModuleBase<SocketCommandContext>
    {
        private const string KAHA = @"KAHA#6655";
        private const string Dreawus = @"dreawus#8802";
        private const string Hiflier = @"hiflier#9320";
        private const string JoBoOne = @"Jobo-One#8261";
        private const string Maestro = @"Maestro#4825";

        /// <summary>
        /// Removes match text/voice channels if the match has been confirmed or cancelled.
        /// </summary>
        /// <param name="context">The socket context to be used when searching/removing channels.</param>
        internal static void DoCleanUp(SocketCommandContext context)
        {
            foreach (IChannel channel in context.Guild.Channels)
            {
                if (channel.Name.StartsWith("match-"))
                {
                    int id = Convert.ToInt32(channel.Name.Split("-".ToCharArray())[1]);
                    Match match = MatchHandler.Find(id);
                    if ((match != null) && ((match.ID == 0) || (match.State == Match.States.Confirmed) || (match.State == Match.States.Cancelled) && (match.State == Match.States.TimedOut)))
                    {
                        if (channel is ITextChannel)
                        {
                            try
                            {
                                Program.Log(new LogMessage(LogSeverity.Info, "DoCleanUp()", $"Deleting match text channel {channel.Name}"));
                                (channel as ITextChannel).DeleteAsync();
                                Program.Log(new LogMessage(LogSeverity.Info, "DoCleanUp()", $"Delete OK"));
                            }
                            catch
                            {
                                Program.Log(new LogMessage(LogSeverity.Error, "DoCleanUp()", $"Delete error"));
                            }
                        }
                        else if (channel is IVoiceChannel)
                        {
                            try
                            {
                                Program.Log(new LogMessage(LogSeverity.Info, "DoCleanUp()", $"Deleting match voice channel {channel.Name}"));
                                (channel as IVoiceChannel).DeleteAsync();
                                Program.Log(new LogMessage(LogSeverity.Info, "DoCleanUp()", $"Delete OK"));
                            }
                            catch
                            {
                                Program.Log(new LogMessage(LogSeverity.Error, "DoCleanUp()", $"Delete error"));
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Get guild (server) instance using the 'GuildId' configuration value.
        /// </summary>
        private SocketGuild Guild
        {
            get { return Context.Client.GetGuild(Convert.ToUInt64(ConfigurationManager.AppSettings["GuildId"])); }
        }

        /// <summary>
        /// Add role to user.
        /// </summary>
        /// <param name="user">A socket user.</param>
        /// <param name="roleName">Name of the role</param>
        /// <returns></returns>
        internal async Task GrantRoleToUser(SocketUser user, string roleName)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            await (user as IGuildUser).AddRoleAsync(role);
        }

        /// <summary>
        /// Revoke role from user.
        /// </summary>
        /// <param name="user">A socket user.</param>
        /// <param name="roleName">Name of the role to be revoked.</param>
        /// <returns></returns>
        internal async Task RevokeRoleFromUser(SocketUser user, string roleName)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            await (user as IGuildUser).RemoveRoleAsync(role);
        }

        internal async void SetRoleBasedOnRank(string discordId)
        {
            SocketUser socketUser = GetUser(discordId);
            if (socketUser == null) return;

            User user = User.Find(discordId);
            if (user == null) return;

            await RevokeRoleFromUser(socketUser, "Gold Class");
            await RevokeRoleFromUser(socketUser, "Silver Class");
            await RevokeRoleFromUser(socketUser, "Bronze Class");

            if (user.Rank < 4) await GrantRoleToUser(socketUser, "Gold Class");
            else if (user.Rank < 11) await GrantRoleToUser(socketUser, "Silver Class");
            else if (user.Rank < 21) await GrantRoleToUser(socketUser, "Bronze Class");
        }

        /// <summary>
        /// Create challenge role.
        /// </summary>
        /// <param name="user">A socket user to be used when creating the role.</param>
        /// <returns>Role</returns>
        internal async Task<Discord.IRole> CreateChallengeRole(SocketUser user, string roleName)
        {
            Discord.IRole role = null;

            try
            {
                ulong permissionsValue =
                    (ulong)GuildPermission.Speak +
                    (ulong)GuildPermission.SendTTSMessages +
                    (ulong)GuildPermission.SendMessages +
                    (ulong)GuildPermission.ViewChannel +
                    (ulong)GuildPermission.EmbedLinks +
                    (ulong)GuildPermission.Connect +
                    (ulong)GuildPermission.AttachFiles +
                    (ulong)GuildPermission.AddReactions;

                GuildPermissions guildPermissions = new GuildPermissions(permissionsValue);
                role = await (user as IGuildUser).Guild.CreateRoleAsync(roleName, guildPermissions, Color.DarkGreen, true);
            }
            catch //(Exception ex)
            {
            }
            finally
            {
            }

            return role;
        }

        /// <summary>
        /// Allow channel access only to a specific user.
        /// </summary>
        /// <param name="everyOneRole">Role for which to deny access.</param>
        /// <param name="user">The user to be allowed access.</param>
        /// <param name="channel">The channel to set permissions for.</param>
        internal static void SetMatchChannelUserPermission(IRole everyOneRole, SocketUser user, IGuildChannel channel)
        {
            OverwritePermissions permissionsNone = new OverwritePermissions(
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

            OverwritePermissions permissions = new OverwritePermissions(
                // createInstantInvite
                PermValue.Deny,
                // manageChannel
                PermValue.Deny,
                // addReactions
                PermValue.Allow,
                // viewChannel
                PermValue.Allow,
                // sendMessages
                PermValue.Allow,
                // sendTTSMessages
                PermValue.Allow,
                // manageMessages
                PermValue.Deny,
                // embedLinks
                PermValue.Allow,
                // attachFiles
                PermValue.Allow,
                // readMessageHistory
                PermValue.Allow,
                // mentionEveryone
                PermValue.Deny,
                // useExternalEmojis
                PermValue.Allow,
                // connect
                PermValue.Allow,
                // speak
                PermValue.Allow,
                // muteMembers
                PermValue.Deny,
                // deafenMembers
                PermValue.Deny,
                // moveMembers 
                PermValue.Deny,
                // useVoiceActivation
                PermValue.Allow,
                // manageRoles
                PermValue.Deny,
                // manageWebhooks 
                PermValue.Deny);

            channel.AddPermissionOverwriteAsync(everyOneRole, permissionsNone);
            channel.AddPermissionOverwriteAsync(user, permissions);
        }

        /// <summary>
        /// Return socket user instance for the bot.
        /// </summary>
        internal SocketGuildUser BotUser
        {
            get { return Context.Client.GetGuild(Misc.GuildId).GetUser(Misc.BotId) as SocketGuildUser; }
        }

        /// <summary>
        /// Get category ID (ulong) for a named category.
        /// </summary>
        /// <param name="categoryName">Name of a category.</param>
        /// <returns>Category ID</returns>
        internal ulong? CategoryId(string categoryName)
        {
            return Context.Guild.CategoryChannels.FirstOrDefault(category => category.Name.Equals(categoryName))?.Id;
        }

        /// <summary>
        /// Return socket user for specified user name.
        /// </summary>
        /// <param name="userId">User name to search for.</param>
        /// <param name="includeBots">Flag to include/exclude bots in searchy.</param>
        /// <returns>Socket user instance.</returns>
        internal SocketUser GetUser(string discordId, bool includeBots = false)
        {
            SocketUser socketUser = Context.Guild.Users.FirstOrDefault(user => ((user.Username.ToLower() + "#" + user.Discriminator) == discordId.ToLower()));
            if ((socketUser != null) && socketUser.IsBot && !includeBots) socketUser = null;
            return socketUser;
        }

        /// <summary>
        /// Get voice channel for a given channel name.
        /// </summary>
        /// <param name="channelName">Name of channel.</param>
        /// <returns>Voice channel instance</returns>
        internal IVoiceChannel GetVoiceChannel(string channelName)
        {
            foreach (SocketGuildChannel channel in Context.Guild.Channels)
            {
                if (channel is IVoiceChannel)
                {
                    if (channel.Name.ToLower() == channelName.ToLower()) return channel as IVoiceChannel;
                }
            }
            return null;
        }

        /// <summary>
        /// Get text channel for a given channel name.
        /// </summary>
        /// <param name="channelName">Name of channel.</param>
        /// <returns>Text channel instance</returns>
        internal ITextChannel GetTextChannel(string channelName)
        {
            foreach (SocketGuildChannel channel in Context.Guild.Channels)
            {
                if (channel is ITextChannel)
                {
                    if (channel.Name.ToLower() == channelName.ToLower()) return channel as ITextChannel;
                }
            }
            return null;
        }
       
        /// <summary>
        /// Return role for this guilds 'Everyone' role.
        /// </summary>
        internal IRole EveryoneRole
        {
            get { return Context.Client.GetGuild(Misc.GuildId).EveryoneRole; }
        }

        /// <summary>
        /// Create text+voice channel for a match and allow access for 2 named users.
        /// </summary>
        /// <param name="channelName">Name for the channels.</param>
        /// <param name="user1">First user to be given access to the channels</param>
        /// <param name="user2">Second user to be given access to the channels</param>
        /// <returns></returns>
        public async Task<bool> CreateMatchChannelSetAsync(string channelName, string user1, string user2)
        {
            try
            {
                IRole everyoneRole = Context.Client.GetGuild(Misc.GuildId).EveryoneRole;
                IVoiceChannel voiceChannel = await BotUser.Guild.CreateVoiceChannelAsync(channelName);

                await voiceChannel.ModifyAsync(x =>
                {
                    x.UserLimit = 2;
                    x.CategoryId = CategoryId("LADDER");
                    x.Position = 100;
                });

                SetMatchChannelUserPermission(EveryoneRole, GetUser(user1), voiceChannel);
                SetMatchChannelUserPermission(EveryoneRole, GetUser(user2), voiceChannel);

                IGuildChannel textChannel = await BotUser.Guild.CreateTextChannelAsync(channelName);

                await textChannel.ModifyAsync(x =>
                {
                    x.CategoryId = CategoryId("LADDER");
                    x.Position = 100;
                });

                SetMatchChannelUserPermission(EveryoneRole, GetUser(user1), textChannel);
                SetMatchChannelUserPermission(EveryoneRole, GetUser(user2), textChannel);

                return true;
            }
            catch (Exception ex)
            {
                await Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "MiscModule.CreateMatchChannelSetAsync()", "Unable to create channels", ex));
                return false;
            }
        }
        
        /// <summary>
        /// Remove named set of text+voice channels.
        /// </summary>
        /// <param name="channelName">Name of channels.</param>
        /// <returns>True if succesfull.</returns>
        public async Task<bool> RemoveMatchChannelSetAsync(string channelName)
        {
            IVoiceChannel voiceChannel = GetVoiceChannel(channelName);
            IGuildChannel textChannel = GetTextChannel(channelName);

            await Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "MiscModule.RemoveMatchChannelSetAsync()", "Entry"));

            try
            {
                if (voiceChannel != null)
                {
                    await Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "MiscModule.RemoveMatchChannelSetAsync()", "Deleting voice channel"));
                    await voiceChannel.DeleteAsync();
                    voiceChannel = null;
                }

                if (textChannel != null)
                {
                    await Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "MiscModule.RemoveMatchChannelSetAsync()", "Deleting text channel"));
                    await textChannel.DeleteAsync();
                    textChannel = null;
                }

                await Program.Log(new Discord.LogMessage(Discord.LogSeverity.Info, "MiscModule.RemoveMatchChannelSetAsync()", "Exit OK"));

                return true;
            }
            catch (Exception ex)
            {
                await Program.Log(new Discord.LogMessage(Discord.LogSeverity.Error, "MiscModule.RemoveMatchChannelSetAsync()", "Unable to delete channels", ex));
                return false;
            }
        }

        /// <summary>
        /// Helper method to send reply message using a builder instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal async Task Reply(EmbedBuilder builder)
        {
            await ReplyAsync("", false, builder.Build());
        }

        /// <summary>
        /// Get a list of available commands.
        /// </summary>
        /// <param name="messageString"></param>
        /// <returns></returns>
        [Command("help")]
        [Summary("List available commands.")]
        public async Task HelpAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;
         
            User.CreateOrFind(Context.Message.Author);

            // await User.SendDirect(Context.Message.Author, "Hello, Maestro :)");

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            builder.Description = "Available commands that HAL9000 (sometimes) understands";

            builder.AddField("!challenge <@opponent>",
                "Use this to start a new challenge against another pilot. Your opponent must accept your challenge using !accept before the deadline (24 hours), " + 
                "otherwise he/she will automatically go 1 position down the ladder. You can challenge different pilots at the same time but only one challenge per pilot is allowed.");

            builder.AddField("!accept <@challenger>", "Accept a challenge.");

            builder.AddField("!cancel <@opponent>", "Cancelling of a challenge is possible if the challenge hasn't been accepted or has timed out. Only the challenging pilot are allowed to cancel a challenge.");

            builder.AddField("!report <your score> <winners score> <@winner>", "After the match has been played the defeated pilot must report the match results using this command.");

            builder.AddField("!Confirm <@opponent>", "Used by a match winner to confirm the scores reported by the defeated pilot. The ladder will be updated if needed.");

            builder.AddField("!list", "Display a list of recent challenges/matches.");

            builder.AddField("!next", "Display a list of recent challenges/matches that you participate in.");

            builder.AddField("!ladder / !users", "Display current ladder rankings.");

            builder.AddField("!now", "Display date and time in different time zones.");

            builder.AddField("!timezone", "Display/set time zone used when displaying dates and times.");

            builder.AddField("!setrank <user#nnn> rank (ADMIN)", "Set a specific rank for a named user.");
            builder.AddField("!addmatch <user1#nnn> <user2#nnn> (ADMIN)", "Create new match and auto-accept.");
            builder.AddField("!cancelmatch <id> (ADMIN)", "Cancel a match.");
            builder.AddField("!reportmatch <id> <challengeScore> <opponentScore> (ADMIN)", "Report scores and auto-confirm match.");
            builder.AddField("!inactive <id> (ADMIN)", "Simulate inactive user.");

            builder.AddField("!clearmatches  (ADMIN)", "Delete all matches (warning!).");
            builder.AddField("!clearhistory (ADMIN)", "Delete all history (warning!).");
            builder.AddField("!clearusers (ADMIN)", "Delete all users (warning!).");

            builder.AddField("!reset (ADMIN)", "Deletes ALL users, current challenges and match history.");

            await Reply(builder);
        }

        /// <summary>
        /// Setup challenge.
        /// </summary>
        /// <param name="messageString"></param>
        /// <returns></returns>
        [Command("challenge")]
        [Summary("Setup a 1v1 challenge.")]
        public async Task MatchChallengeAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            if (Context.Channel.Name != ConfigurationManager.AppSettings["ChallengeChannelName"])
            {
                //await ReplyAsync($"Please use the '{ConfigurationManager.AppSettings["ChallengeChannelName"]}' channel to setup/accept/cancel challenges.");
                //return;
            }

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            if (Context.Message.MentionedUsers.Count < 1)
            {
                builder.Description = $"\nYou must mention the pilot to be challenged. For example: " + Format.Italics("!challenge @SomeUser#1234)");
                await Reply(builder);
                return;
            }

            if (Context.Message.MentionedUsers.Count > 1)
            {
                builder.Description = $"\nOnly a single pilot (your opponent) can be mentioned when creating a challenge.";
                await Reply(builder);
                return;
            }

            SocketUser challenger = Context.Message.Author;
            SocketUser opponent = Context.Message.MentionedUsers.First();

            if (opponent.IsBot)
            {
                builder.Description = Format.Bold($"{opponent.Username}") + " doesn't want to play Overload!";
                await Reply(builder);
                return;
            }

            User.CreateOrFind(challenger);
            User.CreateOrFind(opponent);

            string challengerId = MatchHandler.UserIdFromSockerUser(challenger);
            string opponentId = MatchHandler.UserIdFromSockerUser(opponent);

            if ((opponentId.ToLower() == challengerId.ToLower()) && !challengerId.ToLower().StartsWith("maestro#"))
            {
                await ReplyAsync("You can't play against yourself!");
                return;
            }

            // Create challenge.
            Match match = MatchHandler.Create(challengerId, opponentId);
            if (match == null)
            {
                await ReplyAsync("I can't setup new challenge due to an unexpected error.");
                return;
            }

            // Match now registered in database.
            builder.Description = "Challenge " + Format.Bold($"{match.ID}") + " has been created. " +
                                  $"Your opponent " + Format.Bold($"{opponent.Username}") + "#" + opponent.Discriminator + " " +
                                  $"now has { Convert.ToInt32(ConfigurationManager.AppSettings["MatchTimeoutHours"])} hours " +
                                  "to accept or reject your challenge. Note that until the match results has been reported " +
                                  "you can cancel the challenge anytime (using the !cancel command).";


            await Reply(builder);
        }

        /// <summary>
        /// Accept challenge.
        /// </summary>
        /// <param name="messageString"></param>
        /// <returns></returns>
        [Command("accept")]
        [Summary("Accept a 1v1 challenge.")]
        public async Task MatchAcceptAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            if (Context.Channel.Name != ConfigurationManager.AppSettings["ChallengeChannelName"])
            {
                //await ReplyAsync($"Please use the '{ConfigurationManager.AppSettings["ChallengeChannelName"]}' channel to setup/accept/cancel challenges :)");
                //return;
            }

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            if (Context.Message.MentionedUsers.Count < 1)
            {
                builder.Description = $"\nYou need to mention the pilot that challenged you. For example: " + Format.Italics("!accept @SomeUser#1234)");
                await Reply(builder);
                return;
            }

            if (Context.Message.MentionedUsers.Count > 1)
            {
                builder.Description = $"\nOnly a single pilot (your opponent) can be mentioned when accepting a challenge.";
                await Reply(builder);
                return;
            }

            SocketUser challenger = Context.Message.MentionedUsers.First();
            SocketUser opponent = Context.Message.Author;

            string challengerId = MatchHandler.UserIdFromSockerUser(challenger);
            string opponentId = MatchHandler.UserIdFromSockerUser(opponent);

            if (challenger.IsBot)
            {
                builder.Description = Format.Bold($"{challenger.Username}") + " never plays Overload (he's too busy).";
                await Reply(builder);
                return;
            }

            User.CreateOrFind(challenger);
            User.CreateOrFind(opponent);

            Match match = MatchHandler.Accept(challengerId, opponentId);
            if (match == null)
            {
                builder.Description = $"\nI was unable to process the Accept due to an unexpected error.";
                await Reply(builder);
                return;
            }

            if (match.ID == 0)
            {
                builder.Description = $"\nNo challenge found.";
                await Reply(builder);
                return;
            }

            if (match.ID < 0)
            {
                builder.Description = $"\nChallenge " + Format.Bold($"{match.ID * -1}") + " was automatically cancelled due to timeout.";
                await Reply(builder);
                return;
            }

            // Create private match channels.
            string matchChannelId = "match-" + match.ID.ToString();
            await CreateMatchChannelSetAsync(matchChannelId, match.Challenger, match.Opponent);

            // Match now registered in database.
            builder.Description = "You accepted challenge " + Format.Bold($"{match.ID}") + " against " + Format.Bold($"{challenger.Username}") + "#" + challenger.Discriminator + ". ";
            builder.Description += $"Please use the private channel set " + Format.Bold($"{matchChannelId}") + " for working out server/level and to report match results.";
            await Reply(builder);
        }

        /// <summary>
        /// Cancel challenge.
        /// </summary>
        /// <param name="messageString"></param>
        [Command("cancel")]
        [Summary("Cancel a 1v1 challenge.")]
        public async Task MatchCancelAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            if (Context.Channel.Name != ConfigurationManager.AppSettings["ChallengeChannelName"])
            {
                // await ReplyAsync($"Please use the '{ConfigurationManager.AppSettings["ChallengeChannelName"]}' channel to setup/accept/cancel challenges :)");
                // return;
            }

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            if (Context.Message.MentionedUsers.Count < 1)
            {
                builder.Description = $"\nYou must mention the pilot that challenged you, for example " + Format.Italics("!cancel @SomeUser#1234).");
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (Context.Message.MentionedUsers.Count > 1)
            {
                builder.Description = $"\nOnly a single pilot (your opponent) can be mentioned when cancelling a challenge.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            SocketUser challenger = Context.Message.Author;
            SocketUser opponent = Context.Message.MentionedUsers.First();

            string challengerId = MatchHandler.UserIdFromSockerUser(challenger);
            string opponentId = MatchHandler.UserIdFromSockerUser(opponent);

            if (opponent.IsBot)
            {
                builder.Description = Format.Bold($"{opponent.Username}") + " never plays Overload (he's too busy!).";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            User.CreateOrFind(challenger);
            User.CreateOrFind(opponent);

            Match match = MatchHandler.Cancel(challengerId, opponentId);
            if (match == null)
            {
                builder.Description = $"\nI was unable to cancel a challenge due to an unexpected error.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (match.ID == 0)
            {
                builder.Description = $"\nNo cancellable match was found for you and your opponent.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            await User.SendDirect(opponent, $"Your match " + Format.Bold($"{ match.ID}") + " against " + Format.Bold(challenger.Username) + "#" + challenger.Discriminator + " has been cancelled.");

            // Match now registered in database.
            builder.Description = "Match " + Format.Bold($"{match.ID}") + " against " +
                                 Format.Bold($"{opponent.Username}") + "#" + MatchHandler.Discriminator(opponentId) + " has been cancelled.\nYour your opponent has been notified.";

            await ReplyAsync("", false, builder.Build());

        }

        /// <summary>
        /// Report match results.
        /// </summary>
        /// <param name="messageString"></param>
        [Command("report")]
        [Summary("Report result of a 1v1 challenge.")]
        public async Task MatchReportAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            if (Context.Channel.Name != ConfigurationManager.AppSettings["ChallengeChannelName"])
            {
                // await ReplyAsync($"Please use the '{ConfigurationManager.AppSettings["ChallengeChannelName"]}' channel to setup/accept/cancel challenges :)");
                // return;
            }

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            if (Context.Message.MentionedUsers.Count < 1)
            {
                builder.Description = $"\nYou must mention your match winner (your opponent), for example: " + Format.Italics("!report 5 13 @winner#1234)");
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (Context.Message.MentionedUsers.Count > 1)
            {
                builder.Description = $"\nOnly the match winner (your opponent) should be mentioned.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            messageString = Misc.RemoveMentions(messageString);

            SocketUser looser = Context.Message.Author;
            SocketUser winner = Context.Message.MentionedUsers.First();

            string looserId = MatchHandler.UserIdFromSockerUser(looser);
            string winnerId = MatchHandler.UserIdFromSockerUser(winner);

            if (winner.IsBot)
            {
                builder.Description = Format.Bold($"{winner.Username}") + " never plays Overload (he prefers Descent 3!).";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if ((User.CreateOrFind(looserId) == null) || (User.CreateOrFind(winnerId) == null))
            {
                builder.Description = $"\nI wasn't able to verify players as registered the database. Please report this issue to the admins.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            string[] split = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
            {
                builder.Description = $"\nYou must provide both loosing and winning scores, for example " + Format.Italics("!report 7 13 @Winner#1234");
                await ReplyAsync("", false, builder.Build());
                return;
            }

            int loosingScore;
            int winningScore;

            try
            {
                loosingScore = Convert.ToInt32(split[0]);
                winningScore = Convert.ToInt32(split[1]);

                if (loosingScore > winningScore)
                {
                    loosingScore = Convert.ToInt32(split[1]);
                    winningScore = Convert.ToInt32(split[0]);
                }
            }
            catch
            {
                builder.Description = $"\nInvalid score values, enter the results like this: " + Format.Italics("!report <your score> <winners score> @winner#1234");
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (loosingScore == winningScore)
            {
                builder.Description = $"\nReporting a draw is disallowed, each match must have a winner.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            // Find a open challenge between the two pilots.
            Match match = MatchHandler.Find(looserId, winnerId, Match.States.Accepted);
            if ((match == null) || (match.ID == 0)) match = MatchHandler.Find(looserId, winnerId, Match.States.Reported);

            if (match == null)
            {
                builder.Description = $"\nSorry but I cannot process your match report right now.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (match.ID == 0)
            {
                builder.Description = $"\nNo challenge exists for you and your opponent.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            // If a draw only the challenger may report the results.
            if ((loosingScore == winningScore) && (looserId != match.Challenger))
            {
                builder.Description = $"\nA draw must be reported by the challenging pilot.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            // Everything seem to be in order. 
            // Now report and lock the match.

            if (match.Challenger == looserId) match = MatchHandler.Report(match.ID, loosingScore, winningScore);
            else match = MatchHandler.Report(match.ID, winningScore, loosingScore);

            if (match == null)
            {
                builder.Description = $"\nUnexpected error trying to process the results.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            // Match now registered in database.
            string draw = "draw with both you and " + Format.Bold($"{winner.Username}") + $" scoring {loosingScore} points.";

            string loss = "loss, your opponent " + Format.Bold($"{winner.Username}") + "#" + MatchHandler.Discriminator(winnerId) + $" won with " + Format.Bold(winningScore.ToString()) + " points and " +
                         $"you scored " + Format.Bold(loosingScore.ToString()) + " points.";

            builder.Description = "Your have reported challenge " + Format.Bold(match.ID.ToString()) + " as a ";

            if (loosingScore == winningScore) builder.Description += draw;
            else builder.Description += loss;

            builder.Description += " " + Format.Bold($"{winner.Username}") + "#" + MatchHandler.Discriminator(winnerId) + " must now confirm the result.";

            await ReplyAsync("", false, builder.Build());
        }

        /// <summary>
        /// Confirm match results.
        /// </summary>
        /// <param name="messageString"></param>
        [Command("confirm")]
        [Summary("Confirm a 1v1 match result.")]
        public async Task MatchConfirmAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            if (Context.Channel.Name != ConfigurationManager.AppSettings["ChallengeChannelName"])
            {
                // await ReplyAsync($"Please use the '{ConfigurationManager.AppSettings["ChallengeChannelName"]}' channel to setup/accept/cancel challenges :)");
                // return;
            }

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            if (Context.Message.MentionedUsers.Count < 1)
            {
                builder.Description = $"\nYour opponent must be mentioned, for example " + Format.Italics("!confirm @SomeUser#1234)");
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (Context.Message.MentionedUsers.Count > 1)
            {
                builder.Description = $"\nOnly mention your opponent when confirming a challenge.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            SocketUser confirmer = Context.Message.Author;
            SocketUser opponent = Context.Message.MentionedUsers.First();

            if (opponent.IsBot)
            {
                builder.Description = Format.Bold($"{opponent.Username}") + " never plays Overload (he's too busy).";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            string confirmerId = MatchHandler.UserIdFromSockerUser(confirmer);
            string opponentId = MatchHandler.UserIdFromSockerUser(opponent);

            User confirmUser = User.CreateOrFind(confirmerId);
            User opponentUser = User.CreateOrFind(opponentId);

            if ((confirmUser == null) || (opponentUser == null))
            {
                builder.Description = $"I wasn't able to verify players as registered users (please advice OSPL admins about this issue!).";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            Match match = MatchHandler.Find(confirmerId, opponentId, Match.States.Reported);

            if (match == null)
            {
                builder.Description = $"\nSorry but I am currently unable to process your confirm request.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (match.ID == 0)
            {
                builder.Description = $"\nNo matching challenge found.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            // Only the winner may confirm the result!
            if (((confirmerId == match.Challenger) && (match.ChallengerScore < match.OpponentScore)) ||
                ((confirmerId == match.Opponent) && (match.OpponentScore < match.ChallengerScore)))
            {
                builder.Description = $"\nYou can only confirm results if you are the winner of the match.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            match = MatchHandler.Confirm(match);

            if (match.ID < 0)
            {
                builder.Description = $"\nI am currently unable to process your confirm request.";
                await ReplyAsync("", false, builder.Build());
                return;
            }

            SetRoleBasedOnRank(match.Challenger);
            SetRoleBasedOnRank(match.Opponent);

            // Match now registered in database.
            builder.Description = "You confirmed the match result for challenge " + Format.Bold($"{match.ID}") + " against " +
                                 $" " + Format.Bold($"{opponent.Username}") + "#" + MatchHandler.Discriminator(opponentId) + ". This challenge is now completed.";

            await ReplyAsync("", false, builder.Build());

            string matchChannelId = "match-" + match.ID.ToString();
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += MatchChannelCleanUp;
            backgroundWorker.RunWorkerAsync(new MatchChannelContextInfo() { Context = this.Context, MatchChannelId = matchChannelId });
        }

        [Command("next")]
        [Summary("List users open challenges and recent matches.")]
        public async Task MatchesNextAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            await MatchesListAsync(MatchHandler.UserIdFromSockerUser(Context.Message.Author), messageString);
        }

        [Command("list")]
        [Summary("List recent challenges.")]
        public async Task MatchesListAsync(string userId = null, [Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            List<Match> matches = MatchHandler.GetMatches(25, Match.States.Blank, userId);
            User user = User.CreateOrFind(Context.Message.Author);

            if ((matches == null) || (user == null))
            {
                await ReplyAsync("Unable to retrieve data from the database.");
                return;
            }

            foreach (Match m in matches)
            {
                string title = m.ID.ToString() + ". " + MatchHandler.UserName(m.Challenger) + "#" + MatchHandler.Discriminator(m.Challenger) + " vs " + MatchHandler.UserName(m.Opponent) + "#" + MatchHandler.Discriminator(m.Opponent);
                string field;

                DateTime created = user.CustomDateTime(m.Created);
                DateTime modified = user.CustomDateTime(m.Modified);

                switch (m.State)
                {
                    case Match.States.Created:
                        TimeSpan timeLeft = m.TimeLeft;
                        int hh = timeLeft.Hours;
                        int mm = timeLeft.Minutes;
                        DateTime before = user.CustomDateTime(DateTime.UtcNow + timeLeft);
                        field = $"New challenged created {Misc.Format(created)}. Awaiting opponent accept before {Misc.Format(before)}.";
                        break;

                    case Match.States.Reported:
                        field = $"Played, scores reported {Misc.Format(modified)}. Winner has to confirm the results.";
                        break;

                    case Match.States.Confirmed:
                        field = $"This match was played {Misc.Format(modified)}.";
                        if (m.ChallengerScore > m.OpponentScore)
                        {
                            field += "\n" + Format.Bold(MatchHandler.UserName(m.Challenger)) + "#" + MatchHandler.Discriminator(m.Challenger);
                            field += " won with scores " + Format.Bold($"{m.ChallengerScore}") + "-" + Format.Bold($"{m.OpponentScore}") + ".";
                        }
                        else
                        {
                            field += "\n" + Format.Bold(MatchHandler.UserName(m.Opponent)) + "#" + MatchHandler.Discriminator(m.Opponent);
                            field += " won with scores " + Format.Bold($"{m.OpponentScore}") + "-" + Format.Bold($"{m.ChallengerScore}") + ".";
                        }
                        break;

                    case Match.States.Cancelled:
                        field = $"Cancelled {Misc.Format(modified)}.";
                        break;

                    case Match.States.TimedOut:
                        field = $"Cancelled by timeout {Misc.Format(modified)}.";
                        break;

                    default:
                        field = $"Challenge accepted {Misc.Format(modified)}, awaiting match report.";
                        break;
                }

                builder.AddField(title, field);
            }

            builder.Description = "Showing open challenges and recent matches";

            string tz = user.TimeZone;
            if (tz.StartsWith("+") || tz.StartsWith("-")) tz = "UTC " + tz + " hours";

            builder.Footer = new EmbedFooterBuilder() { Text = $"Displaying {tz} datetimes (change with '!timezone')." };

            await ReplyAsync("", false, builder.Build());
        }

        [Command("stats")]
        [Summary("List users statistics.")]
        public async Task StatsAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;

            List<User> users = new List<User>();

            if (!String.IsNullOrEmpty(messageString))
            {
                string[] ids = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string id in ids) users.Add(User.Find(id));
            }
            else
            {
                users.Add(User.Find(MatchHandler.UserIdFromSockerUser(Context.Message.Author)));
            }

            foreach (User user in users)
            {
                EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

                if (user != null)
                {
                    int open = 0;
                    List<Match> openMatches1 = MatchHandler.GetMatches(1000, Match.States.Created, user.ID);
                    List<Match> openMatches2 = MatchHandler.GetMatches(1000, Match.States.Accepted, user.ID);
                    if (openMatches1 != null) open += openMatches1.Count;
                    if (openMatches2 != null) open += openMatches2.Count;

                    string result = "";
                    result += $"Rank: " + Format.Bold($"{user.Rank}") + "  ";
                    result += $"Wins: " + Format.Bold($"{user.Wins}") + "  ";
                    result += $"Defeats: " + Format.Bold($"{user.Defeats}") + "  ";
                    result += $"Open challenges: " + Format.Bold($"{open}");

                    if ((user.Wins + user.Defeats) > 0) result += "Last played: " + Format.Bold($"{user.LastPlayed.ToString("yyyy-MM-dd HH:mm")}");
                    builder.AddField(user.ID, result);

                    List<Match> history = user.PlayedMatches();
                    if (history == null)
                    {
                        await ReplyAsync("Unable to retrieve data from the database.");
                        return;
                    }

                    if (history.Count > 0)
                    {
                        result = "";
                        foreach (Match m in history)
                        {
                            if (result.Length > 0) result += "\n";

                            string outcome = "Defeated by";
                            string opponent = m.Opponent;
                            int c = m.ChallengerScore, o = m.OpponentScore;

                            if (m.Challenger == user.ID)
                            {
                                if (m.ChallengerScore > m.OpponentScore) outcome = "Won over";
                            }
                            else
                            {
                                opponent = m.Challenger;
                                int t = o;
                                o = c;
                                c = t;
                                if (m.ChallengerScore < m.OpponentScore) outcome = "Won over";
                            }

                            result += $"{Misc.Format(user.CustomDateTime(m.Created))} {outcome} " +
                                        Format.Bold(MatchHandler.UserName(opponent)) + "#" + MatchHandler.Discriminator(opponent) +
                                        " with scores " + Format.Bold(c.ToString()) + "-" + Format.Bold(o.ToString());
                        }

                        builder.AddField("History", result);
                    }

                    string tz = user.TimeZone;
                    if (tz.StartsWith("+") || tz.StartsWith("-")) tz = "UTC " + tz + " hours";

                    if (history.Count > 0) builder.Footer = new EmbedFooterBuilder() { Text = $"Displaying {tz} datetimes (change with '!timezone')." };

                    await ReplyAsync("", false, builder.Build());
                }
            }
        }

        #region Debugging
        internal class MatchChannelContextInfo
        {
            public SocketCommandContext Context;
            public string MatchChannelId;
        }

        private void MatchChannelCleanUp(object sender, DoWorkEventArgs e)
        {
            MiscModule.SetContext(Context);

            SocketCommandContext context = (e.Argument as MatchChannelContextInfo).Context;
            string matchChannelId = (e.Argument as MatchChannelContextInfo).MatchChannelId;

            Thread.Sleep(5000);

            RemoveMatchChannelSetAsync(matchChannelId).Wait();
        }

        [Command("users")]
        [Alias("ladder")]
        [Summary("users")]
        public async Task UsersAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            List<User> users = User.Users;

            if (users == null)
            {
                await ReplyAsync("Unable to get user list from the database.");
                return;
            }

            if (users.Count == 0)
            {
                await ReplyAsync("No user registered (should never happen!).");
                return;
            }

            int n = 0;
            foreach (User user in users)
            {
                builder.AddField(user.Rank.ToString() + ".", Format.Bold(MatchHandler.UserName(user.ID)) + "#" + MatchHandler.Discriminator(user.ID), true);
                if (++n > 24) break;
            }

            builder.Description = "Current ladder rankings (first 25 positions)\n";

            await Reply(builder);
        }

        [Command("setrank")]
        [Summary("setrank")]
        public async Task SetRankAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            string[] split = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2)
            {
                await ReplyAsync("Need user ID and rank.");
                return;
            }

            string userId = split[0].Trim();
            string rankStr= split[1].Trim();

            if (!userId.Contains("#"))
            {
                await ReplyAsync("User ID needs '#'");
                return;
            }

            int rank;

            try
            {
                rank = Convert.ToInt32(rankStr);
            }
            catch
            {
                await ReplyAsync("Invalid rank.");
                return;
            }

            User.SetRank(userId, rank);
            SetRoleBasedOnRank(userId);
        }

        [Command("delete")]
        [Summary("delete")]
        public async Task DeleteAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) return;

            string[] split = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 1)
            {
                await ReplyAsync("Need user ID.");
                return;
            }

            string userId = split[0].Trim();

            if (!userId.Contains("#"))
            {
                await ReplyAsync("User ID needs '#'");
                return;
            }

            User.Delete(userId);
        }

        [Command("addmatch")]
        [Summary("addmatch")]
        public async Task CreateMatchAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            if (messageString == null) 
            {
                await ReplyAsync("Need 2 user IDs.");
                return;
            }

            string[] split = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2)
            {
                await ReplyAsync("Need 2 users.");
                return;
            }

            string challengerId = split[0].Trim();
            string opponentId = split[1].Trim();

            if (!challengerId.Contains("#") || !challengerId.Contains("#"))
            {
                await ReplyAsync("User IDs are missing '#'");
                return;
            }

            User.CreateOrFind(challengerId);
            User.CreateOrFind(opponentId);

            Match match = MatchHandler.Create(challengerId, opponentId);
            if ((match != null) && (match.ID > 0))
            {
                MatchHandler.Accept(match.Challenger, match.Opponent);

                await ReplyAsync($"Created and accepted match {match.ID}.");
                return;
            }

            await ReplyAsync($"Unable to create match.");
        }

        [Command("cancelmatch")]
        [Summary("cancelmatch")]
        public async Task CancelMatchAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
              (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
              (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            if (messageString == null) return;

            int matchId;

            try
            {
                matchId = Convert.ToInt32(messageString);
            }
            catch
            {
                await ReplyAsync("Invalid match ID.");
                return;
            }

            Match match = MatchHandler.Find(matchId);
            if ((match == null) || (match.ID < 0)) return;

            MatchHandler.Cancel(match.Challenger, match.Opponent);

            await ReplyAsync($"Cancelled match {match.ID}.");            
        }

        [Command("reset")]
        [Summary("reset")]
        public async Task ResetMatchAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
              (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
              (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            User.DeleteAll();
            MatchHandler.ClearHistory();
            MatchHandler.ClearMatches();

            return;
            int n = 0;
            for (int i = 1; i < 11; i++)
            {
                string userId = $"Test#{i}";
                User.CreateOrFind(userId);
                User.SetRank(userId, i);
                SetRoleBasedOnRank(userId);
                n = i;
            }

            User.CreateOrFind(KAHA);
            User.SetRank(KAHA, ++n);
            SetRoleBasedOnRank(KAHA);

            User.CreateOrFind(Dreawus);
            User.SetRank(Dreawus, ++n);
            SetRoleBasedOnRank(Dreawus);

            User.CreateOrFind(Maestro);
            User.SetRank(Maestro, ++n);
            SetRoleBasedOnRank(Maestro);

            User.CreateOrFind(Hiflier);
            User.SetRank(Hiflier, ++n);
            SetRoleBasedOnRank(Hiflier);

            User.CreateOrFind(JoBoOne);
            User.SetRank(JoBoOne, ++n);
            SetRoleBasedOnRank(JoBoOne);

            await ReplyAsync("Reset complete.");
        }

        [Command("clearmatches")]
        [Summary("clearmatches")]
        public async Task ClearMatchesAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            MatchHandler.ClearMatches();
        }

        [Command("clearhistory")]
        [Summary("clearhistory")]
        public async Task ClearHistoryAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
               (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            MatchHandler.ClearHistory();
        }

        [Command("reportmatch")]
        [Summary("reportmatch")]
        public async Task ReportMatchAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
                (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
                (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            if (messageString == null) return;

            string[] split = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 3)
            {
                await ReplyAsync("Need a match ID and 2 scores.");
                return;
            }

            int matchId, score1, score2;

            try
            {
                matchId = Convert.ToInt32(split[0]);
            }
            catch
            {
                await ReplyAsync("Invalid match ID.");
                return;
            }

            try
            {
                score1 = Convert.ToInt32(split[1]);
            }
            catch
            {
                await ReplyAsync("Invalid challenger score.");
                return;
            }

            try
            {
                score2 = Convert.ToInt32(split[2]);
            }
            catch
            {
                await ReplyAsync("Invalid opponent score.");
                return;
            }

            Match match = MatchHandler.Find(matchId);
            if ((match == null) || (match.ID < 0))
            {
                await ReplyAsync($"Match {matchId} could not be found .");
                return;
            }

            match = MatchHandler.Report(matchId, score1, score2);
            if ((match != null) && (match.ID > 0))
            {
                match = MatchHandler.Confirm(match);
                if ((match != null) && (match.ID > 0))
                {
                    SetRoleBasedOnRank(match.Challenger);
                    SetRoleBasedOnRank(match.Opponent);
                    await ReplyAsync($"Match {match.ID} reported and confirmed.");
                }
                else
                {
                    await ReplyAsync($"Match {matchId} reported OK but couldn't be confirmed.");
                }
            }
            else
            {
                await ReplyAsync($"Match {matchId} could not be reported.");
            }
        }
 
        [Command("inactive")]
        [Summary("inactive")]
        public async Task InactiveAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if ((MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) &&
                (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != KAHA.ToLower()) &&
                (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Dreawus.ToLower())) return;

            if (messageString == null) return;

            string[] userList = messageString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string userId in userList)
            {
                User user = User.Find(userId);
                if ((user != null) && (!String.IsNullOrEmpty(user.ID))) MatchHandler.LowerRank(user);
            }
        }
         #endregion
    }
}