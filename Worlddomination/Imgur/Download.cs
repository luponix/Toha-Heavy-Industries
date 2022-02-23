using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Worlddomination.Imgur
{
    class Download
    {
        //this could be send to Misc
        public static void DownloadUrl(string url, string imagename)
        {
            using (WebClient client = new WebClient())
            {

                //string[] paths = { @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\TohaHeavyIndustriesImages", imagename};
                //string[] paths = { @"E:\TohaHeavyIndustries Images Archive", imagename};
                //string fullPath = Path.Combine(paths);
                //Console.WriteLine("[LOG] DownloadUrl: param: "+url+"    "+imagename);
                try
                {
                    client.DownloadFile(new Uri(url), imagename);
                }
                catch(Exception e)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    [ERROR]   Download.DownloadUrl failed");
                    //Console.WriteLine(url+" "+ imagename);
                    // download https://i.imgur.com/ftxM106.png instead if the exception actually gets thrown here
                    // if it doesnt we can also move this method into stream monitor
                    client.DownloadFile(new Uri("https://i.imgur.com/ftxM106.png"), imagename);
                }
            }
        }
    }
}
