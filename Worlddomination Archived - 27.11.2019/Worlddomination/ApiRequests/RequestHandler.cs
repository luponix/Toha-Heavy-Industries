using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams;

namespace Worlddomination.ApiRequests
{
    class RequestHandler
    {
        /*
        private TwitchAPI API;
    

        public RequestHandler()
        {
            API = new TwitchAPI();
            API.Settings.ClientId = "onn9jpsat3vqo6muefbfgcq9m0wl26";
            API.Settings.AccessToken = "uujpqfrp1c6vc4s0pg7ypemefg7adg";
        }

        //await API.Helix.Streams.GetStreamsAsync(string after = null, List<string> communityIds = null, 
        //                                        int first = 20,  gameIds , List<string> languages = null, 
        //                                        string type = "all", List<string> userIds = null, List<string> userLogins = null); 
        /*
        public string GetStreamsForGameIdsAsyncWrapper( )
        {
            Task<string> task = Task.Run<string>(async () => await GetStreamsForGameIdsAsyncWrapper());
            return task.Result;
        }*/
        /*
        public async Task<string> GetStreamsForGameIdsAsync()
        {
            try
            {
                List<string> gameIds = new List<string>();
                gameIds.Add("513143");
                TwitchLib.Api.Helix.Models.Streams.GetStreamsResponse a = await API.Helix.Streams.GetStreamsAsync( gameIds: gameIds ); 
                //a.Streams.ToString
                foreach( Stream curr in a.Streams)
                {
                    Console.WriteLine(curr.Title);
                }

                return a.Streams.ToString();
            }
            catch (Exception imgurEx)
            {
                Console.WriteLine("An error occurred when trying to get the active streams.");
                Console.WriteLine(imgurEx.Message);
                return "";
            }
        }
        */
    }
}
