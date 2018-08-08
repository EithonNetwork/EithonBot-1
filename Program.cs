using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using EithonBot.Spreadsheet.Logic;

namespace EithonBot
{
    class Program
    {
        //TODO: Remove all _spreadsheetLogic from here
        private SpreadsheetInstance _spreadsheetInstance;
        private DiscordSocketClient _client;

        private CommandService _commands;
        private CommandHandler _commandHandler;
        //private readonly IServiceProvider _services;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            //TODO: How do I update this logic with the new family names added to spreadsheet?
            _spreadsheetInstance = SpreadsheetInstance.Instance;
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
            var reactionName = reaction.Emote.Name;

            if (reactionName == "✅") { _spreadsheetInstance.ReactionsParser.Signup(familyName, "Yes", message.Content); }
            else if (reactionName == "❌") { _spreadsheetInstance.ReactionsParser.Signup(familyName, "No", message.Content); }
            else if (reactionName == "❔") { _spreadsheetInstance.ReactionsParser.Signup(familyName, "Maybe", message.Content); }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}