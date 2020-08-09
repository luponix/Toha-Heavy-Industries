using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Worlddomination
{
    class Misc
    {
        public static void SendTextMessage(DiscordSocketClient client, string channelName, string message)
        {
           // foreach( SocketGuildChannel channel in client.GetGuild(647786907211923456))
        }

        public static void SendMessageWithoutContext(string message, ulong channel_id)
        {
            SocketChannel channel = Program._client.GetChannel(channel_id);
            (channel as SocketTextChannel).SendMessageAsync(message);
        }

        internal static void SendMessageWithoutContext( string message, string channelName, string guildName = null)
        {
            if (String.IsNullOrEmpty(guildName)) guildName = "data";

            SocketGuild guild = Program._client.Guilds.FirstOrDefault(x => x.Name.ToLower() == guildName.ToLower());
            if (guild == null) return;

            SocketChannel channel = guild.Channels.FirstOrDefault(x => (x is SocketTextChannel) && (x.Name.ToLower() == channelName.ToLower()));
            if (channel == null) return;

            (channel as SocketTextChannel).SendMessageAsync(message);
            
        }


        internal static void SendEmbedWithoutContext(string message, Discord.Embed embed, string channelName, string guildName = null)
        {
            if (String.IsNullOrEmpty(guildName)) guildName = "data";

            SocketGuild guild = Program._client.Guilds.FirstOrDefault(x => x.Name.ToLower() == guildName.ToLower());
            if (guild == null) return;

            SocketChannel channel = guild.Channels.FirstOrDefault(x => (x is SocketTextChannel) && (x.Name.ToLower() == channelName.ToLower()));
            if (channel == null) return;

            (channel as SocketTextChannel).SendMessageAsync(message, false, embed);

        }


    }
}
