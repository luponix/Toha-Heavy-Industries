using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TwitchLib.Api.Helix.Models.Streams;
using Stream = TwitchLib.Api.Helix.Models.Streams.Stream;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users;

namespace Worlddomination.Twitch
{
    public class StreamsMonitor
    {
        private Timer timer;
        private int timer_intervall = 60000;
        private string game_id = "491757";
        private int limit_of_streams = 20;
        private bool is_monitor_initialized = false;
        private int event_counter = 0;

        
        private string channel_out = "twitch-output";
        private string server_out = "data";


        //maybe create a class that unites these informations
        private TwitchLib.Api.V5.Models.Streams.Stream[] streams_array;
        private string[] profile_urls;
        List<string> channel_names = new List<string>();

        private TwitchLib.Api.V5.Models.Streams.Stream[] OLDstreams_array;
        private string[] OLDprofile_urls;
        List<string> OLDchannel_names = new List<string>();

        //Datetime to figure out wether this stream should be posted
        private Dictionary<string, DateTime> repost_dict = new Dictionary<string, DateTime>();





        // private Stream[] n_streams_array;
        // private string[] n_channel_names;
        // private string[] n_profile_urls;

        //Constructor
        public StreamsMonitor(string id)
        {  
            game_id = id;
            limit_of_streams = 20;
            timer_intervall = 90000;
            streams_array = new TwitchLib.Api.V5.Models.Streams.Stream[limit_of_streams];
            //channel_names = new string[limit_of_streams];
            profile_urls = new string[limit_of_streams];
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
            streams_array = new TwitchLib.Api.V5.Models.Streams.Stream[limit];
           // channel_names = new string[limit];
            profile_urls = new string[limit];
            if (timer_time >= 60000) // we dont allow to request the api more than once per minute
            {
                timer_intervall = timer_time;
            }
            else
            {
                Console.WriteLine("Monitor instantiation: didnt accept timer_time: value below 60000 : value= " + timer_time);
                timer_intervall = 60000;
            }
        }
        
        public int GetIntervall()
        {
            return timer_intervall;
        }
        public string GetGameID()
        {
            return game_id;
        }
        public int GetStreamLimit()
        {
            return limit_of_streams;
        }
        public string GetTargetServer()
        {
            return server_out;
        }
        public string GetTargetChannel()
        {
            return channel_out;
        }

        //Timer section:
        public void Awake()
        {
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Monitor     Awake");
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
            Program.smh.banlist = Data.Gate.Load("Streamers");
            if (++event_counter % 100 == 0)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    Pulling Data :[" + event_counter + "]");
            }
            List<string> ids = new List<string>();
            List<string> user_ids = new List<string>();
            List<string> n_name = new List<string>();
            string[] n_profile = new string[limit_of_streams];
            TwitchLib.Api.V5.Models.Streams.Stream[] n_streams = new TwitchLib.Api.V5.Models.Streams.Stream[limit_of_streams];
            ids.Add(game_id);

            //var request1 = Program.API.Helix.Streams.GetStreamsAsync(first: limit_of_streams, gameIds: ids).Result;
            var request = Program.API.V5.Streams.GetLiveStreamsAsync(game: game_id, limit: limit_of_streams).Result;
            int i = 0;

            foreach (TwitchLib.Api.V5.Models.Streams.Stream stream in request.Streams)
            {              
                user_ids.Add(stream.Channel.Id);            
                n_streams[i] = stream;
                //if (!stream.Channel.DisplayName.Equals("luponix3"))
                //{
                    //Console.WriteLine(stream.Channel.DisplayName);
                    n_profile[i] = stream.Channel.Logo;
                    n_name.Add(stream.Channel.DisplayName);
                //}
                i++;
            }

            if (!is_monitor_initialized)
            {
                Console.WriteLine("Populated Monitor with pulled data gameid: "+ game_id);
                streams_array = n_streams;
                channel_names = n_name;
                profile_urls = n_profile;
                OLDstreams_array = streams_array;
                OLDchannel_names = channel_names;
                OLDprofile_urls = profile_urls;
                is_monitor_initialized = true;
            }

