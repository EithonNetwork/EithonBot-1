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
            var memberActivity = SpreadsheetInstance.CommandsParser.GetActivity(familyName);
            if (memberActivity == null) await Context.Channel.SendMessageAsync($"Could not find any activity for {Context.User.Mention}. Officers can add it with the ``!activity add <activity> <@user>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.ActivityProfileEmbed(Context.User, memberActivity));
        }

        [Command]
        [Summary("Retrieves other user's activity")]
        public async Task GetActivityOfUser([Summary("The user")]IUser user)
        {
            //TODO: Fix exceptions
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var memberActivity = SpreadsheetInstance.CommandsParser.GetActivity(familyName);
            if (memberActivity == null) await Context.Channel.SendMessageAsync($"Could not find any activity for {user.Mention}. Officers can add it with the ``!activity add <activity> <@user>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.ActivityProfileEmbed(user, memberActivity));
        }

        [Command("InactivityNotice")]
        [Alias("comment")]
        [Summary("Sets your inactivity notice")]
        [Priority(1)]
        public async Task SetInactivityNotice([Remainder][Summary("Inactivity notice message")] string inactivityNotice)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetInstance.CommandsParser.UpdateSingleField(familyName, "InactivityNotice", inactivityNotice);
            if (response == null)
            {
                await Context.Channel.SendMessageAsync("Could not execute command. Get Zil to add proper error messages please");
                return;
            }
            SpreadsheetInstance.CommandsParser.UpdateSingleField(familyName, "Activity last updated", DateTime.Now.ToString());
            var message = "Set your InactivityNotice. Check your updated activity profile with !activity";
            await Context.Channel.SendMessageAsync(message);
        }

        //Prioritized
        [Command("InactivityNotice remove")]
        [Alias("comment remove")]
        [Summary("Removes your inactivity notice")]
        [Priority(2)]
        public async Task RemoveInactivityNotice()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetInstance.CommandsParser.UpdateSingleField(familyName, "InactivityNotice", "");
            if (response == null)
            {
                await Context.Channel.SendMessageAsync("Could not execute command. Get Zil to add proper error messages please");
                return;
            }
            SpreadsheetInstance.CommandsParser.UpdateSingleField(familyName, "Activity last updated", DateTime.Now.ToString());
            var memberActivity = SpreadsheetInstance.CommandsParser.GetActivity(familyName);
            var message = "Removed your inactivity notice. Check your updated activity profile activity with '!activity";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("nodewarparty")]
        [Summary("Adds activity to specified activity for user")]
        public async Task AddPartyActivityAsync([Remainder][Summary("The Activity type")]string partyName)
        {
            //TODO: Fix exceptions
            if (!PermissionsHelper.UserHasRole(Context.User, "Party Leader")) await Context.Channel.SendMessageAsync("``Party Leader`` role is required to execute this command");
            else
            {
                string message = $"**{partyName} members**";
                var partyMembers = SpreadsheetInstance.CommandsParser.GetPartyMembers(partyName);

                for (var i = 0; i < partyMembers.Count; i++)
                {
                    var partyMember = partyMembers[i];
                    message = $"{message}\n{i + 1} {partyMember}";
                }

                var oneEmoji = new Emoji("1⃣");
                var twoEmoji = new Emoji("2⃣");
                var threeEmoji = new Emoji("3⃣");
                var fourEmoji = new Emoji("4⃣");
                var fiveEmoji = new Emoji("5⃣");

                message = $"{message}\n*React with their number to give activity*";
                await MessageHelper.SendMessageWithReactionsAsync(Context.Channel, message, false, null, oneEmoji, twoEmoji, threeEmoji, fourEmoji, fiveEmoji);
                //TODO: Do something with this
            }
        }

        [Command("add")]
        [Summary("Adds activity to specified activity for user")]
        public async Task AddActivityAsync([Summary("The Activity type")]string activity, [Summary("The user")]IUser user)
        {
            string message;
            if (activity == "InactivityNotice")
            {
                message = "Currently you can only add your own inactivity through the ``!activity InactivityNotice add <message>`` command";
                await Context.Channel.SendMessageAsync(message);
                return;
            }
            //TODO: Fix exceptions
            if (!PermissionsHelper.UserHasRole(Context.User, "Officer")) await Context.Channel.SendMessageAsync("``Officer`` role is required to execute this command");
            else
            {
                var familyName = MiscHelper.GetFamilyName(Context, user);
                var response = SpreadsheetInstance.CommandsParser.ChangeActivity(familyName, activity, "add");

                if (response == null) await Context.Channel.SendMessageAsync($"Could not update field {activity} for {familyName}. Get Zil to add proper error messages please");
                //var activityHeadersString = String.Join("\n- ", activityHeaders);
                //message = $"Could not find \"{activity}\". Please make sure it is one of the following: \n- {activityHeadersString}";
                //await Context.Channel.SendMessageAsync(message); 
                else
                {
                    var memberActivity = SpreadsheetInstance.CommandsParser.GetActivity(familyName);
                    message = $"Added one {activity} activity for {user.Mention}. Check their updated activity profile with '!activity {user.Mention}'";
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
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var response = SpreadsheetInstance.CommandsParser.ChangeActivity(familyName, activity, "remove");
            if (response == null) await Context.Channel.SendMessageAsync($"Could not update field {activity} for {familyName}. Get Zil to add proper error messages please");
            //var activityHeadersString = String.Join("\n- ", activityHeaders);
            //message = $"Could not find \"{activity}\". Please make sure it is one of the following: \n- {activityHeadersString}";
            //await Context.Channel.SendMessageAsync(message); else
            else
            {
                var memberActivity = SpreadsheetInstance.CommandsParser.GetActivity(familyName);
                message = $"Removed one {activity} activity for {user.Mention}. Check their updated activity profile with '!activity {user.Mention}'";
                await Context.Channel.SendMessageAsync(message);
            }
        }
    }
}
