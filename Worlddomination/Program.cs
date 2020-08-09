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

        public static string version = "0.8.2";

        internal static CommandService commands;
        internal static IServiceProvider services;

        


        public async Task MainAsync(string[] args)
        {


            Data.Paths.Initialise();
            Console.WriteLine("Initialised Paths");

            // initialise Discord
            _client = new DiscordSocketClient();
            _client.Log += Log;

            // public twitch api object
            API = new TwitchAPI();
            API.Settings.ClientId = Data.APIToken.GetTwitchClientId();          
            API.Settings.Secret = Data.APIToken.GetTwitchClientSecret();           
            API.Settings.AccessToken = Data.APIToken.GetTwitchAccessToken();

            // setup for the Discord command handler
            commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
            });
            // setup the DI container.
            services = ConfigureServices();

            CommandHandler commandhandler = new CommandHandler(_client, commands);
            await commandhandler.InstallCommandsAsync();

            
            var token = Data.APIToken.GetDiscordClientToken();

            // initialise the stream monitor handler as the parent for all stream monitor instances
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
