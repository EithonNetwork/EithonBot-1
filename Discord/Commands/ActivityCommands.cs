using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Helpers;
using EithonBot.Spreadsheet.NewFolder;
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
            var memberActivity = _spreadsheetLogic.GetActivity(familyName);
            if (memberActivity == null) await Context.Channel.SendMessageAsync($"Could not find any activity for {Context.User.Mention}. Officers can add it with the ``!activity add <activity> <@user>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.UserActivityEmbed(Context.User, memberActivity));
        }

        [Command]
        [Summary("Retrieves other user's activity")]
        public async Task GetActivityOfUser([Summary("The user")]IUser user)
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var memberActivity = _spreadsheetLogic.GetActivity(familyName);
            if (memberActivity == null) await Context.Channel.SendMessageAsync($"Could not find any activity for {user.Mention}. Officers can add it with the ``!activity add <activity> <@user>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.UserActivityEmbed(user, memberActivity));
        }

        [Command("InactivityNotice")]
        [Summary("Sets your inactivity notice")]
        public async Task SetInactivityNotice([Remainder][Summary("Inactivity notice message")] string message)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateField(familyName, "InactivityNotice", message);
            var memberActivity = _spreadsheetLogic.GetActivity(familyName);
            message = response + " Your activity profile is now as follows:";
            await Context.Channel.SendMessageAsync(message, false, EmbedHelper.UserActivityEmbed(Context.User, memberActivity));
        }

        [Command("add")]
        [Summary("Adds activity to specified activity for user")]
        public async Task AddActivityAsync([Summary("The Activity type")]string activity, [Summary("The user")]IUser user)
        {
            //TODO: Fix exceptions
            if (!PermissionsHelper.UserHasRole(Context.User, "Officer")) await Context.Channel.SendMessageAsync("Officer role is required to execute this command");
            else {
                string message;
                var activityHeaders = DatabaseHelper.GetActivityHeaders(true);
                if (activityHeaders.Contains(activity))
                {
                    if (activity == "InactivityNotice") {
                        await Context.Channel.SendMessageAsync("Currently you can only add your own inactivity through the !activity inactivitynotice <message> command");
                    }
                    else {
                        var familyName = MiscHelper.GetFamilyName(Context, user);
                        var response = _spreadsheetLogic.AddActivity(familyName, activity);
                        var memberActivity = _spreadsheetLogic.GetActivity(familyName);
                        message = response + " Their activity profile is now as follows:";
                        await Context.Channel.SendMessageAsync(message, false, EmbedHelper.UserActivityEmbed(user, memberActivity));
                    }
                }
                else
                {
                    var activityHeadersString = String.Join("\n- ", activityHeaders);
                    message = $"Could not find \"{activity}\". Please make sure it is one of the following: \n- {activityHeadersString}";
                    await Context.Channel.SendMessageAsync(message);
                }
            }
        }

        [Command("remove")]
        [Summary("Adds activity to specified activity for user")]
        public async Task RemoveActivityAsync([Summary("The Activity type")]string activity, [Summary("The user")]IUser user)
        {
            //TODO: Fix exceptions
            string message;
            var activityHeaders = DatabaseHelper.GetActivityHeaders(true);
            if (activityHeaders.Contains(activity))
            {
                var familyName = MiscHelper.GetFamilyName(Context, user);
                var response = _spreadsheetLogic.RemoveActivity(familyName, activity);
                var memberActivity = _spreadsheetLogic.GetActivity(familyName);
                message = response + " Their activity is now as follows:";
                await Context.Channel.SendMessageAsync(message, false, EmbedHelper.UserActivityEmbed(user, memberActivity));
            }
            else
            {
                var activityHeadersString = String.Join("\n- ", activityHeaders);
                message = $"Could not find \"{activity}\". Please make sure it is one of the following: \n- {activityHeadersString}";
                await Context.Channel.SendMessageAsync(message);
            }
        }
    }
}
