using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Helpers;
using EithonBot.Spreadsheet.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EithonBot.Discord.Commands.Helpers
{
    class CommandHelper
    {
        public static Task GiveActivityToMember(SpreadsheetInstance spreadsheetInstance, SocketCommandContext context, IUser user, string activity)
        {
            var familyName = MiscHelper.GetFamilyName(context, user);
            if (familyName == null) return null;
            var response = spreadsheetInstance.CommandsParser.ChangeActivity(familyName, activity, "add");

            if (response == null) return context.Channel.SendMessageAsync($"Could not update field {activity} for {familyName}. Get Zil to add proper error messages please");
            //var activityHeadersString = String.Join("\n- ", activityHeaders);
            //message = $"Could not find \"{activity}\". Please make sure it is one of the following: \n- {activityHeadersString}";
            //await Context.Channel.SendMessageAsync(message); 
            else
            {
                var message = $"Added one {activity} activity for {user.Mention}. Check their updated activity profile with '!activity {user.Mention}'";
                return context.Channel.SendMessageAsync(message);
            }
        }
    }
}
