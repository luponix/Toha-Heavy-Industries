using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TwitchLib.Api.Helix.Models.Streams;

using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users;

namespace Worlddomination.Twitch
{
    // a class that is incredibly ugly due to hours of testing
    // needs some cleanup.
    //  In the end it comes down to 3 parts:
    //   1. Setup a timer that calls a function every x milliseconds 
    //   2. this function calls the twitch api data for a set category
    //   3. the function parses the data, checks for whitelist,bans,spam
    //      and maybe broadcasts it to a set discord-server, discord-channel
    public class StreamsMonitor
    {
        private Timer timer;
        private int timer_intervall = 60000;  // the x milliseconds delay
        private string game_id = "491757";    // a twitch category id
        private string stream_category = "";
        private int limit_of_streams = 20;    // sets how many streams get requested from the twitch api, maximum is 100
        private bool is_monitor_initialized = false; // circumvents a bug due to my inexperience with async
        private int event_counter = 0;        // counts the amount of timer events

        
        private string channel_out = "twitch-output";   // the discord channel, this monitor should send too, 'twitch-output' = default value
        private string server_out = "data";             // the discord server, this monitor should send too, 'data' = default value


        // holds returned data from the twitch api, need new and old data saved
        // to compare and circumvent some async madness
        TwitchLib.Api.Helix.Models.Streams.GetStreams.GetStreamsResponse veryOldActiveStreams;
        TwitchLib.Api.Helix.Models.Streams.GetStreams.GetStreamsResponse oldActiveStreams;

        // Datetime to figure out wether this stream should be posted, prevents unnecessary spam
        private Dictionary<string, DateTime> repost_dict = new Dictionary<string, DateTime>();







        //Constructor
        public StreamsMonitor(string id)
        {  
            game_id = id;
            limit_of_streams = 20;
            timer_intervall = 90000;

        }
        //Constructor
        public StreamsMonitor(string id, int timer_time, int limit, string channel, string server, bool is_init)
        {
            channel_out = channel;
            server_out = server;
            is_monitor_initialized = is_init;
            
            game_id = id;
            if (limit > 30 || limit < 2)
            {
                limit = 20;
            }
            limit_of_streams = limit;

            if (timer_time >= 60000) timer_intervall = timer_time;// we dont allow to request the api more than once per minute
            else
            {
                Console.WriteLine("Monitor instantiation: didnt accept timer_time: value below 60000 : value= " + timer_time);
                timer_intervall = 60000;
            }
        }


        //Timer section:
        public void Awake()
        {
            var ntimer = new Timer(timer_intervall);
            ntimer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            ntimer.Enabled = true;
            timer = ntimer;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PopulateStreamsData();
        }

        public  void PopulateStreamsData()
        {
            if (++event_counter % 100 == 0) Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    Pulling Data :[" + event_counter + "]");


            // get the information about the currently running twitch streams for the category of this monitor instance
            List<string> ids = new List<string>();
            ids.Add(game_id);
            var activeStreams = Program.API.Helix.Streams.GetStreamsAsync(first: limit_of_streams, gameIds: ids).Result;
            if (activeStreams.Streams.Length == 0)
            {
                if (!is_monitor_initialized)
                {
                    Console.WriteLine("Populated Monitor with pulled data gameid: " + game_id);
                    veryOldActiveStreams = activeStreams;
                    oldActiveStreams = activeStreams;
                    is_monitor_initialized = true;
                }
                veryOldActiveStreams = oldActiveStreams;
                oldActiveStreams = activeStreams;
                return;
            }

            // get the corresponding user data for the active streams in order to get data like the url for each channel logo
            List<string> user_ids = new List<string>();
            foreach (var stream in activeStreams.Streams) user_ids.Add(stream.UserId);
            var activeUsers = Program.API.Helix.Users.GetUsersAsync(user_ids).Result;

            if (!is_monitor_initialized)
            {
                Console.WriteLine("Populated Monitor with pulled data gameid: " + game_id);
                veryOldActiveStreams = activeStreams;
                oldActiveStreams = activeStreams;
                is_monitor_initialized = true;
            }
            if (stream_category.Equals("")) stream_category = activeStreams.Streams[0].GameName;
            int i = 0;
            foreach (TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream in activeStreams.Streams)
            {
                Console.WriteLine(stream.ViewerCount + ":     " +stream.UserName);
                // figures out wether this is a new stream that therefore should maybe be announced
                // filters out occasional twitch api inconsistencies by checking the last two results instead of just the last one
                if ( !ContainsStream(veryOldActiveStreams, stream) && !ContainsStream(oldActiveStreams, stream))
                {
                    if (!Program.smh.banlist.Contains(stream.UserName))
                    {
                        // check wether people restart their stream to frequently
                        TimeSpan min_time_between_streamannouncments = !(ids[0].Equals("Descent") || ids[0].Equals("Descent II") || ids[0].Equals("Descent 3") || ids[0].Equals("Descent Mission Builder")) ? new TimeSpan(0, 0, 25, 0, 0) : new TimeSpan(0, 16, 0, 0, 0);
                        if(HasMinTimePassedSinceLastAnnouncementOfThisChannel(stream.UserName, min_time_between_streamannouncments))
                        {
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + stream.UserName + " started streaming " + game_id);
                            if ((ids[0].Equals("Descent") || ids[0].Equals("Descent II") || ids[0].Equals("Descent 3")) && !Program.smh.descent_streamer_whitelist.Contains(stream.UserName))
                            {
                                SendNotWhitelistedDiscordEmbed(stream.UserName, ids[0], server_out, channel_out);
                            }
                            else
                            {
                                TwitchLib.Api.Helix.Models.Users.GetUsers.User user = null;
                                foreach(var u in activeUsers.Users)
                                {
                                    if (stream.UserId.Equals(u.Id)) user = u;
                                }
                                if (user != null) SendDiscordEmbed(stream, user, ids);
                            }
                        }
                    }
                }
                i++;
            }

