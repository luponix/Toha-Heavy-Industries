using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using TwitchLib.Api;

namespace Worlddomination.Twitch
{
    public class DataTimer
    {
        private Timer _timer;
        private TwitchAPI _API;
        private int _timer_intervall;
        private string _game_id;
        private long _event_counter = 0;

        public DataTimer(int timer_intervall, string game_id)
        {
            _timer_intervall = timer_intervall;
            _game_id = game_id; //Overload is 49757
        }

        // succesfully mined with Logs

        public void Start()
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss")+" Timer       Setup pull streams timer");

            //setup twitch api access
            _API = new TwitchAPI();
            _API.Settings.ClientId = "onn9jpsat3vqo6muefbfgcq9m0wl26";
            _API.Settings.AccessToken = "uujpqfrp1c6vc4s0pg7ypemefg7adg";

            //Setup timer for 2 minutes
            var timer = new Timer(_timer_intervall);
            timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            timer.Enabled = true;
            _timer = timer;
        }

        async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _event_counter++;
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Timer    Event triggered ["+_event_counter+"]");
            //pull active streams for game id

            TwitchLib.Api.V5.Models.Search.SearchStreams a = await  _API.V5.Search.SearchStreamsAsync(_game_id);
            //active_streams.Streams.Stream[];
            Console.WriteLine("welp we got through the api request");
            foreach (TwitchLib.Api.V5.Models.Streams.Stream sel in a.Streams)
            {
                Console.WriteLine("["+_event_counter+"] "+sel.Channel.Name);
            }


        }

        public void RestartTimer()
        {
            _timer.Stop();
            Start();
        }

    }
}


/*
 * using System;
using System.Collections.Generic;
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

            //List<string> lst = new List<string> { "dreawus", "descentmax7930","kutcherlol","sovietwomble","noway4u_sir","solaaaa" };
            //lst.Add()

            //get all currently running streams and maybe add them to the list and then use the list to start the live monitor
            //we need to check somewhere regulary wether there are new unknown streams and if there are we update the list create a new live 
            //monitor and broadcast it to the channel
            //we need a way to check wether a stream is a new stream or not, we could log the time the live monitor gets restarted and test wether the
            //streams that we get as new events got started before that
            //for that we compare hours and minutes and seconds. that way we only have a problem if stream and livemonitor get started in the same second
            //this is getting pretty complicated, lets see wether we cant just use a 2 min timer to get all active overload streams and then use that data
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

            //check wether the stream is known in the list
            e.Stream.UserId.

            //Broadcast to discord if not in silent mode
            // disable the silent mode after 5 min ? as till then you should have gotten all the streams
            if (!Program.silent_on_streamstart_event)
            {
                

            }
            Console.WriteLine("---------<OnOnline>--------");
            Console.WriteLine("Title : " + e.Stream.Title);
            Console.WriteLine("Viewer: " + e.Stream.ViewerCount);
            Console.WriteLine("UserID: " + e.Stream.UserId);
            Console.WriteLine("TumURL: " + e.Stream.ThumbnailUrl);
            Console.WriteLine("Langua: " + e.Stream.Language);
        }
        private void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            Console.WriteLine("---------(OnUpdate)--------");
            Console.WriteLine("Title : "+e.Stream.Title);
           // Console.WriteLine("Viewer: "+e.Stream.ViewerCount);
           // Console.WriteLine("UserID: "+e.Stream.UserId);
           // Console.WriteLine("TumURL: "+e.Stream.ThumbnailUrl);
           // Console.WriteLine("Langua: "+e.Stream.Language );
           // Console.WriteLine("Type  : "+e.Stream.Type);
            Console.WriteLine("ID    : " + e.Stream.Id);
            Console.WriteLine("GameID: " + e.Stream.GameId); //need to figure out overloads gameid the next time somebody streams
        }
        private void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine("---------[OnOffline]--------");
            Console.WriteLine("Title : " + e.Stream.Title);
            Console.WriteLine("Viewer: " + e.Stream.ViewerCount);
            Console.WriteLine("UserID: " + e.Stream.UserId);
            Console.WriteLine("TumURL: " + e.Stream.ThumbnailUrl);
            Console.WriteLine("Langua: " + e.Stream.Language);
            Console.WriteLine("GameID: " + e.Stream.GameId);
        }
       /* private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)
        {
            throw new NotImplementedException();
        }
        private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
 */ 