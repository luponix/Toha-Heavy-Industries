using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Worlddomination.Imgur
{
    class Download
    {
        public static void DownloadUrl(string url, string imagename)
        {
            using (WebClient client = new WebClient())
            {
                string[] paths = { @"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\TohaHeavyIndustriesImages", imagename};
                string fullPath = Path.Combine(paths);
                client.DownloadFile(new Uri(url), fullPath);
                
            }
        }
    }
}
