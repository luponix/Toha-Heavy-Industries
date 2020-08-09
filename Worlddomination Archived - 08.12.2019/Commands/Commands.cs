using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Helix.Models.Users;

namespace Worlddomination.Commands
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("gameid")]
        [Summary("returns the gameid of a live channelname")]
        public async Task GetGameId( string channel )
        {
            if ((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("luponix#5950") 
             || (Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("CHILLY_BUS#0001"))
            {
                List<string> userlog = new List<string>();
                userlog.Add(channel);
                var stream = await Program.API.Helix.Streams.GetStreamsAsync(userLogins: userlog);
               
                    string gameid = stream.Streams[0].GameId;
                    await Context.Channel.SendMessageAsync(channel + " is streaming " + gameid);
               

            }
            else
            {
                await Context.Channel.SendMessageAsync("you are not the pathfinder main but if you want i can generate a minesweeper grid for you");
            }
        }

        // 1. target server
        // 2. target channel
        // 3. gameid
        // 4. intervall
        // 5. limit
        [Command("SetupTracker")]
        [Summary("Sets up a Monitor object for streams")]
        public async Task RequestMonitor(string target_server, string target_channel, string gameid, int intervall, int limit )
        {
            if ((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("luponix#5950")
             || (Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("CHILLY_BUS#0001"))
            {

                Program.smh.Add(target_server, target_channel, gameid, intervall, limit);
                
                await Context.Channel.SendMessageAsync("Trying to setup ");


            }
            else
            {
                await Context.Channel.SendMessageAsync("you are not the pathfinder main but if you want i can generate a minesweeper grid for you");
            }
        }

        [Command("upload")]
        [Summary("saves channel and guild id to Broadcast")]
        public async Task RegisterChannelId()
        {
            if ((Context.Message.Author.Username+"#"+Context.Message.Author.Discriminator).Equals("luponix#5950"))
            {
                /*
                ulong c_id = Context.Channel.Id;
                ulong g_id = Context.Guild.Id;
                string c_name = Context.Channel.Name;
                string g_name = Context.Guild.Name;
                Data.Gate.SaveBroadcastLocations(c_id.ToString());
                Data.Gate.SaveBroadcastLocations(g_id.ToString());
                await Context.Channel.SendMessageAsync("Success: g-"+g_name+"| c-"+c_name); */
                Console.WriteLine("Received Upload request");
                Imgur.Imgur imgur = new Imgur.Imgur();
                
                 //   https://icatcare.org/app/uploads/2018/07/Thinking-of-getting-a-cat.png
                string upload = imgur.GetImageUrl(@"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\imgurTest.png");
                await Context.Channel.SendMessageAsync(upload);

            }
            else
            {
                await Context.Channel.SendMessageAsync((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator));
            }//491757
        }
        /*
        [Command("clearchannel")]
        [Summary("Delete all messages in channel.")]
        public async Task ClearChannelAsync([Remainder] string messageString = null)
        {
            MiscModule.SetContext(Context);

            if (Match.DiscordUserId(Context.Message.Author).ToLower() != Maestro.ToLower()) return;

            int hours = 0;
            int mins = 0;
            int secs = 0;

            if (!String.IsNullOrEmpty(messageString))
            {
                bool error = false;
                try
                {

                    foreach (string parm in messageString.Split(" ".ToCharArray()))
                    {
                        if (parm.ToLower().EndsWith("h")) hours = Convert.ToInt32(parm.Substring(0, parm.Length - 1));
                        if (parm.ToLower().EndsWith("m")) mins = Convert.ToInt32(parm.Substring(0, parm.Length - 1));
                        if (parm.ToLower().EndsWith("s")) secs = Convert.ToInt32(parm.Substring(0, parm.Length - 1));
                    }
                }
                catch
                {
                    error = true;
                }

                if (error) return;
            }

            IEnumerable<IMessage> all = await Context.Channel.GetMessagesAsync(9999).FlattenAsync();
            IEnumerable<IMessage> delete = all.Where(x => (DateTime.Now - x.Timestamp) > new TimeSpan(hours, mins, secs));

            int count = delete.Count();
            if (count > 0) await ((ITextChannel)Context.Channel).DeleteMessagesAsync(delete);

            await Task.Delay(200);
            await ((ITextChannel)Context.Channel).DeleteMessageAsync(Context.Message);

            IUserMessage m = await ReplyAsync($"Deleted {count} messages. This message will selfdestruct right now.");
            await Task.Delay(1000);
            await m.DeleteAsync();
        }
        */
        /*
        [Command("GetStreams")]
        [Summary("hopefully sends an API request for a game id")]
        public async Task GetStreams()
        {
            if ((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("luponix#5950"))
            {
                ApiRequests.RequestHandler ApiRequestHandler = new ApiRequests.RequestHandler();
                
                string result = await ApiRequestHandler.GetStreamsForGameIdsAsync();

                await Context.Channel.SendMessageAsync(result);

            }
            else
            {
                await Context.Channel.SendMessageAsync((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator));
            }//491757
        }*/



        // add command to get guild id and channel id as broadcast targets

        // add command to add streamers to track

        // add command to create a minesweeper field

    }
}
