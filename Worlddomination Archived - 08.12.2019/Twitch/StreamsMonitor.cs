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
        private Stream[] streams_array;
        private string[] profile_urls;
        List<string> channel_names = new List<string>();
        // private Stream[] n_streams_array;
        // private string[] n_channel_names;
        // private string[] n_profile_urls;

        //Constructor
        public StreamsMonitor(string id)
        {  
            game_id = id;
            limit_of_streams = 20;
            timer_intervall = 90000;
            streams_array = new Stream[limit_of_streams];
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
            streams_array = new Stream[limit];
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
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Monitor     Awake");
            var ntimer = new Timer(timer_intervall);
            ntimer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            ntimer.Enabled = true;
            timer = ntimer;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PopulateStreamsData();

            /*
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Monitor      Event[" + event_counter++ + "]: Pulling stream data ");


            // grab the streams data           
            //Twitch.TwitchApi.RequestHandler a = new Twitch.TwitchApi.RequestHandler(API);
            //Task  a = await a.GetStreamsForGameIdsAsync(limit_of_streams, game_id);
            Stream[] n_stream = Worlddomination.Twitch.TwitchApi.RequestHandler.CallGetStreamsForGameIdsAsyncAndWaitOnResult(limit_of_streams, game_id);

            List<string> user_ids = new List<string>();


            foreach (Stream curr in n_stream)
            {
                user_ids.Add(curr.UserId);
            }


            //List<string> mixed = Task.Run(async () => await Worlddomination.Twitch.TwitchApi.RequestHandler.CallGetNamesAndProfileUrlsAsyncAndWaitOnResult(user_ids)).Result;

            List<string> mixed =  Worlddomination.Twitch.TwitchApi.RequestHandler.CallGetNamesAndProfileUrlsAsyncAndWaitOnResult(user_ids);
            List<string> n_profile = new List<string>();
            List<string> n_name = new List<string>();

            int i = 0;
            int j = 0;
            foreach (string selected in mixed)
            {
                if (mixed.IndexOf(selected) % 2 == 0)
                {
                    n_profile[i] = selected;
                    i++;
                }
                else
                {
                    n_name[j] = selected;
                    j++;
                }
            }
            // TEST: print all the necessary informations:
            int counter = 0;
            foreach (Stream curr in n_stream)
            {
                Console.WriteLine("WE WENT INTO THE CREATE MESSAGE LOOP");
                DateTime dateTimeNow = DateTime.Now;
                int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
                string name = sec.ToString();

                string thumUrl = curr.ThumbnailUrl;
                thumUrl = thumUrl.Replace("{width}", "480");
                thumUrl = thumUrl.Replace("{height}", "270");

                string[] paths = { @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\TohaHeavyIndustriesImages", name };
                string fullPath = Path.Combine(paths);

                Imgur.Download.DownloadUrl(thumUrl, fullPath);

                Imgur.Imgur imgur = new Imgur.Imgur();
                string finallink = imgur.GetImageUrl(fullPath);

                string url = "https://www.twitch.tv/" + n_name[counter];



                Discord.EmbedBuilder embed = new EmbedBuilder
                {
                    Title = curr.Title,
                };
                embed.AddField(n_name[counter] + " is streaming " + Format.Bold(game_id), "under " + url);
                embed.ImageUrl = finallink;
                embed.WithColor(Color.Blue);
                embed.WithUrl(url);
                embed.WithCurrentTimestamp();
                Discord.Embed embedded = embed.Build();

                Misc.SendEmbedWithoutContext("", embedded, "twitch-output", "data");
                counter++;

                /*
                int counter = 0;
                foreach( string mix in mixed )
                {
                    counter++;
                    int cur = mixed.IndexOf(mix);
                    if (cur % 2 == 0)
                    {
                        profile_urls[counter] = mixed[cur];
                    }
                    else
                    {
                        channel_names[counter] = mixed[cur];
                    }         
                }
                int c = 0;
                foreach(Stream i in streams_array)
                {
                    Console.WriteLine("-----------------------------------["+channel_names[c]+"]------------------------------------");
                    Console.WriteLine(profile_urls[c]);
                    Console.WriteLine();
                    c++;
                }


                // monitor is not initialized put all streams directly into the private stream array -> check for corner cases
                if (!is_monitor_initialized)
                {
                    //put the pulled data into a streamlist object and set the private streamlist object of this class to that
                    return;
                }
                // if the monitor is initialised compare the stream objects in the private monitor to the pulled streams
                // foreach pulled stream test wether it is in stored already  -> yes = update event   -> no = somebody went online
                // update event -> is pretty much useless since we pull streams out of the overload category anyways
                // we can use an onOffline event to share that the streamer is offline/not streaming overload anymore + broadcast the url for the
                // saved video if that exists -> for that we look in the last recent streams of that person for the thumbnail url or sth of that sort
                // if we dont find it schedule a 30 min timer that looks for it again, if that doesnt work check in 30 min again
                // technically you could also create an on offline event by comparing the known streams to the new ones but we dont need that right now
                // so we can simply put the pulled streams into the known streams array
                else
                {
                    //create another streamlist object and compare it to the saved one from last pull
                }
                */



            /*

            }
            Console.WriteLine("WE LEFT THE THE CREATE MESSAGE LOOP");

            */
        }


        // new idea: we dont return the values over the correct path but we directly write into variables of this object 
        // after that it calls a process method
        public  void PopulateStreamsData()
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    Pulling Data :["+event_counter+++"]");

            List<string> ids = new List<string>();
            List<string> user_ids = new List<string>();
            List<string> n_name = new List<string>();
            string[] n_profile = new string[limit_of_streams];
            Stream[] n_streams = new Stream[limit_of_streams];
            ids.Add(game_id);

            var request = Program.API.Helix.Streams.GetStreamsAsync(gameIds: ids, first: limit_of_streams).Result;

            int i = 0;

          //  Console.WriteLine("-----------Stream User IDs----------------");
            foreach (Stream stream in request.Streams)
            {           
                Console.WriteLine(stream.UserId);
                user_ids.Add(stream.UserId);
                //stream.
                n_streams[i] = stream;
                i++;
            }
            var request2 = Program.API.Helix.Users.GetUsersAsync(ids: user_ids).Result;
            int c = 0;
           // Console.WriteLine("-----------User Display Names-------------");
            foreach ( User user in request2.Users)
            {
                if( !user.DisplayName.Equals("luponix3"))
                {   
                    Console.WriteLine(user.DisplayName);
                    n_profile[c] = user.ProfileImageUrl;
                    n_name.Add(user.DisplayName);
                    c++;
                }
            }
           


            if (!is_monitor_initialized)
            {
                Console.WriteLine("Populated Monitor with pulled data gameid: "+ game_id);
                //Save Data
                streams_array = n_streams;
                channel_names = n_name;
                profile_urls = n_profile;

                is_monitor_initialized = true;
            }

            

            else
            {
                // compare old tick and new tick
                int j = 0;
                foreach ( string na in n_name)
                {            
                    if( !channel_names.Contains(n_name[j]))
                    {// Broadcast to discord
                        Console.WriteLine("|| DID NOT contain : " + n_name[j]);
                        //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + n_name[j] + " started streaming " + game_name);
                        try
                            {
                            string game_name = ids[0];
                            if (game_id.Equals("491757"))
                            {
                                game_name = "Overload";
                            }
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + n_name[j] + " started streaming " + game_name);

                            DateTime dateTimeNow = DateTime.Now;
                            int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
                            string name = sec.ToString();

                            string thumUrl = n_streams[j].ThumbnailUrl;
                            thumUrl = thumUrl.Replace("{width}", "480");
                            thumUrl = thumUrl.Replace("{height}", "270");

                            string[] paths = { @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\TohaHeavyIndustriesImages", name };
                            string fullPath = System.IO.Path.Combine(paths);
                            
                            Imgur.Download.DownloadUrl(thumUrl, fullPath);

                            Imgur.Imgur imgur = new Imgur.Imgur();
                            string finallink = imgur.GetImageUrl(fullPath);

                            string url = "https://www.twitch.tv/" + n_name[j];

                            
                            Discord.EmbedBuilder embed = new EmbedBuilder
                            {
                                Title = n_streams[j].Title,
                            };
                            embed.AddField(n_name[j] + " is streaming " + Format.Bold(game_name), "under " + url);
                            embed.ImageUrl = finallink;
                            embed.WithColor(Color.Blue);
                            embed.WithUrl(url);
                            embed.WithCurrentTimestamp();
                            Discord.Embed embedded = embed.Build();

                            Misc.SendEmbedWithoutContext("", embedded, channel_out, server_out);
                            
                        }
                        catch( Exception e)
                        {
                            Console.WriteLine("Encountered Exception: " + e);
                        }
                    }
                    else
                    {
                        Console.WriteLine("|| Did contain : "+n_name[j]);
                    }
                    j++;
                }
               
                // Save Data to old tick
                streams_array = n_streams;
                channel_names = n_name;
                profile_urls = n_profile;

            }
            

        }

        private bool ContainsName( string name )
        {
            foreach( string i in channel_names)
            {
                if (name.Equals(i))
                {  
                    return true;
                }     
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + name + " started streaming " + game_id);
            return false;
        }

    }
}
