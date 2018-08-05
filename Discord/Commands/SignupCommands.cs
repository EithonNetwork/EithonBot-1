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
    [Group("signup")]
    public class SignupModule : SpreadsheetModuleBase
    {
        [Command]
        [Summary("Retrieves your current signup data")]
        public async Task GetOwnSignupData()
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var memberActivity = SpreadsheetCommands.GetSignupData(familyName);
            if (memberActivity == null) await Context.Channel.SendMessageAsync($"Could not find any signup data for {Context.User.Mention}. Sign up by reacting to the nodewar signup announcement.");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.SignupProfileEmbed(Context.User, memberActivity));
        }

        [Command]
        [Summary("Retrieves specified user's signup data")]
        public async Task GetSignupDataOfUser([Summary("The user")]IUser user)
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var memberActivity = SpreadsheetCommands.GetSignupData(familyName);
            if (memberActivity == null) await Context.Channel.SendMessageAsync($"Could not find any signup data for {user.Mention}. They can sign up by reacting to the nodewar signup announcement");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.SignupProfileEmbed(user, memberActivity));
        }

        [Command("comment")]
        [Summary("Sets your signup comment")]
        [Priority(1)]
        public async Task SetSignupComment([Remainder][Summary("Signup comment")] string signupComment)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetCommands.UpdateField(familyName, "SignupComment", signupComment);
            var embedData = SpreadsheetCommands.GetSignupData(familyName);
            var message = response + " Your signup profile is now as follows:";
            await Context.Channel.SendMessageAsync(message, false, EmbedHelper.SignupProfileEmbed(Context.User, embedData));
        }

        [Command("comment remove")]
        [Summary("Removes your signup comment")]
        [Priority(2)]
        public async Task RemoveSignupComment()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetCommands.UpdateField(familyName, "SignupComment", "");
            var embedData = SpreadsheetCommands.GetSignupData(familyName);
            var message = response + " Your signup profile is now as follows:";
            await Context.Channel.SendMessageAsync(message, false, EmbedHelper.SignupProfileEmbed(Context.User, embedData));
        }

        //[Command]
        //[Summary("Adds activity to specified activity for user")]
        //public async Task AddActivityAsync([Summary("The Activity type")]string activity, [Summary("The user")]IUser user)
        //{
        //    //TODO: Fix exceptions
        //    if (!PermissionsHelper.UserHasRole(Context.User, "Officer")) await Context.Channel.SendMessageAsync("``Officer`` role is required to execute this command");
        //    else
        //    {
        //        string message;
        //        var activityHeaders = DatabaseHelper.GetActivityHeaders(true);
        //        if (activityHeaders.Contains(activity))
        //        {
        //            if (activity == "InactivityNotice")
        //            {
        //                message = "Currently you can only add your own inactivity through the ``!activity InactivityNotice add <message>`` command";
        //                await Context.Channel.SendMessageAsync(message);
        //            }
        //            else
        //            {
        //                var familyName = MiscHelper.GetFamilyName(Context, user);
        //                var response = SpreadsheetLogic.AddActivity(familyName, activity);
        //                var memberActivity = SpreadsheetLogic.GetActivity(familyName);
        //                message = response + " Their activity profile is now as follows:";
        //                await Context.Channel.SendMessageAsync(message, false, EmbedHelper.ActivityProfileEmbed(user, memberActivity));
        //            }
        //        }
        //        else
        //        {
        //            var activityHeadersString = String.Join("\n- ", activityHeaders);
        //            message = $"Could not find \"{activity}\". Please make sure it is one of the following: \n- {activityHeadersString}";
        //            await Context.Channel.SendMessageAsync(message);
        //        }
        //    }
        //}
    }
}
