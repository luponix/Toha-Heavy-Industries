using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
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
        // public variables that should prbably be in program but whatever
        public static IReadOnlyCollection<SocketGuildUser> users;

       

        [Command("help")]
        [Summary("gives an overview for the commands")]
        public async Task HelpCommand()
        {
            Discord.EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Commands: ",
            };
            embed.AddField("General Commands ",
                Format.Code("-meme") + "\n   memes ? \n\n" +
                Format.Code("-whitelist <streamer>") + "\n   adds a streamer to a Descent stream whitelist \n\n" +
                Format.Code("-force <category> <server> <channel>") + "\n   shows the top 5 streams for a twitch category \n\n" +
                Format.Code("-minesweeper <x> <y> <bombs>") + "\n  creates a minesweeper grid \n\n" +
                Format.Code("-version") + "\n   returns the version number \n\n\u200B"
                );

            embed.AddField("Restricted Commands ",
                Format.Code("-showresults <int>") + "\n   returns currently known live channels for a tracker instance \n\n" +
                Format.Code("-setuptracker <server> <channel> <category> <intervall> <limit>") + "\n   creates a new trackerinstance for a given categ. in a channel \n\n" +
                Format.Code("-ban <streamer>") + "\n   prevents a streamer from getting broadcasted \n\n" +
                Format.Code("-gameid <streamer>") + "\n   returns the gameid of a live channelname \n\n" +
                Format.Code("-getid <discordname>") + "\n   return the snowflake for a full discord name \n\n"
                );
            embed.WithColor(Color.Blue);
            embed.WithCurrentTimestamp();  
            Discord.Embed embedded = embed.Build();

            await Context.Channel.SendMessageAsync(embed: embedded);
        }

        

        // **``** MARK SQL 
        [Command("isBanned")]
        [Summary("null")]
        public async Task GenericClass1(string streamer)
        {
            if ( Permissions.IsUserAuthorized( (long)Context.Message.Author.Id, 1))
            {
                await Context.Channel.SendMessageAsync(Program.smh.banlist.Contains(streamer).ToString());
            }
            else
            {
                await Context.Channel.SendMessageAsync("Insufficient permissions");
            }
        }


        public static string[] memefileNames;

        [Command("meme")]
        [Summary("returns a meme")]
        public async Task returnMeme()
        {
            try
            {
                if (Directory.Exists(Data.Paths.meme_directory))
                {
                    memefileNames = Directory.GetFiles(Data.Paths.meme_directory);
                    if (memefileNames.Length > 0)
                    {
                        await Context.Channel.SendFileAsync(memefileNames[new Random().Next(memefileNames.Length)]);
                    }
                }
                else
                {
                    Console.WriteLine("[ERROR] : meme directory doesnt exist");
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine(ex);
            }
            
        }



        [Command("minesweeper")]
        [Summary("creates a minesweeper field ")]
        public async Task minesweeper( int x, int y, int bombs )
        {
            if( x*y*12 < 2000 )
            {
                if( x < 1 || y < 1 || bombs < 1)
                {
                    await Context.Channel.SendMessageAsync("Invalid arguments");
                    return;
                }
                else
                {
                    if( x > 20 )
                    {
                        await Context.Channel.SendMessageAsync("width higher than 20 is not allowed since it would split the entries into different lines");
                        return;
                    }
                    else
                    {
                        Minesweeper m = new Minesweeper(x, y, bombs);
                        await Context.Channel.SendMessageAsync(m.GenerateMinesweeperField());
                    }
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("invalid size, this command currently has to comply with the 2000 letter per message limit");
                return;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamer"></param>
        /// <returns></returns>
        [Command("whitelist")]
        [Summary("marks someone as broadcastable, is only relevant for the descent streams")]
        public async Task addThatGuy(string streamer)
        {
            if( Program.smh.descent_streamer_whitelist.Contains(streamer) )
            {
                await Context.Channel.SendMessageAsync( streamer + " is already whitelisted");
            }
            else
            {
                Program.smh.descent_streamer_whitelist.Add(streamer);
                Data.Gate.Save("Whitelist", Program.smh.descent_streamer_whitelist);

                var emoji = new Emoji("☑");
                await Context.Message.AddReactionAsync(emoji);
            }
        }

        [Command("getId")]
        [Summary("return the snowflake of a given username")]
        public async Task GetUserId(string nickname)
        {
            Console.WriteLine("-getID issued");
            ulong author_id = Context.Message.Author.Id;
            string author = Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator;
            if (author.Equals("luponix#5950")
             || author.Equals("CHILLY_BUS#0001")
             || author.Equals("Yoshimitsu#8541")
             || author.Equals("Hunter#5276")
             || author.Equals("DescentMax7930#9275")
             )
            {

                if(users == null)
                {
                    Console.WriteLine("Populating Userdata from " + Context.Guild.Name);
                    users = Context.Guild.Users;
                }
                
                //var users = Context.Guild.Users;
                //find the user

                foreach( var user in users )
                {
                    if(nickname.Equals(user.ToString()))
                    {
                        Console.WriteLine(" -getId: found equal nickname");
                        await Context.Channel.SendMessageAsync("id: ```"+user.Id+"```");
                        return;
                    }
                }
                await Context.Channel.SendMessageAsync(" did not find a user with this name ");

            }
            else
            {
                Console.WriteLine("-getID failed: Insufficient user permissions");
                await Context.Channel.SendMessageAsync("getID failed: Insufficient user permissions");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamer"></param>
        /// <returns></returns>
        [Command("ban")]
        [Summary("let me have none of that shit")]
        public async Task BanThatFucker(string streamer)
        {
            ulong author_id = Context.Message.Author.Id;
            string author = Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator;
            if (author.Equals("luponix#5950")
             || author.Equals("CHILLY_BUS#0001")
             || author.Equals("Yoshimitsu#8541")
             || author.Equals("Hunter#5276")
             || author.Equals("DescentMax7930#9275")
             || author.Equals("derhass#6611")
             || author.Equals("Phyrexy#1281")
             )
            {
                if(Data.Gate.Load("Streamers").Contains(streamer))
                {
                    await Context.Channel.SendMessageAsync("streamer is already banned");
                }
                else
                {
                    Data.Gate.AddAndSaveBanlist(streamer);
                    Program.smh.banlist = Data.Gate.Load("Streamers");
                    var emoji = new Emoji("☑");
                    await Context.Message.AddReactionAsync(emoji);
                }

            }
            else
            {
                // build an actual permission system at some point
                await Context.Channel.SendMessageAsync("Insufficient permissions");
            }
        }

        [Command("version")]
        [Summary("let me have none of that shit")]
        public async Task Version()
        {
            Discord.EmbedBuilder embed = new EmbedBuilder
            {
                Title = "version " + Program.version,
            };
            embed.WithColor(Color.Blue);
            Discord.Embed embedded = embed.Build();

            await Context.Channel.SendMessageAsync(embed: embedded);
        }

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


       



        [Command("force")]
        [Summary("forces ")]
        public async Task force( string game_id, string server, string channel)
        {
            await Context.Channel.SendMessageAsync(". . .");
            TwitchLib.Api.V5.Models.Streams.Stream[] n_streams = new TwitchLib.Api.V5.Models.Streams.Stream[5];
           
            var request = Program.API.V5.Streams.GetLiveStreamsAsync(game: game_id, limit: 5).Result;


            foreach( var stream in request.Streams)
            {

                    DateTime dateTimeNow = DateTime.Now;
                    int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
                    string name = sec.ToString();

                    string thumUrl = stream.Preview.Large;


                    string[] paths = { @"E:\TohaHeavyIndustries Images Archive", name + ".jpg" };
                    string fullPath = System.IO.Path.Combine(paths);

                    Imgur.Download.DownloadUrl(thumUrl, fullPath); 

                    Imgur.Imgur imgur = new Imgur.Imgur();
                    string finallink = imgur.GetImageUrl(fullPath);

                    string url = "https://www.twitch.tv/" + stream.Channel.DisplayName;


                    var user_videos = await Program.API.V5.Channels.GetChannelVideosAsync(stream.Channel.Id, limit: 1);
                    Discord.EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = stream.Channel.Status,
                        //Author = author,
                        ThumbnailUrl = stream.Channel.Logo,
                    };
                    embed.AddField(stream.Channel.DisplayName + " is streaming " + Format.Bold(game_id), "under " + url);
                    embed.ImageUrl = finallink;
                    embed.WithColor(Color.Blue);
                    embed.WithUrl(url);
                    embed.AddField("This stream will be archived under ", user_videos.Videos[0].Url);
                    embed.WithCurrentTimestamp();
                    Discord.Embed embedded = embed.Build();

                    Misc.SendEmbedWithoutContext("", embedded, channel, server);
            }

           
            return;

        }

        

        [Command("getUserVideos")]
        [Summary("returns 25 test embeddeds")]
        public async Task getData(string channel_name, int amount)
        {
            List<string> id = new List<string>();
            id.Add(channel_name);
            var request = await Program.API.Helix.Users.GetUsersAsync(logins: id);
            
            var request2 = await Program.API.V5.Streams.GetStreamByUserAsync(request.Users[0].Id);
            //var request = await Program.API.V5.Streams.GetLiveStreamsAsync(channelList: id);
            var user_videos = await  Program.API.V5.Channels.GetChannelVideosAsync(channelId: request.Users[0].Id, limit: amount);
            string a = "";
            string b = "";
            string c = "";
            string d = "";
            string e = "";

            string g = "STREAMID: "+request2.Stream.Id.ToString();
            foreach ( var z in user_videos.Videos)
            {
                a = "**ID: ** `" + z.Id+ "`";
                b = "**Preview: ** `" + z.Preview.Large + "`";
                c = "**URL: ** `" + z.Url + "`";
                d = "**Title: ** `" + z.Title + "`";
                e = "**ThumbnailURL: ** `" + z.Thumbnails.Small.ToString() + "`";

                Discord.EmbedBuilder embed = new EmbedBuilder
                {
                    Title = g,
                    Description = a + "\n" + b + "\n" + c + "\n" + d + "\n" + e,
                };
                embed.WithColor(Color.Blue);
                embed.WithCurrentTimestamp();
                Discord.Embed embedded = embed.Build();

                Misc.SendEmbedWithoutContext("", embedded, "twitch-output", "data");            
            }
            await Context.Channel.SendMessageAsync("Done");
        }



        // level 1
        [Command("SetupTracker")]
        [Summary("Sets up a Monitor object for streams")]
        public async Task RequestMonitor(string target_server, string target_channel, string gameid, int intervall, int limit )
        {
            if ( Permissions.IsUserAuthorized(Context.Message.Author.Id.ToString(), 1)
            {
                await Context.Channel.SendMessageAsync("Trying to setup ");
                Program.smh.Add(target_server, target_channel, gameid, intervall, limit);
            }
            else
            {
                await Context.Channel.SendMessageAsync("you are not the pathfinder main but if you want i can generate a minesweeper grid for you");
            }
        }



        [Command("showresults")]
        [Summary("returns the channelnames for a tracker instance")]
        public async Task RequestTrackedChannelnames(int instance)
        {
            if (Permissions.IsUserAuthorized( Context.Message.Author.Id.ToString() , 2))
            {
                await Context.Channel.SendMessageAsync("Processing...");

                Program.smh.instances[instance].PrintChannelNames(Context.Guild.Name,Context.Message.Channel.Name);              
            }
            else
            {
                await Context.Channel.SendMessageAsync("you are not the pathfinder main but if you want i can generate a minesweeper grid for you");
            }
        }

       /*
        private static bool IsUserAuthorized( long id, int perm_level_required )
        {
            sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Test Text ', 1); ";
            sqlite_cmd.ExecuteNonQuery();
            return false;
        }*/
    }
}