            else
            {
                // update banlist


                // compare old tick and new tick
                int j = 0;
                foreach ( string na in n_name)
                {            
                    if( !channel_names.Contains(n_name[j]) & !OLDchannel_names.Contains(n_name[j]))
                    {// Broadcast to discord
                        //Console.WriteLine("|| DID NOT contain : " + n_name[j]);

                        if (!Program.smh.banlist.Contains(n_name[j]))
                        {
                            //look wether this stream is started to frequently
                            bool valid = true;
                            DateTime dateTimeNow = DateTime.Now;
                            TimeSpan repostTime = new TimeSpan(0, 0, 25, 0, 0);
                            if (repost_dict.ContainsKey(n_name[j])  )
                            {
                                if (!(dateTimeNow.Subtract(repost_dict.GetValueOrDefault(n_name[j])).CompareTo(repostTime) > 0))
                                {
                                    valid = false;
                                }
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + n_name[j] + " got evaluated: " + valid + " value: " + dateTimeNow.Subtract(repost_dict.GetValueOrDefault(n_name[j])).CompareTo(repostTime));
                                repost_dict.Remove(n_name[j]);
                            }
                            repost_dict.Add(n_name[j], dateTimeNow);

                            // descent streamer whitelist enactment
                            if ( (ids[0].Equals("Descent") || ids[0].Equals("Descent II") || ids[0].Equals("Descent 3") )
                                && !Program.smh.descent_streamer_whitelist.Contains(n_name[j]) )
                            {
                                string url = "https://www.twitch.tv/" + n_name[j];
                                Discord.EmbedBuilder embed = new EmbedBuilder
                                {};
                                embed.AddField(n_name[j] + " started streaming " + Format.Bold(ids[0]), "under " + url);               
                                embed.WithColor(Color.Red);
                                embed.WithUrl(url); 
                                // embed.WithCurrentTimestamp();
                                Discord.Embed embedded = embed.Build();
                                Misc.SendEmbedWithoutContext("", embedded, channel_out, server_out);
                            }
                            else
                            {
                                if (valid)
                                {
                                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + n_name[j] + " started streaming " + game_id);
                                    try
                                    {
                                        // DateTime dateTimeNow = DateTime.Now;
                                        int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
                                        string name = sec.ToString();
                                        string thumUrl = n_streams[j].Preview.Large;

                                        string[] paths = { @Data.Paths.img_directory, name + ".jpg" };//@"E:\TohaHeavyIndustries Images Archive", name+".jpg" };
                                        string fullPath = System.IO.Path.Combine(paths);

                                        Imgur.Download.DownloadUrl(thumUrl, fullPath);
                                        Imgur.Imgur imgur = new Imgur.Imgur();
                                        string finallink = imgur.GetImageUrl(fullPath);

                                        string url = "https://www.twitch.tv/" + n_name[j];

                                        string game_name = ids[0];
                                        if (ids[0].Equals("491757")) game_name = "Overload"; // this might not be necessary anymore, test by adding a console.writeline

                                        var user_videos = Program.API.V5.Channels.GetChannelVideosAsync(n_streams[j].Channel.Id, limit: 1).Result;
                                        Discord.EmbedBuilder embed = new EmbedBuilder
                                        {
                                            Title = n_streams[j].Channel.Status,
                                            ThumbnailUrl = n_profile[j],
                                        };
                                        embed.AddField(n_name[j] + " is streaming " + Format.Bold(game_name), "under " + url);
                                        embed.ImageUrl = finallink;
                                        embed.WithColor(Color.Blue);
                                        embed.WithUrl(url);
                                        try
                                        {
                                            embed.AddField("This stream will be archived under ", user_videos.Videos[0].Url);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(na + " doesnt have any past videos");
                                        }
                                        embed.WithCurrentTimestamp();
                                        Discord.Embed embedded = embed.Build();
                                        Misc.SendEmbedWithoutContext("", embedded, channel_out, server_out);
                                    }

                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Encountered Exception: " + n_name[j]);
                                    }
                                }
                            }

                       


                        }
                        else
                        {
                            Console.WriteLine("Didnt broadcast stream due to to streamer being banned: "+ n_name[j]);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("|| Did contain : "+n_name[j]);
                    }
                    j++;
                }

                // Save Data to old tick
                OLDstreams_array = streams_array;
                OLDchannel_names = channel_names;
                OLDprofile_urls = profile_urls;

                streams_array = n_streams;
                channel_names = n_name;
                profile_urls = n_profile;

            }
            

        }

        public void PrintChannelNames(string server, string channel)
        {
           // Console.WriteLine("I exist");
           // string result = "";//"```";
            foreach(TwitchLib.Api.V5.Models.Streams.Stream stream in streams_array)
            {
                Misc.SendMessageWithoutContext(stream.Channel.DisplayName, channel, server);
                // result += stream.Channel.DisplayName;//+ "\n";
            }
            
           // result += "```";
           // Console.WriteLine(result);
           // Misc.SendMessageWithoutContext(result, server, channel);
        }

    }
}
