using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Worlddomination
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class Commands : ModuleBase<SocketCommandContext>
    {
        

        [Command("ctx")]
        [Summary("")]
        public async Task QueryTwitch()
        {
            if ((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("luponix#5950"))
            {
                ulong c_id = Context.Channel.Id;
                ulong g_id = Context.Guild.Id;
                
                Data.Gate.SaveBroadcastLocations(c_id.ToString());
                Data.Gate.SaveBroadcastLocations(g_id.ToString());
                //Misc.SendMessageWithoutContext("luponix#5950 ->", "music", "data");


                Discord.EmbedBuilder embed = new EmbedBuilder
                {

                    Title = "Hello world!",
                    Description = "I am a description set by initializer."

                };

                embed.AddField("Field title",
               "Field value. I also support [hyperlink markdown](https://example.com)!");

                embed.WithFooter(footer => footer.Text = "I am a footer.");
                embed.WithColor(Color.Blue);
                embed.WithTitle("I overwrote \"Hello world!\"");
                embed.WithDescription("I am a description.");
                embed.WithUrl("https://example.com");
                embed.WithCurrentTimestamp();
                Discord.Embed a = embed.Build();

                Misc.SendEmbedWithoutContext("message1", a, "twitch-output", "data");


            }
            else
            {
                await Context.Channel.SendMessageAsync("No");
            }
        }

        [Command("upload")]
        [Summary("saves channel and guild id to Broadcast")]
        public async Task RegisterChannelId()
        {
            if ((Context.Message.Author.Username+"#"+Context.Message.Author.Discriminator).Equals("luponix#5950"))
            {
                /*
                ulong c_id = Context.Channel.Id;
                ulong g_id = Context.Guild.Id;
                string c_name = Context.Channel.Name;
                string g_name = Context.Guild.Name;
                Data.Gate.SaveBroadcastLocations(c_id.ToString());
                Data.Gate.SaveBroadcastLocations(g_id.ToString());
                await Context.Channel.SendMessageAsync("Success: g-"+g_name+"| c-"+c_name); */
                Console.WriteLine("Received Upload request");
                Imgur.Imgur imgur = new Imgur.Imgur();
                
                 //   https://icatcare.org/app/uploads/2018/07/Thinking-of-getting-a-cat.png
                string upload = imgur.GetImageUrl(@"C:\Users\luponix\Desktop\[TOHA HEAVY INDUSTRIES]\imgurTest.png");
                await Context.Channel.SendMessageAsync(upload);

            }
            else
            {
                await Context.Channel.SendMessageAsync((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator));
            }//491757
        }

        /*
        [Command("GetStreams")]
        [Summary("hopefully sends an API request for a game id")]
        public async Task GetStreams()
        {
            if ((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator).Equals("luponix#5950"))
            {
                ApiRequests.RequestHandler ApiRequestHandler = new ApiRequests.RequestHandler();
                
                string result = await ApiRequestHandler.GetStreamsForGameIdsAsync();

                await Context.Channel.SendMessageAsync(result);

            }
            else
            {
                await Context.Channel.SendMessageAsync((Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator));
            }//491757
        }*/



        // add command to get guild id and channel id as broadcast targets

        // add command to add streamers to track

        // add command to create a minesweeper field

    }
}
