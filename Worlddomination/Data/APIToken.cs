using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Worlddomination.Data
{
    class APIToken
    {
        private static string discord_token, twitch_id, twitch_secret, twitch_token, imgur_id, imgur_secret;

        //Discord API
        public static string GetDiscordClientToken()
        {
            return discord_token;
        }


        //Twitch API
        public static string GetTwitchClientId()
        {
            return twitch_id;
        }
        public static string GetTwitchClientSecret()
        {
            return twitch_secret;
        }
        public static string GetTwitchAccessToken()
        {
            return twitch_token;
        }



        //Imgur API
        public static string GetImgurClientId()
        {
            return imgur_id;
        }
        public static string GetImgurClientSecret()
        {
            return imgur_secret;
        }


        public static void Populate()
        {
            try
            {
                using (StreamReader sw = new StreamReader(Paths.APIToken))
                {
                    discord_token = sw.ReadLine();
                    twitch_id = sw.ReadLine();
                    twitch_secret = sw.ReadLine();
                    twitch_token = sw.ReadLine();
                    imgur_id = sw.ReadLine();
                    imgur_secret = sw.ReadLine();

                    if (!String.IsNullOrEmpty(discord_token)
                         || !String.IsNullOrEmpty(twitch_id)
                         || !String.IsNullOrEmpty(twitch_secret)
                         || !String.IsNullOrEmpty(twitch_token)
                         || !String.IsNullOrEmpty(imgur_id)
                         || !String.IsNullOrEmpty(imgur_secret)
                        )
                    {
                        Console.WriteLine("Successfully populated ApiTokens");
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine("Successfully populated ApiTokens");
                        System.Environment.Exit(1);
                    }
                }
            }
            catch( Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");
                System.Environment.Exit(1); 
            }
           
        }

    }
}


