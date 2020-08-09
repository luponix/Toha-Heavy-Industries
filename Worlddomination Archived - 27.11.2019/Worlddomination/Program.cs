using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;


namespace Worlddomination
{
    public class Program
    {
       

        public static void Main(string[] args)

            => new Program().MainAsync(args).GetAwaiter().GetResult();
        public static DiscordSocketClient _client;
        

        internal static CommandService commands;
        internal static IServiceProvider services;
        public static bool silent_on_streamstart_event = true;
        


        public async Task MainAsync(string[] args)
        {
            //this handles the commandline arguments
            foreach( string command_argument in args)
            {
                Console.WriteLine(command_argument);
                if (command_argument.Equals("-notsilent")) silent_on_streamstart_event = false;
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss")+" Control     silent_on_streamstart_event is set to " +silent_on_streamstart_event);
            Twitch.SilentTimer st = new Twitch.SilentTimer(180000);


            //initialise Discord
            _client = new DiscordSocketClient();

            _client.Log += Log;

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

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = "NTg1NTEwNzg0NTQxMzkyODk2.Xbh37A.pYTT77bHefrQUiOme-mUqati0eM";
            //setup your timer here
            //Twitch.DataTimer inter = new Twitch.DataTimer(120000, "513143");
            //inter.Start();
            Twitch.LiveMonitor LiveMonitor = new Twitch.LiveMonitor(); //this should be in the commands class at a later stage

            Console.WriteLine("-----------------------------------[Discord]------------------------------------");

            



            //Twitch.ApiCalls.Initialise();
            // InitTwitch TwitchInitialisation = new InitTwitch();
            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

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

        // If any services require the client, or the CommandService, or something else you keep on hand,
        // pass them as parameters into this method as needed.
        // If this method is getting pretty long, you can seperate it out into another file using partials.
        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            //       .AddSingleton(new SomeServiceClass());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }

        
    }
}
