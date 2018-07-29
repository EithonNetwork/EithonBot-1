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
        public static bool UserHasRole(IUser user, string roleName)
        {
            var guildUser = (SocketGuildUser)user;
            if (guildUser.Roles.Any(r => r.Name == roleName)) return true;
            return false;
        }
    }
}
