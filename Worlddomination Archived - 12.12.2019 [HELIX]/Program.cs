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
        public static TwitchAPI API ;
        public static StreamsMonitorHandler smh = new StreamsMonitorHandler();
        public static string version = "0.0.1";

        internal static CommandService commands;
        internal static IServiceProvider services;
        //public static bool silent_on_streamstart_event = true;
        


        public async Task MainAsync(string[] args)
        {
            //this handles the commandline arguments
            foreach( string command_argument in args)
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
            API.Settings.ClientId = Data.APIToken.GetTwitchClientId();          //Twitch API ClientID
            API.Settings.Secret = Data.APIToken.GetTwitchClientSecret();            //Twitch APi Client_Secret
            API.Settings.AccessToken = Data.APIToken.GetTwitchAccessToken();

            

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

            
            var token = Data.APIToken.GetDiscordClientToken();

            smh.Init();

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
