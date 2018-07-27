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
            //TODO: Add check so you can't add duplicates
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var characterName = MiscHelper.GetCharacterName(Context, user);
            var hasRole = PermissionsHelper.UserHasRole((SocketGuildUser)Context.User, "Officer");

            if (!hasRole)
            {
                await Context.Channel.SendMessageAsync("Officer role required to execute this command");
                return;
            }
            if (_spreadsheetLogic.MemberExists(familyName))
            {
                await Context.Channel.SendMessageAsync("Member is already added");
                return;
            }

            _spreadsheetLogic.AddMember(familyName, characterName);
            await Context.Channel.SendMessageAsync("Added member to spreadsheet");
        }

        [Command("remove")]
        [Summary("Remove a member from the google sheet database")]
        public async Task RemoveMember([Summary("The user")]IUser user)
        {
            //TODO: Add check so you can't add duplicates
            var familyName = MiscHelper.GetFamilyName(Context, user);
            var characterName = MiscHelper.GetCharacterName(Context, user);
            var hasRole = PermissionsHelper.UserHasRole((SocketGuildUser)Context.User, "Officer");

            if (!hasRole)
            {
                await Context.Channel.SendMessageAsync("Officer role required to execute this command");
                return;
            }
            if (!_spreadsheetLogic.MemberExists(familyName))
            {
                await Context.Channel.SendMessageAsync("Member does not exist on the spreadsheet");
                return;
            }

            _spreadsheetLogic.RemoveMember(familyName);
            await Context.Channel.SendMessageAsync("Removed member from spreadsheet");
        }
    }

    [Group("reset")]
    public class ResetModule : SpreadsheetModuleBase
    {
        [Command("signups")]
        [Summary("Resets signups")]
        public async Task ResetSignups()
        {
            _spreadsheetLogic.ResetSignups();
            await Context.Channel.SendMessageAsync("Reset Signups");
        }

        [Command("activity")]
        [Summary("Resets activity")]
        public async Task ResetActivity()
        {
            _spreadsheetLogic.ResetActivity();
            await Context.Channel.SendMessageAsync("Reset Activity");
        }
    }

    public class ReplyModule : ModuleBase<SocketCommandContext>
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
            => ReplyAsync(echo, false, EmbedHelper.GearEmbed());
        //await Context.Channel.SendMessageAsync("", false, builder);

        // ReplyAsync is a method on ModuleBase
    }

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

            var eventMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Message, "Event. Sunday event", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            var mondayMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Message, "1. Monday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            var wednesdayMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Message, "2. Wednesday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            var fridayMessage = await MessageHelper.SendMessageWithReactionsAsync(Context.Message, "3. Friday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
        }
    }
}
