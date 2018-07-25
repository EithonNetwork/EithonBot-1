using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace EithonBot
{
    class Program
    {
        //TODO: Remove all _spreadsheetLogic from here
        private SpreadsheetLogic _spreadsheetLogic;
        private DiscordSocketClient _client;

        private CommandService _commands;
        private CommandHandler _commandHandler;
        //private readonly IServiceProvider _services;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            _spreadsheetLogic = new SpreadsheetLogic("1pLMcQ7Uxha4g3c_poI7YTzZXla7omFwhQRUiCg8IzKI", "Nodewar Signup", "A", 3);
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                //MessageCacheSize = 50,

                // If your platform doesn't have native WebSockets,
                // add Discord.Net.Providers.WS4Net from NuGet,
                // add the `using` at the top, and uncomment this line:
                //WebSocketProvider = WS4NetProvider.Instance
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,

                CaseSensitiveCommands = false,
            });

            _client.Log += Log;
            _commands.Log += Log;

            // Setup your DI container.
            //_services = ConfigureServices();
        }

        /*private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection()
                // Repeat this for all the service classes
                // and other dependencies that your commands might need.
                .AddSingleton(new SomeServiceClass());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }*/


        public async Task MainAsync()
        {
            // Centralize the logic for commands into a separate method.
            _commandHandler = new CommandHandler(_client, _commands);
            await _commandHandler.InstallCommandsAsync();

            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings.Get("BotToken"));
            await _client.StartAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);

            _client.ReactionAdded += ReactionAdded;

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
        }
        //-------------------------------------

        //TODO: Can I move this into the command class?
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await arg1.GetOrDownloadAsync();
            var guild = (channel as SocketGuildChannel)?.Guild;
            var user = guild.GetUser(reaction.UserId);
            var familyName = user.Nickname.Substring(0, user.Nickname.IndexOf(" "));

            if (reaction.Emote.Name == "✅") { _spreadsheetLogic.Signup(familyName, "Yes", message.Content); }
            else if (reaction.Emote.Name == "❌") { _spreadsheetLogic.Signup(familyName, "No", message.Content); }
            else if (reaction.Emote.Name == "❔") { _spreadsheetLogic.Signup(familyName, "Maybe", message.Content); }
        }


        /*private async Task MessageReceived(SocketMessage message)
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

            if (message.Content == "!reset all")
            {
                var response = _spreadsheetLogic.ResetAll();
                await channel.SendMessageAsync(response);
            }

            if (message.Content == "!reset signups")
            {
                var response = _spreadsheetLogic.ResetSignups();
                await channel.SendMessageAsync(response);
            }

            if (message.Content == "!reset activity")
            {
                var response = _spreadsheetLogic.ResetActivity();
                await channel.SendMessageAsync(response);
            }

            if (message.Content.IndexOf("!gear") > -1)
            {
                var familyName = user.Nickname.Substring(0, user.Nickname.IndexOf(" "));
                var words = message.Content.Split(" ");
                var wordsList = words.ToList();

                if(words.Length < 2)
                {
                    var gearMessage = _spreadsheetLogic.GetGear(familyName);
                    await channel.SendMessageAsync(gearMessage);
                    return;
                }
                else {
                    //Todo: Make this less ugly
                    wordsList.RemoveAt(0);
                    var stat = wordsList[0];
                    wordsList.RemoveAt(0);
                    var param = String.Join(" ", wordsList);
                    var response = _spreadsheetLogic.updateStat(familyName, stat, param);
                    var gearMessage = response + " Your gear info is now as follows:\n" + _spreadsheetLogic.GetGear(familyName);
                    await channel.SendMessageAsync(gearMessage);
                }
            }
        }*/

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}