using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Worlddomination.Twitch
{
    public class StreamsMonitorHandler
    {
        //this class gets initialised when the bot gets started
        //and then pulls data in order to setup monitors on all servers that he is setup to do so for
        //we should also setup the RequestHandler for the twitch api here

        List<StreamsMonitor> instances;

        //Constructor
        public StreamsMonitorHandler()
        {
            instances = new List<StreamsMonitor>();
            
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
            //save
            Save();
        }

        // Loads and creates Monitor instances // should only be called initially when the bot starts
        public void Load()
        {
            // 1. target server
            // 2. target channel
            // 3. gameid
            // 4. intervall
            // 5. limit

            string file_path = @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Twitch\MonitorData.txt";
            
            List<string> content = new List<string> { };
            if (File.Exists(file_path))
            {
                using (StreamReader f = new StreamReader(file_path))
                {
                    string target_server = "";
                    string target_channel = "";
                    string game_id = "";
                    string intervall = "";
                    string limit = "";

                    string line;
                    int counter = 0;
                    while ((line = f.ReadLine()) != null && line != "")
                    {
                       
                        if( counter == 0 )
                        {
                            target_server = line;
                        }
                        if (counter == 1)
                        {
                            target_channel = line;
                        }
                        if (counter == 2)
                        {
                            game_id = line;
                        }
                        if (counter == 3)
                        {
                            intervall = line;
                        }
                        if (counter == 4)
                        {
                            limit = line;
                            StreamsMonitor stm = new StreamsMonitor(game_id, StringToInt(intervall), StringToInt(limit), target_channel, target_server, false);
                            stm.Awake();
                            instances.Add(stm);
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [Monitor Added] arg: "+ target_server +", "+target_channel+", "+game_id
                                            + ", "+ limit+ ", "+intervall);
                            counter = -1;
                        }
                        counter++;
                    }
                }

            }
            
        }

        // Saves Monitor instances
        public void Save()
        {
            using (StreamWriter sw = File.CreateText(@"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Twitch\MonitorData.txt"))
            {
                foreach (StreamsMonitor monitor in instances)
                {
                    sw.WriteLine(monitor.GetTargetServer());
                    sw.WriteLine(monitor.GetTargetChannel());
                    sw.WriteLine(monitor.GetGameID());
                    sw.WriteLine(monitor.GetIntervall().ToString());
                    sw.WriteLine(monitor.GetStreamLimit().ToString());
                }
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
