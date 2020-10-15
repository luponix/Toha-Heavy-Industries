using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Worlddomination.Data
{
    class Paths
    {
        public static string paths_txt = Path.Combine(Directory.GetCurrentDirectory(), "THI-paths.txt");

        public static string APIToken = Path.Combine(Directory.GetCurrentDirectory(), "APIToken.txt");

        public static string img_directory = Directory.GetCurrentDirectory();

        public static string meme_directory = Directory.GetCurrentDirectory();

       

        public static void Initialise()
        {
            if (File.Exists(paths_txt))
            {
                ReadPathFile();
                Console.WriteLine(" Found Paths file, using");
                //Console.WriteLine("  path APIToken: " + APIToken);
                Console.WriteLine("  path ImageArc: " + img_directory);
                Console.WriteLine("  path MemeDire: " + meme_directory);
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
                //string line0 = sw.ReadLine();
                string line1 = sw.ReadLine();
                string line2 = sw.ReadLine();
                if( /*line0 != null &&*/ line1 != null && line2 != null )
                {
                    //APIToken = line0;
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
                //sw.WriteLine(APIToken);
                sw.WriteLine(img_directory);
                sw.WriteLine(meme_directory);
            }
        }

    }
}
