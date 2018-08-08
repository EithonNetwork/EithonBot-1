using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Commands;
using EithonBot.Discord.Helpers;
using System.Threading.Tasks;

namespace EithonBot
{
    [Group("member")]
    public class MemberModule : SpreadsheetModuleBase
    {
        [Command("add")]
        [Summary("Add a member to the google sheet database")]
        public async Task AddMember([Summary("The user")]IUser user)
        {
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var characterName = MiscHelper.GetCharacterName(Context, user);
            var hasRole = PermissionsHelper.UserHasRole((SocketGuildUser)Context.User, "Officer");

            if (!hasRole)
            {
                await Context.Channel.SendMessageAsync("Officer role required to execute this command");
                return;
            }
            else
            {
                var member = SpreadsheetInstance.CommandsParser.AddMember(familyName, characterName);
                if (member == null)
                {
                    await Context.Channel.SendMessageAsync("Could not add member (Already exists?). Get Zil to add proper error messages please");
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Added member to spreadsheet");
                }
            }
        }

        [Command("remove")]
        [Summary("Remove a member from the google sheet database")]
        public async Task RemoveMember([Summary("The user")]IUser user)
        {
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var characterName = MiscHelper.GetCharacterName(Context, user);
            var hasRole = PermissionsHelper.UserHasRole((SocketGuildUser)Context.User, "Officer");

            if (!hasRole)
            {
                await Context.Channel.SendMessageAsync("Officer role required to execute this command");
                return;
            }
            else
            {
                var succeded = SpreadsheetInstance.CommandsParser.RemoveMember(familyName);
                if (!succeded)
                {
                    await Context.Channel.SendMessageAsync("Could not remove member (Not added?). Get Zil to add proper error messages please");
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Removed member from spreadsheet");
                }
            }
        }
    }

    [Group("reset")]
    public class ResetModule : SpreadsheetModuleBase
    {
        [Command("signups")]
        [Summary("Resets signups")]
        public async Task ResetSignups()
        {
            SpreadsheetInstance.CommandsParser.ResetSignups();
            await Context.Channel.SendMessageAsync("Reset Signups");
        }

        [Command("activity")]
        [Summary("Resets activity")]
        public async Task ResetActivity()
        {
            SpreadsheetInstance.CommandsParser.ResetActivity();
            await Context.Channel.SendMessageAsync("Reset Activity");
        }
    }

    //public class ReplyModule : ModuleBase<SocketCommandContext>
    //{
    //    // ~say hello world -> hello world
    //    [Command("say")]
    //    [Summary("Echoes a message.")]
    //    public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
    //        => ReplyAsync(echo);

    //    // ~say hello world -> hello world
    //    [Command("invitelink")]
    //    [Summary("Provides the invite link.")]
    //    public Task GetLink()
    //        => ReplyAsync("https://discordapp.com/oauth2/authorize?client_id=460865617424154624&scope=bot");
    //}

    public class SignupModule : SpreadsheetModuleBase
    {
        [Command("nodewarsignup")]
        [Summary("Creates a signup message which users can react to")]
        public async Task NodewarSignupsMessage()
        {
            var greenCheckEmoji = new Emoji("✅");
            var xEmoji = new Emoji("❌");
            var greyQuestionEmoji = new Emoji("❔");

            await Context.Message.DeleteAsync();

            var infoMessage = await Context.Channel.SendMessageAsync("**Please react to the following messages to indicate your participation in the coming guild activities:**");

            var eventMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Channel, "Event. Sunday event", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            var mondayMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Channel, "1. Monday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            var wednesdayMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Channel, "2. Wednesday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            var fridayMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Channel, "3. Friday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
        }
    }
}
