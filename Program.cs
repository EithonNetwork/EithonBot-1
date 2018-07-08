using Discord;
using Discord.WebSocket;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace EithonBot
{
    class Program
    {
        private SpreadsheetHandler _spreadsheetHandler;
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _spreadsheetHandler = new SpreadsheetHandler("1pLMcQ7Uxha4g3c_poI7YTzZXla7omFwhQRUiCg8IzKI", "Nodewar Signup");
            _client = new DiscordSocketClient();
            _client.Log += Log;

            string token = ConfigurationManager.AppSettings.Get("BotToken"); // Remember to keep this private!
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;
            _client.ReactionAdded += ReactionAdded;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await arg1.GetOrDownloadAsync();
            var guild = (channel as SocketGuildChannel)?.Guild;
            var user = guild.GetUser(reaction.UserId);
            var familyName = user.Nickname.Substring(0, user.Nickname.IndexOf(" "));

            if (reaction.Emote.Name == "✅") { _spreadsheetHandler.Signup(familyName, "Yes", message.Content); }
            else if (reaction.Emote.Name == "❌") { _spreadsheetHandler.Signup(familyName, "No", message.Content); }
            else if (reaction.Emote.Name == "❔") { _spreadsheetHandler.Signup(familyName, "Maybe", message.Content); }
        }


        private async Task MessageReceived(SocketMessage message)
        {
            var channel = message.Channel;
            var guild = (channel as SocketGuildChannel)?.Guild;
            var user = guild.GetUser(message.Author.Id);

            if (message.Content == "!nodewarsignup")
            {
                var greenCheckEmoji = new Emoji("✅");
                var xEmoji = new Emoji("❌");
                var greyQuestionEmoji = new Emoji("❔");

                await message.DeleteAsync();

                var infoMessage = await channel.SendMessageAsync("**Please react to the following messages to indicate your participation in the coming guild activities:**");

                var eventMessage = await MessageHelper.SendMessageWithReactionsAsync(message, "Event. Sunday event", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
                var mondayMessage = await MessageHelper.SendMessageWithReactionsAsync(message, "1. Monday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
                var wednesdayMessage = await MessageHelper.SendMessageWithReactionsAsync(message, "2. Wednesday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
                var fridayMessage = await MessageHelper.SendMessageWithReactionsAsync(message, "3. Friday nodewar", false, null, greenCheckEmoji, xEmoji, greyQuestionEmoji);
            }

            if (message.Content == "!nw signup reset")
            {
                var response = _spreadsheetHandler.ResetSignups();
                await channel.SendMessageAsync(response);
            }

            if (message.Content.IndexOf("!gear") > -1)
            {
                var familyName = user.Nickname.Substring(0, user.Nickname.IndexOf(" "));
                var words = message.Content.Split(" ");
                var response = _spreadsheetHandler.updateStat(familyName, words[1], words[2]);
                await channel.SendMessageAsync(response);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}