            veryOldActiveStreams = oldActiveStreams;
            oldActiveStreams = activeStreams;
        }

        public static bool ContainsStream(TwitchLib.Api.Helix.Models.Streams.GetStreams.GetStreamsResponse response, TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream)
        {
            foreach(TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream str in response.Streams) {
                if (str.Equals(stream)) return true;
            }
            return false;
        }

        public bool HasMinTimePassedSinceLastAnnouncementOfThisChannel( string name, TimeSpan min_time_between_streams )
        {
            bool result = true;
            DateTime dateTimeNow = DateTime.Now;
            if (repost_dict.ContainsKey(name))
            {
                if (!(dateTimeNow.Subtract(repost_dict.GetValueOrDefault(name)).CompareTo(min_time_between_streams) > 0)){
                    result = false;
                }
                repost_dict.Remove(name);
            }
            repost_dict.Add(name, dateTimeNow);
            return result;
        }

        private static void SendNotWhitelistedDiscordEmbed(string channel_name, string stream_category, string discord_server, string discord_channel)
        {
            string url = "https://www.twitch.tv/" + channel_name;
            Discord.EmbedBuilder embed = new EmbedBuilder
            { };
            embed.AddField(channel_name + " started streaming " + Format.Bold(stream_category), "under " + url);
            embed.WithColor(Color.Red);
            embed.WithUrl(url);
            Discord.Embed embedded = embed.Build();
            Misc.SendEmbedWithoutContext("", embedded, discord_channel, discord_server);
        }

        private void SendDiscordEmbed(TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream stream, TwitchLib.Api.Helix.Models.Users.GetUsers.User user, List<string> ids)
        {
            DateTime dateTimeNow = DateTime.Now;
            int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
            string name = sec.ToString();
            string thumUrl = stream.ThumbnailUrl.Replace("{width}","1920").Replace("{height}","1080");//n_streams[j].Preview.Large;
                
            string[] paths = { @Data.Paths.img_directory, name + ".jpg" };
            string fullPath = System.IO.Path.Combine(paths);

            Imgur.Download.DownloadUrl(thumUrl, fullPath);
            Imgur.Imgur imgur = new Imgur.Imgur();
            string finallink = imgur.GetImageUrl(fullPath);

            string url = "https://www.twitch.tv/" + user.Login;

            
            var user_videos = Program.API.Helix.Videos.GetVideoAsync(userId: stream.UserId, first: 1).Result; 
            Discord.EmbedBuilder embed = new EmbedBuilder
            {
                Title = stream.Title,
                ThumbnailUrl = user.ProfileImageUrl,
            };
            embed.AddField(user.DisplayName + " is streaming " + Format.Bold(stream_category), "under " + url);
            embed.ImageUrl = finallink;
            embed.WithColor(Color.Blue);
            embed.WithUrl(url);
            try
            {
                if(!user_videos.Videos[0].Id.Equals("null")) embed.AddField("This stream will be archived under ", "https://www.twitch.tv/videos/"+user_videos.Videos[0].Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(stream.UserName + " doesnt have any past videos");
            }
            embed.WithCurrentTimestamp();
            Discord.Embed embedded = embed.Build();
            Misc.SendEmbedWithoutContext("", embedded, channel_out, server_out);
        }



        public void PrintChannelNames(string server, string channel)
        {
            foreach(var stream in oldActiveStreams.Streams)
            {
                Misc.SendMessageWithoutContext(stream.UserName, channel, server);
            }
        }

    }
}
