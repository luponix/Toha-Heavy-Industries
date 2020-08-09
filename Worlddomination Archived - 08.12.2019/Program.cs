using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TwitchLib.Api;
using Worlddomination.Twitch;

namespace Worlddomination
{
    public class Program
    {


        public static void Main(string[] args)
        

            => new Program().MainAsync(args).GetAwaiter().GetResult(); 


        public static DiscordSocketClient _client;
        public static TwitchAPI API;
        public static StreamsMonitorHandler smh = new StreamsMonitorHandler();

        internal static CommandService commands;
        internal static IServiceProvider services;
        //public static bool silent_on_streamstart_event = true;



        public async Task MainAsync(string[] args)
        {
            //this handles the commandline arguments
            foreach (string command_argument in args)
            {
                Console.WriteLine(command_argument);
                //if (command_argument.Equals("-notsilent")) silent_on_streamstart_event = false;
            }
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss")+" Control     silent_on_streamstart_event is set to " +silent_on_streamstart_event);
            //Twitch.SilentTimer st = new Twitch.SilentTimer(180000);


            //initialise Discord
            _client = new DiscordSocketClient();
            _client.Log += Log;

            API = new TwitchAPI();
            API.Settings.ClientId = "onn9jpsat3vqo6muefbfgcq9m0wl26";          //Twitch API ClientID
            API.Settings.Secret = "212b473uo803kull6iw707sq37cmzo";            //Twitch APi Client_Secret
            API.Settings.AccessToken = "uujpqfrp1c6vc4s0pg7ypemefg7adg";



            commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
            });
            // Setup your DI container.
            services = ConfigureServices();

            CommandHandler commandhandler = new CommandHandler(_client, commands);
            await commandhandler.InstallCommandsAsync();


            var token = "NTg1NTEwNzg0NTQxMzkyODk2.Xbh37A.pYTT77bHefrQUiOme-mUqati0eM";




            smh.Init();

            //Twitch.StreamsMonitor test_monitor = new Twitch.StreamsMonitor("491757", 90000, 20, "community-streams", "Overload"); //"");513142 497057 "twitch-output-overload", "data");
            //test_monitor.Awake();










            Console.WriteLine("-----------------------------------[Discord]------------------------------------");






            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();
            return map.BuildServiceProvider();
        }

    
    }
}
