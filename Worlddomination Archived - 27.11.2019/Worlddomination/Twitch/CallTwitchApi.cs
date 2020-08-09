using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Worlddomination.Twitch
{
    class CallTwitchApi
    {
        


        public static void GetSth(string url)
        {
            //url = "https://yourAPIurl";
            WebRequest myReq = WebRequest.Create(url);
                                  
            string credentials = "onn9jpsat3vqo6muefbfgcq9m0wl26:uujpqfrp1c6vc4s0pg7ypemefg7adg";
            CredentialCache mycache = new CredentialCache();
            myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            WebResponse wr = myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            Console.WriteLine(content);
            var json = "[" + content + "]"; // change this to array
            var objects = JArray.Parse(json); // parse as array  
            foreach (JObject o in objects.Children<JObject>())
            {
                foreach (JProperty p in o.Properties())
                {
                    string name = p.Name;
                    string value = p.Value.ToString();
                    Console.Write(name + ": " + value);
                }
            }
            Console.ReadLine();
        }

    }
}
