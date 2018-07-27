using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EithonBot.Discord.Commands
{
    [Group("activity")]
    public class ActivityModule : SpreadsheetModuleBase
    {
        [Command]
        [Summary("Retrieves your current activity")]
        public async Task GetOwnActivity()
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var activityMessage = _spreadsheetLogic.GetActivity(familyName);
            await Context.Channel.SendMessageAsync(activityMessage);
        }

        [Command]
        [Summary("Retrieves other user's gear")]
        public async Task GetGearOfUser([Summary("The user")]IUser user)
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var activityMessage = _spreadsheetLogic.GetActivity(familyName);
            await Context.Channel.SendMessageAsync(activityMessage);
        }

        [Command]
        [Summary("Adds activity to specified activity for user")]
        public async Task UpdateActivityAsync([Summary("The Activity type")]string activity, [Summary("The user's level")]IUser user)
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var response = _spreadsheetLogic.addActivity(familyName, activity.ToUpper());
            var gearMessage = response + " Their activity is now as follows:\n" + _spreadsheetLogic.GetActivity(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }
    }
}
