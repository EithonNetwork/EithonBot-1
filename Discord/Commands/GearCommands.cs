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
            var memberGear = _spreadsheetLogic.GetGear(familyName);
            if (memberGear == null) await Context.Channel.SendMessageAsync($"Could not find any gear for {Context.User.Mention}. You can add it with the ``!gear <stat> <value>`` command");
            else await Context.Channel.SendMessageAsync("",false, EmbedHelper.UserGearEmbed(Context.User, memberGear));
        }

        [Command]
        [Summary("Retrieves other user's gear")]
        public async Task GetGearOfUser([Summary("The user")]IUser user)
        {
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var memberGear = _spreadsheetLogic.GetGear(familyName);
            if (memberGear == null) await Context.Channel.SendMessageAsync($"Could not find any gear for {user.Mention}. They can add it with the ``!gear <stat> <value>`` command");
            else await Context.Channel.SendMessageAsync("", false, EmbedHelper.UserGearEmbed(user, memberGear));
        }

        //TODO: Create a !gear help command (using the summaries already added?)

        [Command]
        [Summary("Sets the selected stat to the specified value")]
        public async Task UpdateStatAsync([Summary("The stat to be changed")] string stat, [Remainder][Summary("The user's level")] string value)
        {
            string message;
            var gearHeaders = DatabaseHelper.GetGearHeaders(true);
            if (gearHeaders.Contains(stat))
            {
                var familyName = MiscHelper.GetFamilyName(Context, Context.User);
                var response = _spreadsheetLogic.updateGearStat(familyName, stat, value);
                var memberGear = _spreadsheetLogic.GetGear(familyName);
                message = response + " Your gear profile is now as follows:";
                await Context.Channel.SendMessageAsync(message, false, EmbedHelper.UserGearEmbed(Context.User, memberGear));
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
