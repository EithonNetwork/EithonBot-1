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
    [Group("gear")]
    public class GearModule : SpreadsheetModuleBase
    {

        [Command]
        [Summary("Retrieves your own gear")]
        public async Task GetOwnGear()
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var gearMessage = _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command]
        [Summary("Retrieves other user's gear")]
        public async Task GetGearOfUser([Summary("The user")]IUser user)
        {
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var gearMessage = _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command("LVL")]
        [Summary("Sets your level")]
        public async Task UpdateLvlAsync([Summary("The user's level")] int value)
        {
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "LVL", value.ToString());
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }

        [Command("AP")]
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
        }

        [Command("GearComment")]
        [Summary("Sets your gear comment")]
        public async Task UpdateGearCommentAsync([Remainder] [Summary("The user's gear comment")] string value)
        {
            //TODO: Only allow actual alchemy stones
            var familyName = MiscHelper.GetFamilyName(Context, Context.User);
            var response = _spreadsheetLogic.updateStat(familyName, "GearComment", value);
            var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
            await Context.Channel.SendMessageAsync(gearMessage);
        }
    }
}
