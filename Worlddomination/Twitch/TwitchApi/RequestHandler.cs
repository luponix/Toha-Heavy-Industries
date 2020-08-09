using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Helix.Models.Users;

namespace Worlddomination.Twitch.TwitchApi
{
    public class RequestHandler
    {        
        /*
        public static Stream[] CallGetStreamsForGameIdsAsyncAndWaitOnResult(int limit, string gameid)
        {
            var task = GetStreamsForGameIdsAsync(limit, gameid);
            task.Wait(); // Blocks current thread until GetFooAsync task completes
                         // For pedagogical use only: in general, don't do this!
            Stream[] result = task.Result;
            return result;
        }*/
        public static async Task<Stream[]> GetStreamsForGameIdsAsync(int limit, string gameid)
        {
            Stream[] result = new Stream[limit];

            try
            {
                TwitchAPI API = new TwitchAPI();
                API.Settings.ClientId = "onn9jpsat3vqo6muefbfgcq9m0wl26";
                API.Settings.Secret = "212b473uo803kull6iw707sq37cmzo";
                API.Settings.AccessToken = "uujpqfrp1c6vc4s0pg7ypemefg7adg";

                List<string> gameIds = new List<string>();
                gameIds.Add(gameid);
                TwitchLib.Api.Helix.Models.Streams.GetStreamsResponse a = await API.Helix.Streams.GetStreamsAsync(gameIds: gameIds, first: limit);
                int i = 0;

                foreach (Stream curr in a.Streams)
                {
                    Console.WriteLine(curr.Title);
                    result[i] = curr;
                    i++;
                }
                return result;

            }
            catch (Exception imgurEx)
            {
                Console.WriteLine("An error occurred when trying to get the active streams.");
                Console.WriteLine(imgurEx.Message);
                return result;
            }
        }





        /*
        public List<string> CallGetNamesAndProfileUrlsAsyncAndWaitOnResult(List<string> ids)
        {
            var task = GetNamesAndProfileUrlsAsync(ids);
            task.Wait(); // Blocks current thread until GetFooAsync task completes
                         // For pedagogical use only: in general, don't do this!
            var result = task.Result;
            return result;
        }*/
        public static async Task<List<string>> GetNamesAndProfileUrlsAsync(List<string> ids)
        {
            List<string> result = new List<string>();

            try
            {
                TwitchAPI API = new TwitchAPI();
                API.Settings.ClientId = "onn9jpsat3vqo6muefbfgcq9m0wl26";
                API.Settings.Secret = "212b473uo803kull6iw707sq37cmzo";
                API.Settings.AccessToken = "uujpqfrp1c6vc4s0pg7ypemefg7adg";


                GetUsersResponse v = await API.Helix.Users.GetUsersAsync(ids: ids);
                foreach (User curr in v.Users)
                {
                    //onsole.WriteLine(curr.ProfileImageUrl);
                    //Console.WriteLine(curr.DisplayName);

                    result.Add(curr.ProfileImageUrl);
                    result.Add(curr.DisplayName);
                }
                return result;

            }
            catch (Exception imgurEx)
            {
                Console.WriteLine("An error occurred when trying to get the display names and images.");
                Console.WriteLine(imgurEx.Message);
                return result;
            }
        }















    }
}
/*
 *   
                foreach (Stream curr in n_stream)
                {
                    user_ids.Add(curr.UserId);
                }
            
           

            List<string> mixed = await a.GetNamesAndProfileUrlsAsync(user_ids);
            List<string> n_profile = new List<string>();
            List<string> n_name = new List<string>();

            int i = 0;
            int j = 0;
            foreach( string selected in mixed)
            {
                if( mixed.IndexOf(selected) % 2 == 0   )
                {
                    n_profile[i] = selected;
                    i++;
                }
                else
                {
                    n_name[j] = selected;
                    j++;
                }
            }
            // TEST: print all the necessary informations:
            int counter = 0;
            foreach( Stream curr in n_stream)
            {
                DateTime dateTimeNow = DateTime.Now;
                int sec = (dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond;
                string name = sec.ToString();

                string thumUrl = curr.ThumbnailUrl;
                thumUrl = thumUrl.Replace("{width}", "480");
                thumUrl = thumUrl.Replace("{height}", "270");

                string[] paths = { @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\TohaHeavyIndustriesImages", name };
                string fullPath = Path.Combine(paths);

                Imgur.Download.DownloadUrl(thumUrl, fullPath);

                Imgur.Imgur imgur = new Imgur.Imgur();
                string finallink = imgur.GetImageUrl(fullPath);

                string url = "https://www.twitch.tv/" + n_name[counter];



                Discord.EmbedBuilder embed = new EmbedBuilder
                {
                    Title = curr.Title,
                };
                embed.AddField(n_name[counter] + " is streaming " + Format.Bold(game_id), "under " + url);
                embed.ImageUrl = finallink;
                embed.WithColor(Color.Blue);
                embed.WithUrl(url);
                embed.WithCurrentTimestamp();
                Discord.Embed embedded = embed.Build();

                Misc.SendEmbedWithoutContext("", embedded, "twitch-output", "data");
                counter++;
 */
