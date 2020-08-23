using System;
using System.Collections.Generic;
using System.Text;

namespace Worlddomination.Commands
{
    class Permissions
    {
        private static Dictionary<string, int> userPerm = new Dictionary<string, int>();

        public static void Add( string user, int perm )
        {
            if( user != null )
            {
                userPerm.Add(user,perm);
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
                userPerm.Remove(user);
            }
            else
            {
                Console.WriteLine("Error: Permissions.Remove - empty user provided");
            }
        }


        public static bool IsUserAuthorized( string user, int minPermLevel )
        {
            if(!userPerm.ContainsKey(user))
            {
                return false;
            }
            int perm;
            userPerm.TryGetValue(user, out perm);
            return perm <= minPermLevel;
        }
    }
}
