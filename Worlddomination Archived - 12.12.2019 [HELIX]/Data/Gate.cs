using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Worlddomination.Data
{
    class Gate
    {
        //serialise and deserialise data in this class
        public static List<string> Load( string file )
        {
            string file_path = "";
            if (file.Equals("Streamers"))
            {
                file_path = @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Data\Streamers.txt";
            }
            else if (file.Equals("Broadcast"))
            {
                file_path = @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Data\Broadcast.txt";
            }
            else if (file.Equals("ImageNames"))
            {
                file_path = @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Data\ImageNames.txt";
            }
            //more additions you just add here
            else
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [ERROR]   Unexpected param for file load method");
            }
            List<string> content = new List<string> { };
            if (File.Exists(file_path))
            {
                using (StreamReader f = new StreamReader(file_path))
                {
                    string line;
                    while ((line = f.ReadLine()) != null && line != "")
                    {
                        //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [LiveMonitor] Added " + line); //Debug Line
                        content.Add(line);
                    }
                }

            }
            return content;
        }

        //if needed add an string to the broadcast but save it regardless
        public static void SaveBroadcastLocations(string add)
        {
            List<string> current = Load("Broadcast");
            bool result = false;
            foreach( string ele in current )
            {
                if (ele.Equals(add)) result = true;
            }
            if (!result) current.Add(add);
            using (StreamWriter sw = File.CreateText(@"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\[C]DiscordBot\Worlddomination\Data\Broadcast.txt"))
            {
                foreach( string element in current )
                {
                    sw.WriteLine(element);
                }
            }
        }


        public static void MaybeAdd(string[] arr)
        {
            List<string> streamer_list = Load("Streamers");
            if (arr != null && arr.Length > 0)
            {

            }
            
            foreach( string streamer in streamer_list )
            {

            }

        }



        public static void Save()
        {
            //if its the streamer file get t
        }

    }
}



/*
 *  public static void Initialise()
        {
            MenuManager.opt_primary_autoswitch = 0; 
            if (File.Exists(textFile))
            {
                readContent();
            }
            else
            {
                uConsole.Log("-AUTOSELECTORDER- [ERROR] File does not exist. Creating default priority list");
                Debug.Log("-AUTOSELECTORDER- [ERROR] File does not exist. Creating default priority list");
                createDefaultPriorityFile();
                readContent();
            }
            AOControl.isInitialised = true;
        }

        private static void createDefaultPriorityFile()
        {
            using (StreamWriter sw = File.CreateText(textFile))
            {
                sw.WriteLine("THUNDERBOLT");
                sw.WriteLine("CYCLONE");
                sw.WriteLine("DRILLER");
                sw.WriteLine("IMPULSE");
                sw.WriteLine("FLAK");
                sw.WriteLine("SHOTGUN");
                sw.WriteLine("LANCER");
                sw.WriteLine("REFLEX");
                sw.WriteLine("DEVASTATOR");
                sw.WriteLine("NOVA");
                sw.WriteLine("TIMEBOMB");
                sw.WriteLine("HUNTER");
                sw.WriteLine("VORTEX");
                sw.WriteLine("FALCON");
                sw.WriteLine("MISSILE_POD");
                sw.WriteLine("CREEPER");
            }
        }

        private static bool stringToBool(string b)
        {
            if (b == "True")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void readContent()
        {
            using (StreamReader file = new StreamReader(textFile))
            {
                int counter = 0;
                string ln;

                //To-Do use default values if encountering sth unexpected in the file
                // (3) is not critical

                while ((ln = file.ReadLine()) != null)
                {
                    ///<summary>
                    /// Contains the priorities of the primary weapons
                    ///</summary>
                    if (counter < 8)
                    {
                        if (ln == "THUNDERBOLT" | ln == "IMPULSE" | ln == "CYCLONE" | ln == "DRILLER" | ln == "LANCER" | ln == "REFLEX" | ln == "FLAK" | ln == "SHOTGUN")
                        {
                            PrimaryPriorityArray[counter] = ln;
                        }
                        else
                        {
                            uConsole.Log("ERROR(1) while reading File, unexpected line content : " + ln);
                            Debug.Log("-AUTOSELECTORDER- [ERROR](1) unexpected line content -> (content: " + ln + " )");

                            return;
                        }

                    }
                    ///<summary>
                    /// Contains the priorities of the secondary weapons
                    ///</summary>
                    else if (counter < 16)
                    {
                        if (ln == "DEVASTATOR" | ln == "TIMEBOMB" | ln == "VORTEX" | ln == "NOVA" | ln == "HUNTER" | ln == "FALCON" | ln == "CREEPER" | ln == "MISSILE_POD")
                        {
                            SecondaryPriorityArray[counter - 8] = ln;
                        }
                        else
                        {
                            uConsole.Log("ERROR(2) while reading File, unexpected line content : " + ln);
                            Debug.Log("-AUTOSELECTORDER- [ERROR](2) unexpected line content -> (content: " + ln + " )");

                            return;
                        }
                    }
                    ///<summary>
                    /// Contains true/false whether primary priorities are neverselected
                    ///</summary>
                    else if (counter < 24)
                    {
                        if (ln == "True" || ln == "False")
                        {
                            AOSwitchLogic.PrimaryNeverSelect[counter - 16] = stringToBool(ln);
                        }
                        else
                        {
                            //if we got here, the data before is fine we just need to generate default for this
                            for (int i = 0; i < 8; i++)
                            {
                                uConsole.Log("REEEEEEEEEEEEEEEEEEEEEEE(1)");
                                AOSwitchLogic.PrimaryNeverSelect[i] = false;
                            }
                        }
                    }
                    ///<summary>
                    /// Contains true/false whether secondary priorities are neverselected
                    ///</summary>
                    else if (counter < 32)
                    {
                        if (ln == "True" || ln == "False")
                        {
                            AOSwitchLogic.SecondaryNeverSelect[counter - 24] = stringToBool(ln);
                        }
                        else
                        {
                            //if we got here, the data before is fine we just need to generate default for this
                            for (int i = 0; i < 8; i++)
                            {
                                uConsole.Log("REEEEEEEEEEEEEEEEEEEEEEE(2)");
                                AOSwitchLogic.SecondaryNeverSelect[i] = false;
                            }
                        }
                    }
                    else if (counter == 32)
                    {
                        if (ln == "True" || ln == "False") { AOControl.primarySwapFlag = stringToBool(ln);  }              
                    }
                    else if (counter == 33)
                    {
                        if (ln == "True" || ln == "False") { AOControl.secondarySwapFlag = stringToBool(ln); }       
                    }
                    else if (counter == 34)
                    {
                        if (ln == "True" || ln == "False") { AOControl.COswapToHighest = stringToBool(ln); }  
                    }
                    else if (counter == 35)
                    {
                        if (ln == "True" || ln == "False") { AOControl.patchPrevNext = stringToBool(ln); }   
                    }
                    else if (counter == 36)
                    {
                        if (ln == "True" || ln == "False") { AOControl.zorc = stringToBool(ln); }   
                    }
                  
                    else
                    {
                        // uConsole.Log("ERROR(3) while reading File, unexpected line content : " + ln);
                        Debug.Log("-AUTOSELECTORDER- [ERROR](3) unexpected line content -> (content: " + ln+ " : "+ counter + " )");

                        return;
                    }
                    counter++;
                }
                file.Close();

            }
        }
 */
