using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Commands;
using EithonBot.Discord.Helpers;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using System;

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
            //TODO: Fix this shit
            var hasRole = true;//PermissionsHelper.UserHasRole((SocketGuildUser)Context.User, "Officer");

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
            //TODO: Fix this shit
            var hasRole = true; // PermissionsHelper.UserHasRole((SocketGuildUser)Context.User, "Officer");

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

        // InlineReactionReplyAsync will send a message and adds reactions on it.
        // Once an user adds a reaction, the callback is fired.
        // If callback was successfull next callback is not handled
        // Unsuccessful callback is a reaction that did not have a callback.
        [Command("reaction")]
        public async Task Test_ReactionReply()
        {
            await InlineReactionReplyAsync(new ReactionCallbackData("text", null, false, false)
                .WithCallback(new Emoji("👍"), (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} replied with 👍"))
                .WithCallback(new Emoji("👎"), (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} replied with 👎"))
            );
        }
        [Command("embedreaction")]
        public async Task Test_EmedReactionReply(bool expiresafteruse, bool singleuseperuser, bool sourceuser)
        {
            var oneEmoji = new Emoji("1⃣");
            var twoEmoji = new Emoji("2⃣");
            var threeEmoji = new Emoji("3⃣");
            var fourEmoji = new Emoji("4⃣");
            var fiveEmoji = new Emoji("5⃣");

            var embed = new EmbedBuilder()
                .WithTitle("Below are the members of Left 1. React with their number to give activity")
                .WithDescription($"" +
                $"{oneEmoji.Name} - Ziryen \n" +
                $"{twoEmoji.Name} - Emiyah \n" +
                $"{threeEmoji.Name} - Marketeer \n" +
                $"{fourEmoji.Name} - Aftertwelve \n" +
                $"{fiveEmoji.Name} - Holmquist \n")
                .WithFooter("You have 30 seconds before this message times out.")
                .Build();

            await InlineReactionReplyAsync(new ReactionCallbackData(null, embed, expiresafteruse, singleuseperuser, TimeSpan.FromSeconds(30), (c) => c.Channel.SendMessageAsync($"{c.User.Mention} Nodewar party activity message timed out."))
                .WithCallback(oneEmoji, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :beer:"))
                .WithCallback(twoEmoji, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :tropical_drink:"))
                .WithCallback(threeEmoji, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :tropical_drink:"))
                .WithCallback(fourEmoji, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :tropical_drink:"))
                .WithCallback(fiveEmoji, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :tropical_drink:")), sourceuser
            );
        }
    }
}
