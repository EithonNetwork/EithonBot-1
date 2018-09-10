using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Commands.Helpers;
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
            if(!CommandHelper.VerifyThatUserHasRole(Context, "Party Leader")) return;
            var errorEmoji = new Emoji("🛑");
            var emojiList = new List<Emoji>
            {
                new Emoji("1⃣"),
                new Emoji("2⃣"),
                new Emoji("3⃣"),
                new Emoji("4⃣"),
                new Emoji("5⃣")
            };

            var partyMembers = SpreadsheetInstance.CommandsParser.GetPartyMembers(partyName);
            if (partyMembers == null)
            {
                await Context.Channel.SendMessageAsync($"Could not find partyMembers of party {partyName}");
                return;
            }

            var discordUsers = new List<IUser>();
            var names = new List<string>();
            for (var i = 0; i < partyMembers.Count; i++)
            {
                var familyName = partyMembers[i].ToString();
                var discordUser = MiscHelper.GetDiscordUserFromFamilyName(Context, familyName);
                discordUsers.Add(discordUser);

                if (discordUser != null)
                {
                    names.Add($"**{discordUser.Nickname}**");
                }
                else
                {
                    emojiList[i] = errorEmoji;
                    names.Add($"Error: No user with family name ``{familyName}`` could be found on the server.");
                }
                /* names.Add((discordUser != null) ? discordUser.Nickname : familyName + " wasn't found"); */
            }

            var embed = new EmbedBuilder()
                .WithTitle("Party members of Left 1. React with their number to give activity")
                .WithDescription($"" +
                $"{emojiList[0]} - {names[0]} \n" +
                $"{emojiList[1]} - {names[1]} \n" +
                $"{emojiList[2]} - {names[2]} \n" +
                $"{emojiList[3]} - {names[3]} \n" +
                $"{emojiList[4]} - {names[4]} \n")
                .WithFooter("You have 30 seconds before this message times out.")
                .Build();

            await InlineReactionReplyAsync(new ReactionCallbackData(null, embed, false, false, TimeSpan.FromSeconds(30), (c) => c.Channel.SendMessageAsync($"{c.User.Mention} Nodewar party activity message timed out."))
                .WithCallback(emojiList[0], (c, r) => CommandHelper.GiveActivityToMember(SpreadsheetInstance, Context, discordUsers[0], "NW"))
                .WithCallback(emojiList[1], (c, r) => CommandHelper.GiveActivityToMember(SpreadsheetInstance, Context, discordUsers[1], "NW"))
                .WithCallback(emojiList[2], (c, r) => CommandHelper.GiveActivityToMember(SpreadsheetInstance, Context, discordUsers[2], "NW"))
                .WithCallback(emojiList[3], (c, r) => CommandHelper.GiveActivityToMember(SpreadsheetInstance, Context, discordUsers[3], "NW"))
                .WithCallback(emojiList[4], (c, r) => CommandHelper.GiveActivityToMember(SpreadsheetInstance, Context, discordUsers[4], "NW")), true
            );
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
            //if (!PermissionsHelper.VerifyThatUserHasRole(Context.User, "Officer")) await Context.Channel.SendMessageAsync("``Officer`` role is required to execute this command");
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
