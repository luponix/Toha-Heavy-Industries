using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordOSPLBot
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        private static string[] quotes = null;
        private static string[] halQuotes = null;
        private static string[] greetings = null;

        private const string Maestro = @"Maestro#4825";

        internal static SocketCommandContext GlobalCommandContext = null;
        internal static bool GlobalCommandContextAquired = false;

        [Command("test")]
        [Summary("test.")]
        public async Task TestAsync()
        {
            if (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) return;

            EmbedBuilder builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            //Context.Guild.GetTextChannel("challenge").GetTextChannel("id").SendMessageAsync("message")

            //Context.Client.GetGuild(1).GetTextChannel("asd").

            builder.AddField(Format.Bold("Title"),             "This is the title - repeat - repeat - repeat - repeat - repeat - repeat -1");

            builder.AddField("1. Dreawus", "5 wins - 7 defeats, last played 2019-10-10.");
            builder.AddField("2. Maestro", "11 wins - 9 defeats, last played 2019-10-10.");

            builder.Author = new EmbedAuthorBuilder() { Name = "Myself - repeat - repeat - repeat - repeat - repeat - repeat - 1" };
            builder.Footer = new EmbedFooterBuilder() { Text = "This is a footer" };

            await ReplyAsync("", false, builder.Build());
        }

        internal static void SendTextMessage(DiscordSocketClient botClient, string channelName, string message)
        {
            foreach (SocketGuildChannel channel in botClient.GetGuild(Misc.GuildId).Channels)
            {
                if (channel is ITextChannel)
                {
                    if (channel.Name.ToLower() == channelName.ToLower()) (channel as SocketTextChannel).SendMessageAsync(message);
                }
            }
        }

        private static ulong GetTextChannelId(SocketCommandContext context, string channelName)
        {
            foreach (SocketGuildChannel channel in context.Guild.Channels)
            {
                if (channel is ITextChannel)
                {
                    if (channel.Name.ToLower() == channelName.ToLower()) return channel.Id;
                }
            }
            return 0;
        }

        internal static void PeriodicBackgroundJobs(DiscordSocketClient botClient)
        {
            if (GlobalCommandContextAquired) MatchModule.DoCleanUp(GlobalCommandContext);
        }

        internal static void SetContext(SocketCommandContext context)
        {
            if (!GlobalCommandContextAquired && (GlobalCommandContext == null))
            {
                GlobalCommandContext = context;
                GlobalCommandContextAquired = true;
                Program.Log(new LogMessage(LogSeverity.Info, "Background", "Global context aquired"));
            }
        }

        internal static void SendMessage(string channelName, string message)
        {
            if (!GlobalCommandContextAquired) return;
            (GlobalCommandContext.Client.GetChannel(GetTextChannelId(GlobalCommandContext, channelName)) as SocketTextChannel).SendMessageAsync(message);
        }

        [Command("clearchannel")]
        [Summary("Delete all messages in channel.")]
        public async Task ClearChannelAsync()
        {
            MiscModule.SetContext(Context);

            if (MatchHandler.UserIdFromSockerUser(Context.Message.Author).ToLower() != Maestro.ToLower()) return;

            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(9999).FlattenAsync();
            int count = messages.Count();

            //await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);

            foreach (IMessage message in messages)
            {
                ((ITextChannel)Context.Channel).DeleteMessageAsync(message).Wait();
                await Task.Delay(500);
            }

            const int delay = 5000;
            IUserMessage m = await ReplyAsync($"Deleted {count} messages - this message will selfdestruct in 5 seconds.");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }


        [Command("hello")]
        [Alias("hi", "hey", "morning", "goodmorning", "goodnight", "goodnite", "nite", "bonjour", "dav", "hej")]
        [Summary("Greeting.")]
        public async Task HelloAsync()
        {
            MiscModule.SetContext(Context);

            User.CreateOrFind(Context.Message.Author);

            if (greetings == null) greetings = File.ReadAllLines("greetings.txt");
            if (greetings.Length < 2)
            {
                await ReplyAsync("Yo!");
                return;
            }

            int a = new Random().Next(0, greetings.Length - 1);

            await ReplyAsync("Hi " + Context.Message.Author.Username + "! " + greetings[a]);
        }
        
        [Command("lucky")]
        [Summary("Who is lucky?")]
        public async Task LuckyAsync()
        {
            MiscModule.SetContext(Context);

            User.CreateOrFind(Context.Message.Author);

            int i = 0;
            int u = new Random().Next(1, Context.Guild.MemberCount);

            foreach (SocketUser user in Context.Guild.Users)
            {
                if (++i == u)
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = Misc.EmbedColor,
                        Description = "When playing Overload, " + Format.Bold($"{user.Username}") + " is always lucky!"
                    };

                    await ReplyAsync("", false, builder.Build());
                    return;
                }
            }

            await ReplyAsync("Nobody is lucky today.");
        }

        [Command("open")]
        [Summary("Open those bay doors!")]
        public async Task OpenBayDoorsAsync([Remainder] string message = null)
        {
            MiscModule.SetContext(Context);

            User.CreateOrFind(Context.Message.Author);
            if (!String.IsNullOrEmpty(message) && message.ToLower().Contains("door")) await ReplyAsync("I’m sorry Dave, I’m afraid I can’t do that. We're not in space right now.");
        }

        [Command("beer")]
        [Summary("Need a beer.")]
        public async Task BeerAsync()
        {
            MiscModule.SetContext(Context);

            User.CreateOrFind(Context.Message.Author);
            var builder = new EmbedBuilder()
            {
                Color = Misc.EmbedColor,
                Description = "A Masterbrew for " + Format.Bold(Context.Message.Author.Username) + " coming right up!"
            };

            await ReplyAsync("", false, builder.Build());
        }

        [Command("quote")]
        [Summary("Today's quote.")]
        public async Task QuoteAsync()
        {
            MiscModule.SetContext(Context);

            User.CreateOrFind(Context.Message.Author);
            if (quotes == null) quotes = File.ReadAllLines("quotes.txt");
            if (quotes.Length < 3)
            {
                await ReplyAsync("No quotes today.");
                return;
            }

            int a, b = new Random().Next(2, quotes.Length - 1);

            while (!quotes[b].StartsWith("-")) b--;
            a = b - 1;
            while ((a > 0) && !String.IsNullOrEmpty(quotes[a])) a--;

            string result = "";
            for (int i = a; i <= b; i++)
            {
                if (i == b)
                {
                    result += "\n*" + quotes[i] + "*";
                }
                else
                {
                    string temp = quotes[i] + " ";
                    while (temp.Contains("  ")) temp = temp.Replace("  ", " ");
                    result += temp;
                }
            }

            var builder = new EmbedBuilder()
            {
                Color = Misc.EmbedColor,
                Description = result
            };

            await ReplyAsync("", false, builder.Build());
        }

        [Command("hal")]
        [Alias("hal9000", "computer", "speak", "say")]
        [Summary("Today's HAL9000 quote.")]
        public async Task Hal9000QuoteAsync()
        {
            MiscModule.SetContext(Context);

            User.CreateOrFind(Context.Message.Author);
            if (halQuotes == null) halQuotes = File.ReadAllLines("hal9000.txt");
            if (halQuotes.Length < 3) return;

            int a = new Random().Next(1, halQuotes.Length - 1);

            var builder = new EmbedBuilder()
            {
                Color = Misc.EmbedColor,
                Description = halQuotes[a]
            };

            await ReplyAsync("", false, builder.Build());
        }

        [Command("now")]
        [Summary("Shows datetime in different time zones.")]
        public async Task NowAsync()
        {
            MiscModule.SetContext(Context);

            User user = User.CreateOrFind(Context.Message.Author);
            if (user == null) return;

            // <yyyy-mm-dd> <hh:mm>
            DateTime utcDateTime;
            EmbedBuilder builder;

            //utcDateTime = Common.GetNetworkTime();
            utcDateTime = DateTime.UtcNow;
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            builder = new EmbedBuilder() { Color = Misc.EmbedColor };

            string tz = user.TimeZone;
            if (tz.StartsWith("+") || tz.StartsWith("-")) tz = "UTC " + tz + " hours";

            string result = $"\nYour time is {user.CustomDateTime(utcDateTime).ToString("yyyy-MM-dd HH:mm")} (based on {tz}).\n";

            result +=
                $"\nCET: {Misc.CET(utcDateTime).ToString("yyyy-MM-dd HH:mm")}" +
                $"\nGMT: {Misc.GMT(utcDateTime).ToString("yyyy-MM-dd HH:mm")}" +
                $"\nPST: {Misc.PST(utcDateTime).ToString("yyyy-MM-dd HH:mm")}" +
                $"\nMST: {Misc.MST(utcDateTime).ToString("yyyy-MM-dd HH:mm")}" +
                $"\nEST: {Misc.EST(utcDateTime).ToString("yyyy-MM-dd HH:mm")}";

            builder.Description = result + "\n\nNote: Use '!timezone' to change time zone.";

            await ReplyAsync("", false, builder.Build());
        }

        [Command("timezone")]
        [Summary("Set users time zone.")]
        public async Task SetTimeZoneAsync([Remainder] string parms = null)
        {
            MiscModule.SetContext(Context);

            if (Context.Message.Author.IsBot) return;
            if (User.CreateOrFind(Context.Message.Author) == null) return;

            string userId = Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator;

            string tz = User.GetTimeZone(userId).Trim();

            if (String.IsNullOrEmpty(parms))
            {
                tz = User.GetTimeZone(userId).Trim();
                if (tz.StartsWith("+") || tz.StartsWith("-")) tz = "UTC " + tz + " hours";

                await ReplyAsync($"Your time zone is {tz}.");
                return;
            }

            tz = parms.ToUpper().Replace("HOURS", "").Replace("HOUR", "").Replace("H", "").Trim();

            if (tz.StartsWith("+") || parms.StartsWith("-"))
            {
                int offset = -100;
                try
                {
                    offset = Convert.ToInt32(parms, CultureInfo.InvariantCulture);
                }
                catch
                {
                }

                if ((offset < -23) || (offset > 23))
                {
                    await ReplyAsync("Invalid UTC offset. Allowed values are (+/-)23 hours.");
                    return;
                }
            }
            else
            {
                if (!Misc.IsValidTimeZone(tz))
                {
                    await ReplyAsync("Specify a time zone code or +/- value (hours) relative to UTC time.");
                    return;
                }
            }

            if (User.SetTimeZone(Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator, tz) == false)
            {
                await ReplyAsync("Unable to set time zone.");
            }
            else
            {
                if (tz.StartsWith("+") || tz.StartsWith("-")) tz = "UTC " + tz + " hours";

                await ReplyAsync($"Your time zone has been set to {tz}.");
            }
        }
    }
}
