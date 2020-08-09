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

        [Command("version")]
        [Summary("returns current version")]
        public async Task Version()
        {
            Discord.EmbedBuilder embed = new EmbedBuilder
            {
                Title = "version "+Program.version,
            };
            embed.WithColor(Color.Blue);    
           // embed.WithCurrentTimestamp();
            Discord.Embed embedded = embed.Build();

            await Context.Channel.SendMessageAsync(embed: embedded);
        }
        [Command("helpme")]
        [Summary("")]
        public async Task helpME()
        {
            var a = await Program.API.V5.Streams.GetLiveStreamsAsync(game: "Doom");
            foreach( TwitchLib.Api.V5.Models.Streams.Stream b in a.Streams)
            {
                Console.WriteLine(b.Channel.DisplayName);
            }
        }

        [Command("sim")]
        [Summary("returns 25 test embeddeds")]
        public async Task test( string game_ids)
        {
            List<string> game_id = new List<string>();
            List<string> user_ids = new List<string>();
            List<string> n_name = new List<string>();
            List<string> n_profile = new List<string>();

            game_id.Add(game_ids);
            var request =  await Program.API.Helix.Streams.GetStreamsAsync( first: 25, gameIds: game_id);
            foreach (Stream stream in request.Streams)
            {
                user_ids.Add(stream.UserId);
            }

            var request2 =  await Program.API.Helix.Users.GetUsersAsync(ids: user_ids);            
            foreach (User user in request2.Users)
            {
                if (!user.DisplayName.Equals("luponix3"))
                {
                    n_profile.Add(user.ProfileImageUrl);
                    n_name.Add(user.DisplayName);
                }
            }
            int i = 0;
            foreach (Stream stream in request.Streams)
            {  
                try
                {
                   
                    string thumUrl = stream.ThumbnailUrl;
                    thumUrl = thumUrl.Replace("{width}", "480");
                    thumUrl = thumUrl.Replace("{height}", "270");

                    string url = "https://www.twitch.tv/" + n_name[i];

                    Discord.EmbedAuthorBuilder author = new EmbedAuthorBuilder
                    {
                        Name = n_name[i],
                        IconUrl = n_profile[i],
                    };

                    Discord.EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = stream.Title,
                        Author = author,
                        ThumbnailUrl = n_profile[i],
                    };
                    embed.AddField(n_name[i] + " is streaming " + Format.Bold(game_ids), "under " + url);
                    embed.ImageUrl = thumUrl;
                    embed.WithColor(Color.Blue);
                    embed.WithUrl(url);
                    embed.WithCurrentTimestamp();
                    Discord.Embed embedded = embed.Build();

                    await Context.Channel.SendMessageAsync(embed: embedded);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Encountered Exception: " + e);
                }
                i++;
            }

            await Context.Channel.SendMessageAsync("Done");
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
