using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
namespace Worlddomination.Twitch
{
    public class LiveMonitor
    {
        private LiveStreamMonitorService Monitor;
        private TwitchAPI API;
        private static List<string> lst;
        public static List<string> blocked = new List<string>();

        public LiveMonitor()
        {
            Task.Run(() => ConfigLiveMonitorAsync());
        }
        private async Task ConfigLiveMonitorAsync()
        {
            API = new TwitchAPI();
            API.Settings.ClientId = "onn9jpsat3vqo6muefbfgcq9m0wl26";
            API.Settings.AccessToken = "uujpqfrp1c6vc4s0pg7ypemefg7adg";


            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " TwitchAPI   Initialized the LiveMonitor");

            Monitor = new LiveStreamMonitorService(API, 60);

            //List<string> lst = new List<string> { "dreawus", "descentmax7930","kutcherlol","sovietwomble","noway4u_sir","solaaaa", "diegosaurs" };
            //lst.Add()

            //get all currently running streams and maybe add them to the list and then use the list to start the live monitor
            //we need to check somewhere regulary wether there are new unknown streams and if there are we update the list create a new live 
            //monitor and broadcast it to the channel
            //we need a way to check wether a stream is a new stream or not, we could log the time the live monitor gets restarted and test wether the
            //streams that we get as new events got started before that
            //for that we compare hours and minutes and seconds. that way we only have a problem if stream and livemonitor get started in the same second
            //this is getting pretty complicated, lets see wether we cant just use a 2 min timer to get all active overload streams and then use that data EDIT: we cant
            //create a list of streams that we knew in the former event

            lst = Data.Gate.Load("Streamers");
            Monitor.SetChannelsByName(lst);
            Monitor.OnStreamOnline += Monitor_OnStreamOnline;
            Monitor.OnStreamOffline += Monitor_OnStreamOffline;
            Monitor.OnStreamUpdate += Monitor_OnStreamUpdate;
            // Monitor.OnServiceStarted += Monitor_OnServiceStarted;

            // Monitor.OnChannelsSet += Monitor_OnChannelsSet;
            Monitor.Start(); //Keep at the end!
            await Task.Delay(-1);
        }
        private void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss")+" [NOTIFY]    "+ e.Channel + " just went online ");

            string target_server = "Overload";
            string target_channel = "community-streams";
            if( !Program.silent_on_streamstart_event )
            {
                if (e.Stream.GameId.Equals("491757"))
                {
                    if(!blocked.Contains(e.Channel))
                    {
                        Console.WriteLine("<<< we recognized this as an Overload stream");

                        DateTime dateTimeNow = DateTime.Now;
                        int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
                        string name = sec.ToString();

                        string thumUrl = e.Stream.ThumbnailUrl;
                        thumUrl = thumUrl.Replace("{width}", "480");
                        thumUrl = thumUrl.Replace("{height}", "270");

                        string[] paths = { @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\TohaHeavyIndustriesImages", name };
                        string fullPath = Path.Combine(paths);

                        Imgur.Download.DownloadUrl(thumUrl, fullPath);

                        Imgur.Imgur imgur = new Imgur.Imgur();
                        string finallink = imgur.GetImageUrl(fullPath);

                        string url = "https://www.twitch.tv/" + e.Channel.ToString();



                        Discord.EmbedBuilder embed = new EmbedBuilder
                        {
                            Title = e.Stream.Title,
                        };
                        embed.AddField(e.Channel + " is streaming " + Format.Bold("Overload"), "under " + url);
                        embed.ImageUrl = finallink;
                        embed.WithColor(Color.Blue);
                        embed.WithUrl(url);
                        embed.WithCurrentTimestamp();
                        Discord.Embed a = embed.Build();

                        Misc.SendEmbedWithoutContext("", a, target_channel, target_server); 
                    }
                    else
                    {
                        Console.WriteLine("BLOCKED STREAM FROM GETTING BROADCASTED : "+e.Channel);
                    }
                    
                }
                
            }
            //6328qort https://www.getpostman.com/oauth2/callback
        }



        private void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            //CAREFULL WE MAINLY USE THIS TO TEST RIGHT NOW
            if(e.Channel.Equals("chillybus"))
            {
                Console.WriteLine(" [LOG]-CHILLYBUS GameId: " + e.Stream.GameId);
            }
            
        }


        private void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [NOTIFY]    " + e.Channel + " just went offline ");
            blocked.Add(e.Channel);
            BlockTimer a = new BlockTimer(e.Channel);
           
        }
        /* private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)
         {
             throw new NotImplementedException();
         }
         private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
         {
             throw new NotImplementedException();
         }*/
    }
}