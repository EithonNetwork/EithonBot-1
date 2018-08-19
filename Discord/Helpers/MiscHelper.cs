using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EithonBot.Discord.Helpers
{
    class MiscHelper
    {
        public static string GetFamilyName(SocketCommandContext context, IUser user)
        {
            var guildUser = (SocketGuildUser)user;
            try
            {
                return guildUser.Nickname.Substring(0, guildUser.Nickname.IndexOf(" "));
            }
            //TODO: Properly handle the exception
            catch
            {
                context.Channel.SendMessageAsync("Could not parse family name of command user. Please make sure that the nickname of the user is in the correct format: **<FamilyName> (CharacterName)**");
                return null;
            }
        }

        public static SocketGuildUser GetDiscordUserFromFamilyName(SocketCommandContext context, string familyName)
        {
            var matchingUsers = context.Guild.Users.Where(user => user.Nickname != null && user.Nickname.Contains(familyName));
            try
            {
                var user = matchingUsers.FirstOrDefault();
                return user;
            }
            catch
            {
                return null;
            }
        }

        public static string GetCharacterName(SocketCommandContext context, IUser user)
        {
            var guildUser = (SocketGuildUser)user;
            try
            {
                string input = guildUser.Nickname;
                return input.Remove(input.IndexOf(')')).Substring(input.IndexOf('(') + 1);
            }
            //TODO: Properly handle the exception
            catch
            {
                context.Channel.SendMessageAsync("Could not parse character name of command user. Please make sure that the nickname of the user is in the correct format: **<FamilyName> (<CharacterName>)**");
                throw new ApplicationException();
            }
        }

        public static string GetClassRoleOfUser(IUser user)
        {
            var guildUser = (SocketGuildUser)user;

            var classRoles = GetClassRolesOfGuild();
            foreach (var role in classRoles)
            {
                if (guildUser.Roles.Any(r => r.Name == role)) return role;
            }
            return null;
        }

        public static List<string> GetClassRolesOfGuild()
        {
            var classList = new List<string>
            {
                "Warrior",
                "Ranger",
                "Sorceress",
                "Berserker",
                "Tamer",
                "Valkyrie",
                "Witch",
                "Wizard",
                "Kunoichi",
                "Ninja",
                "Maehwa",
                "Musa",
                "Dark Knight",
                "Striker",
                "Mystic"
            };
            return classList;
        }
    }
}
