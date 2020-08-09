using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Worlddomination.Imgur
{
    public class Imgur
    {
        


        public string UploadedImageUrl = ""; //might not be needed

        public Imgur()
        {

        }

        public string GetImageUrl( string url)
        {
            Task<string> task = Task.Run<string>(async () => await UploadImage( url ));
            return task.Result;
        }

        private async Task<string> UploadImage( string url )
        {
            try
            {
                var client = new ImgurClient(Data.APIToken.GetImgurClientId(), Data.APIToken.GetImgurClientSecret());
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using (var fs = new FileStream( url , FileMode.Open))
                {
                    image = await endpoint.UploadImageStreamAsync(fs);
                }
               // Console.WriteLine("Image uploaded. Image Url: " + image.Link);
                UploadedImageUrl = image.Link;
                return image.Link;
            }
            catch (ImgurException imgurEx)
            {
                Console.WriteLine("An error occurred uploading an image to Imgur.");
                Console.WriteLine(imgurEx.Message);
                return "";
            }
        }



    }


   
}