using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace Worlddomination.Twitch
{
    public class StreamsMonitorHandler
    {
        //this class gets initialised when the bot get started
        //and then pulls data in order to setup monitors on all servers that he is setup to do so for
        //we should also setup the RequestHandler for the twitch api here

        public List<StreamsMonitor> instances;
        public List<string> descent_streamer_whitelist;
        public List<string> banlist;

        //Constructor
        public StreamsMonitorHandler()
        {
            instances = new List<StreamsMonitor>();
            
            banlist = new List<string>();

            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Banned_Streamers";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                banlist.Add(sqlite_datareader.GetString(0));
            }


            descent_streamer_whitelist = new List<string>();
            sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Descent_Whitelisted_Streamers";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                descent_streamer_whitelist.Add(sqlite_datareader.GetString(0));
            }

        }

        public void Init()
        {
            Load();
        }

        // speciality:  streammonitors that get created by commands
        // start with initialized bool = true so that they broadcast streams
        // but that state shouldnt be saved
        // so add a parameter for that to the StreamsMonitor constructor
         
        // Add Monitor to In Memory storage and save
        public void Add( string target_server, string target_channel, string game_id,  int intervall, int limit)
        {
            StreamsMonitor stm = new StreamsMonitor(game_id, intervall, limit, target_channel, target_server, true);
            stm.Awake();
            instances.Add(stm);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    [Monitor Added*] arg: " + target_server + ", " + target_channel + ", " + game_id
                            + ", " + limit + ", " + intervall);

            SQLiteCommand sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO Monitors(game_category, server_name, channel_name, intervall, pull_limit)"
                                    +" VALUES('" +game_id+ "', '" +target_server + "', '" +target_channel+ "', "+intervall+", "+limit+"  ); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        // Loads and creates Monitor instances // should only be called initially when the bot starts
        public void Load()
        {

            SQLiteCommand sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Monitors";

            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string game_id = sqlite_datareader.GetValue(0).ToString();
                string target_server = sqlite_datareader.GetValue(1).ToString();
                string target_channel = sqlite_datareader.GetValue(2).ToString();
                int intervall = (int)(long)sqlite_datareader.GetValue(3);
                int limit = (int)(long)sqlite_datareader.GetValue(4);

                StreamsMonitor stm = new StreamsMonitor(game_id, intervall, limit, target_channel, target_server, false);
                stm.Awake();
                instances.Add(stm);

                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    [Monitor Added] arg: " + target_server + ", " + target_channel + ", " + game_id
                            + ", " + limit + ", " + intervall);
            }
        }


        private int StringToInt( string intString )
        {
            int i = 0;
            if (!Int32.TryParse(intString, out i))
            {
                i = -1;
            }
            return i;
        }
        


    }
}
