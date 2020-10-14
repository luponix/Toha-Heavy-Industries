using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Worlddomination.Commands
{
    class Permissions
    {

        private static Dictionary<string, int> users = new Dictionary<string, int>();

        // populate users
        public static void Initialise()
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = Program.sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Permissions";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string id = sqlite_datareader.GetValue(0).ToString();
                int level = (int)sqlite_datareader.GetValue(1);
                users.Add( id, level);
            }
        }


        public static void Add( string user, int perm )
        {
            if( user != null)
            {
                users.Add(user,perm);
            }
            else
            {
                Console.WriteLine("Error: Permissions.Add - empty user provided");
            }
        }

        public static void Remove(string user)
        {
            if (user != null)
            {
                users.Remove(user);
            }
            else
            {
                Console.WriteLine("Error: Permissions.Remove - empty user provided");
            }
        }


        public static bool IsUserAuthorized( string user, int minPermLevel )
        {
            if(!users.ContainsKey(user))
            {
                return false;
            }
            int perm;
            users.TryGetValue(user, out perm);
            return perm <= minPermLevel;
        }
    }
}
