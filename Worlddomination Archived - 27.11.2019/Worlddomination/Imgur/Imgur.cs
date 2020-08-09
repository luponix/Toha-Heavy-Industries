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
        // Client ID : a485389d553eac3
        // Client Secret : 9bb54d30101594266d08f5f76b35b55e7d1a590b

        // Access Token : ce2a9b4faca1a59dede1c2ef5e5dd1f4f8f3945a INVALID
        // Refresh Token : e0a802127d73ed2829882f958b08abf83bc4ba01 INVALID
        // Account ID : 118693615
        // Acc Username : Iuponix


        /*
         *     "access_token": "a41058bc19c1ab495dcea0bb72cf8919a79a70dd",
               "expires_in": 315360000,
               "token_type": "bearer",
               "scope": "",
               "refresh_token": "8ea907c4565db6fbea70734ba60983885f842eef",
               "account_id": 118693615,
               "account_username": "Iuponix"

              Token : 5eeae49394cd929e299785c8805bd168fc675280

         */


        public string UploadedImageUrl = ""; //might not be needed

        public Imgur()
        {

        }

        public string GetImageUrl( string url)
        {
            Task<string> task = Task.Run<string>(async () => await UploadImage( url ));
            return task.Result;
        }

        public async Task<string> UploadImage( string url )
        {
            try
            {
                var client = new ImgurClient("a485389d553eac3", "9bb54d30101594266d08f5f76b35b55e7d1a590b");
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using (var fs = new FileStream( url , FileMode.Open))
                {
                    image = await endpoint.UploadImageStreamAsync(fs);
                }
                Console.WriteLine("Image uploaded. Image Url: " + image.Link);
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