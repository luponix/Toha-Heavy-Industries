using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TwitchLib.Api;
using Worlddomination.Twitch;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Worlddomination
{
    public class Program
    {
       

        public static void Main(string[] args)

            => new Program().MainAsync(args).GetAwaiter().GetResult();
        public static DiscordSocketClient _client;
        public static TwitchAPI API;
        public static SQLiteConnection sqlite_conn;

        public static StreamsMonitorHandler smh;

        public static string version = "0.8.6";

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

            // Initialize the SQL Database here
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Database    initialised");
            sqlite_conn = CreateConnection();
            //CreateDefaultTables(sqlite_conn);

            smh = new StreamsMonitorHandler();

            Commands.Permissions.Initialise();   // populate the in memory register of authorised users for fast lookup
            // smh.init using the data from the db



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


            // initialise the stream monitor handler as the parent for all stream monitor instances
            smh.Init();

            Console.WriteLine("-----------------------------------[Discord]------------------------------------");

            await _client.LoginAsync(TokenType.Bot, Data.APIToken.GetDiscordClientToken());
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



        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=Worlddomination.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DB EXCEPTION] /n" + ex + "/n");
            }
            return sqlite_conn;
        }

        static void CreateDefaultTables(SQLiteConnection conn)
        {
            try
            {
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = "CREATE TABLE Monitors ("
                                        + "game_category TEXT NOT NULL,"
                                        + "server_name TEXT NOT NULL,"
                                        + "channel_name TEXT NOT NULL,"
                                        + "intervall INTEGER(32),"
                                        + "pull_limit INTEGER(32)"
                                        +");";
                sqlite_cmd.ExecuteNonQuery();
                sqlite_cmd.CommandText = "CREATE TABLE Banned_Streamers ("
                                        + "streamer_name TEXT NOT NULL"
                                        +");";
                sqlite_cmd.ExecuteNonQuery();
                sqlite_cmd.CommandText = "CREATE TABLE Descent_Whitelisted_Streamers ("
                                        + "streamer_name TEXT NOT NULL"
                                        + ");";
                sqlite_cmd.ExecuteNonQuery();
                sqlite_cmd.CommandText = "CREATE TABLE Permissions ("
                                        + "name TEXT NOT NULL,"
                                        + "level INTEGER(32)"
                                        + ");";
                sqlite_cmd.ExecuteNonQuery();



                sqlite_cmd.CommandText = "INSERT INTO Permissions(name, level) VALUES('211180504060395521', 0); ";
                sqlite_cmd.ExecuteNonQuery();


            }
            catch( Exception ex )
            {
                Console.WriteLine(ex);
            }
            

        }

        static void InsertData(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Test Text ', 1); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Hallo Ich ', 2); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Test2 Bin ', 3); ";
            sqlite_cmd.ExecuteNonQuery();


            sqlite_cmd.CommandText = "INSERT INTO SampleTable1(Col1, Col2) VALUES('Ein Baum ', 3); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        static void ReadData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM SampleTable";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string myreader = sqlite_datareader.GetString(0);
                Console.WriteLine(myreader);
            }
            conn.Close();
        }
    }
}
