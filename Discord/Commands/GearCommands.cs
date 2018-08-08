using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Helpers;
using EithonBot.Spreadsheet.NewFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EithonBot.Discord.Commands
{
    [Group("gear")]
    public class GearModule : SpreadsheetModuleBase
    {

        [Command]
        [Summary("Retrieves your own gear")]
        public async Task GetOwnGear()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var memberGear = SpreadsheetInstance.CommandsParser.GetGear(familyName);
            if (memberGear == null) await Context.Channel.SendMessageAsync($"Could not find any gear for {Context.User.Mention}. You can add it with the ``!gear <stat> <value>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.GearProfileEmbed(Context.User, memberGear));
        }

        [Command]
        [Summary("Retrieves other user's gear")]
        public async Task GetGearOfUser([Summary("The user")]IUser user)
        {
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var memberGear = SpreadsheetInstance.CommandsParser.GetGear(familyName);
            if (memberGear == null) await Context.Channel.SendMessageAsync($"Could not find any gear for {user.Mention}. They can add it with the ``!gear <stat> <value>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.GearProfileEmbed(user, memberGear));
        }

        //TODO: Create a !gear help command (using the summaries already added?)


        [Command("comment")]
        [Summary("Sets your gear comment")]
        [Priority(2)]
        public async Task SetGearComment([Remainder][Summary("Gear comment message")] string gearComment)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetInstance.CommandsParser.UpdateField(familyName, "GearComment", gearComment);
            SpreadsheetInstance.CommandsParser.UpdateField(familyName, "Activity last updated", DateTime.Now.ToString());
            var message = response + " Check your updated gear profile with with ``!gear``";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("comment remove")]
        [Summary("Removes your gear comment")]
        [Priority(3)]
        public async Task RemoveGearComment()
        {
            //TODO: Make sure you can only update intended columns
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetInstance.CommandsParser.UpdateField(familyName, "GearComment", "");
            SpreadsheetInstance.CommandsParser.UpdateField(familyName, "Gear last updated", DateTime.Now.ToString());
            var message = response + " Check your updated gear profile with with ``!gear``";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("GearComment remove")]
        [Summary("Removes your GearComment")]
        [Priority(3)]
        public async Task RemoveGearComment2()
        {
            //TODO: Make sure you can only update intended columns
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetInstance.CommandsParser.UpdateField(familyName, "GearComment", "");
            SpreadsheetInstance.CommandsParser.UpdateField(familyName, "Gear last updated", DateTime.Now.ToString());
            var message = response + " Check your updated gear profile with with ``!gear``";
            await Context.Channel.SendMessageAsync(message);
        }


        [Command]
        [Summary("Sets the selected stat to the specified value")]
        [Priority(1)]
        public async Task UpdateStatAsync([Summary("The stat to be changed")] string stat, [Remainder][Summary("The value of stat")] string value)
        {
            string message;
            var ch = Context.Channel;
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetInstance.CommandsParser.UpdateField(familyName, stat, value);
            //TODO: Improve error messages (What went wrong)
            if (response == null) await ch.SendMessageAsync($"Could not update field {stat} for {familyName}. Get Zil to add proper error messages please");
            else
            {
                var lastUpdatedResponse = SpreadsheetInstance.CommandsParser.UpdateField(familyName, "Gear last updated", DateTime.Now.ToString());
                if (lastUpdatedResponse == null) await ch.SendMessageAsync($"Could not update last updated date for {familyName}. Get Zil to add proper error messages please");
                message = $"Updated {response.Column.ColumnHeader}. Check your updated gear profile with with ``!gear``";
                await ch.SendMessageAsync(message);
            }
        }
    }
}
