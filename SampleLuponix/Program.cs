using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordOSPLBot
{
    class Program
    {
        internal readonly DiscordSocketClient osplClient;
        internal readonly DiscordSocketClient botClient;

        internal readonly CommandService commands;
        internal readonly IServiceProvider services;

        // Program entry point
        static void Main(string[] args)
        {
            // Call the Program constructor, followed by the 
            // MainAsync method and wait until it finishes (which should be never).
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private Program()
        {
            botClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                MessageCacheSize = 50,
            });

            commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
            });

            // Subscribe the logging handler to both the client and the CommandService.
            botClient.Log += Log;
            commands.Log += Log;

            // Setup your DI container.
            services = ConfigureServices();
        }

        // Example of a logging handler. This can be re-used by addons
        // that ask for a Func<LogMessage, Task>.
        internal static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            string formattedMessage = $"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}";
            System.IO.File.AppendAllText(LogFileName, formattedMessage + Environment.NewLine);

            Console.WriteLine("{0}", formattedMessage);

            Console.ResetColor();

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
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

        private async Task InitCommands()
        {
            // Either search the program and add all Module classes that can be found.
            // Module classes MUST be marked 'public' or they will be ignored.
            // You also need to pass your 'IServiceProvider' instance now,
            // so make sure that's done before you get here.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<SomeModule>(_services);
            // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

            // Subscribe a handler to see if a message invokes a command.
            botClient.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            // We don't want the bot to respond to itself or other bots.
            if (msg.Author.Id == botClient.CurrentUser.Id || msg.Author.IsBot) return;

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;

            // Replace the '!' with whatever character
            // you want to prefix your commands with.
            // Uncomment the second half if you also want
            // commands to be invoked by mentioning the bot instead.
            if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
            {
                // Create a Command Context.
                var context = new SocketCommandContext(botClient, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully).
                var result = await commands.ExecuteAsync(context, pos, services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed.
                // This does not catch errors from commands with 'RunMode.Async',
                // subscribe a handler for '_commands.CommandExecuted' to see those.
                if (!result.IsSuccess && result.Error == CommandError.UnknownCommand)
                { 
                    await msg.Channel.SendMessageAsync($"I'm sorry {context.User.Username}, but I don't know that command. Please rephrase or use the !help command to get a list of all available commands.");
                }
                // if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                //    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        private async Task MainAsync()
        {
            // Centralize the logic for commands into a separate method.
            await InitCommands();

            // Login and connect.
            await botClient.LoginAsync(TokenType.Bot, "NTg1NTEwNzg0NTQxMzkyODk2.Xbh37A.pYTT77bHefrQUiOme-mUqati0eM");
            await botClient.StartAsync();

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
        }

        private const int BackgroundWorkerIntervalMins = 1;
        internal void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(60 * 1000 * BackgroundWorkerIntervalMins);
                MiscModule.PeriodicBackgroundJobs(botClient);
            }
        }

        /// <summary>
        /// Return role for this guilds 'Everyone' role.
        /// </summary>
        internal static string LogFileName
        {
            get
            {
                string fileName = ConfigurationManager.AppSettings["LogFileName"];
                if (String.IsNullOrEmpty(fileName)) fileName = "HAL9000_log.txt";
                return fileName;
            }
        }
    }
}