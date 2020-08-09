using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Discord;
using Discord.Webhook;


namespace DiscordOSPLBot
{
    public class Common
    {
        internal DiscordWebhookClient discordWebhookClient = null;

        internal static ulong ClientId
        {
            get { return Convert.ToUInt64(ConfigurationManager.AppSettings["ClientId"]); }
        }

        internal static ulong GuildId
        {
            get { return Convert.ToUInt64(ConfigurationManager.AppSettings["GuildId"]); }
        }
   
        internal static ulong BotId
        {
            get { return Convert.ToUInt64(ConfigurationManager.AppSettings["BotId"]); }
        }

        internal static string Format(DateTime dateTime)
        {
            return dateTime.ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        internal const string MaxBuilderFieldString = "This is the title - repeat - repeat - repeat - repeat - repeat - repeat...";

        internal static int MaxFieldSize
        {
            get { return MaxBuilderFieldString.Length; }
        }

        public static string Caps(string str)
        {
            if (str == null) return null;
            return str.Substring(0, 1).ToString().ToUpper() + str.Substring(1);
        }

        internal static string[] TimeZones = new string[] { "CET", "CMT", "PST", "MST", "EST", "UTC" };
  
        internal static bool IsValidTimeZone(string tz)
        {
            for (int i = 0; i < TimeZones.Length; i++) if (tz.ToUpper() == TimeZones[i]) return true;
            return false;
        }

        internal static Color EmbedColor
        {
            get { return Color.Green; }
        }

        internal static string RemoveMentions(string message)
        {
            string result = "";
            int i = 0;

            while (i < message.Length)
            {
                if ((i < (message.Length - 1)) && (message[i] == '<') && (message[i + 1] == '@'))
                {
                    while ((i < message.Length) && (message[i] != ' ')) i++;
                }
                else
                {
                    result += message[i++];
                }
            }

            return result.Replace("  ", " ");
        }

        internal static string ConnectionString
        {
            get { return ConfigurationManager.AppSettings["ConnectionString"]; }
        }

        internal static string MailFrom
        {
            get { return ConfigurationManager.AppSettings["MailFrom"]; }
        }

        internal static string MailPassword
        {
            get { return ConfigurationManager.AppSettings["MailPassword"]; }
        }

        internal static void SqlCleanUp(SqlCommand command)
        {
            try { if (command.Transaction != null) command.Transaction.Rollback(); } catch { }
            try { if (command.Connection != null) command.Connection.Close(); } catch { }
            try { if (command.Connection != null) command.Connection.Dispose(); } catch { }
            try { if (command != null) command.Dispose(); } catch { }
        }

        internal static string SqlDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        internal static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // https://docs.microsoft.com/en-us/previous-versions/windows/embedded/ms912391(v=winembedded.11)?redirectedfrom=MSDN

        public static DateTime GetNetworkTime()
        {
            const string ntpServer = "pool.ntp.org";
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();
            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
            return networkDateTime;
        }

        internal static DateTime UTC(DateTime localDateTime)
        {
            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime);
            return utcDateTime;
        }

        internal static DateTime EST(DateTime utcDateTime)
        {
            DateTime estDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            return estDateTime;
        }

        internal static DateTime PST(DateTime utcDateTime)
        {
            DateTime estDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            return estDateTime;
        }

        internal static DateTime MST(DateTime utcDateTime)
        {
            DateTime estDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time"));
            return estDateTime;
        }

        internal static DateTime CET(DateTime utcDateTime)
        {
            DateTime estDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
            return estDateTime;
        }

        internal static DateTime GMT(DateTime utcDateTime)
        {
            DateTime estDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            return estDateTime;
        }
        
        internal static bool SendEmail(string name, string email, string subject, string content)
        {
            var fromAddress = new MailAddress(MailFrom, "Overload.DK");
            var toAddress = new MailAddress(email, name);

            if (email.Contains("drop.com")) email = "mickdk2010@gmail.com";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(MailFrom, MailPassword),
                Timeout = 20000
            };

            try
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = content
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}