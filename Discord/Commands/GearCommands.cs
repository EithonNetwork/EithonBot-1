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
    [Group("gear")]
    public class GearModule : SpreadsheetModuleBase
    {

        [Command]
        [Summary("Retrieves your own gear")]
        public async Task GetOwnGear()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var memberGear = SpreadsheetLogic.GetGear(familyName);
            if (memberGear == null) await Context.Channel.SendMessageAsync($"Could not find any gear for {Context.User.Mention}. You can add it with the ``!gear <stat> <value>`` command");
            else await Context.Channel.SendMessageAsync("",false, EmbedHelper.GearProfileEmbed(Context.User, memberGear));
        }

        [Command]
        [Summary("Retrieves other user's gear")]
        public async Task GetGearOfUser([Summary("The user")]IUser user)
        {
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var memberGear = SpreadsheetLogic.GetGear(familyName);
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
            var response = SpreadsheetLogic.UpdateField(familyName, "GearComment", gearComment);
            SpreadsheetLogic.UpdateField(familyName, "Activity last updated", DateTime.Now.ToString());
            var message = response + " Check your updated gear profile with with ``!gear``";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("comment remove")]
        [Summary("Removes your gear comment")]
        [Priority(3)]
        public async Task RemoveGearComment()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetLogic.UpdateField(familyName, "GearComment", "");
            SpreadsheetLogic.UpdateField(familyName, "Gear last updated", DateTime.Now.ToString());
            var message = response + " Check your updated gear profile with with ``!gear``";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("GearComment remove")]
        [Summary("Removes your GearComment")]
        [Priority(3)]
        public async Task RemoveInactivityNotice()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = SpreadsheetLogic.UpdateField(familyName, "GearComment", "");
            SpreadsheetLogic.UpdateField(familyName, "Gear last updated", DateTime.Now.ToString());
            var message = response + " Check your updated gear profile with with ``!gear``";
            await Context.Channel.SendMessageAsync(message);
        }


        [Command]
        [Summary("Sets the selected stat to the specified value")]
        [Priority(1)]
        public async Task UpdateStatAsync([Summary("The stat to be changed")] string stat, [Remainder][Summary("The value of stat")] string value)
        {
            string message;
            var gearHeaders = DatabaseHelper.GetGearHeaders(true);
            if (gearHeaders.Contains(stat))
            {
                var familyName = MiscHelper.GetFamilyName(Context, Context.User);
                var response = SpreadsheetLogic.UpdateField(familyName, stat, value);
                SpreadsheetLogic.UpdateField(familyName, "Gear last updated", DateTime.Now.ToString());
                message = response + " Check your updated gear profile with with ``!gear``";
                await Context.Channel.SendMessageAsync(message);
            }
            else {
                var gearHeadersString = String.Join("\n- ", gearHeaders);
                message = $"Could not find \"{stat}\". Please make sure it is one of the following: \n- {gearHeadersString}";
                await Context.Channel.SendMessageAsync(message);
            }
        }

        /*[Command("GearComment")]
        [Summary("Sets your gear comment")]
        public async Task UpdateGearCommentAsync([Remainder] [Summary("The user's gear comment")] string value)
        {
            //TODO: Only allow actual alchemy stones
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "GearComment", value);
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }*/

        /*[Command("AP")]
        [Summary("Sets your AP")]
        public async Task UpdateApAsync([Summary("The user's AP")] int value)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "AP", value.ToString());
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command("AAP")]
        [Summary("Sets your Awakening AP")]
        public async Task UpdateAapAsync([Summary("The user's Awakening AP")] int value)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "AAP", value.ToString());
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command("DP")]
        [Summary("Sets your DP")]
        public async Task UpdateDpAsync([Summary("The user's DP")] int value)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "DP", value.ToString());
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command("AlchStone")]
        [Summary("Sets your Alchemy stone")]
        public async Task UpdateAlchStoneAsync([Summary("The user's Alchemy stone")] int value)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "AlchStone", value.ToString());
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command("Axe")]
        [Summary("Sets your Alchemy stone")]
        public async Task UpdateAxeAsync([Summary("The user's axe")] string value)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "Axe", value);
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }*/
    }
}
