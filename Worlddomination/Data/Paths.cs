using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Worlddomination.Data
{
    class Paths
    {
        public static string paths_txt = Path.Combine(Directory.GetCurrentDirectory(), "THI-paths.txt");

        public static string monitor_instances_txt = @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Twitch\MonitorData.txt";

        public static string img_directory = @"E:\TohaHeavyIndustries Images Archive";

        public static string meme_directory = @"C:\Users\luponix\Desktop\approved memes";


        public static void Initialise()
        {
            if (File.Exists(paths_txt))
            {
                ReadPathFile();
            }
            else
            {
                Console.WriteLine(" Creating default path file: "+paths_txt);
                CreateDefaultPathFile();
            }
        }

        private static void ReadPathFile()
        {
            using (StreamReader sw = new StreamReader(paths_txt))
            {
                string line0 = sw.ReadLine();
                string line1 = sw.ReadLine();
                string line2 = sw.ReadLine();
                if( line0 != null && line1 != null && line2 != null )
                {
                    monitor_instances_txt = line0;
                    img_directory = line1;
                    meme_directory = line2;
                }
                else
                {
                    CreateDefaultPathFile();
                    ReadPathFile();
                }
            }
        }

        private static void CreateDefaultPathFile()
        {
            using (StreamWriter sw = File.CreateText(paths_txt))
            {
                sw.WriteLine(monitor_instances_txt);
                sw.WriteLine(img_directory);
                sw.WriteLine(meme_directory);
            }
        }

    }
}
