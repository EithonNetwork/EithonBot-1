using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EithonBot.Discord.Helpers
{
    class PermissionsHelper
    {
        public static bool UserHasRole(SocketGuildUser user, string roleName)
        {
            if (user.Roles.Any(r => r.Name == roleName)) return true;
            return false;
        }
    }
}
