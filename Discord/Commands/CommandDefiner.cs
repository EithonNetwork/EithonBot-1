using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Discord.Commands;
using System.Threading.Tasks;

namespace EithonBot
{
    public class ReplyModule : ModuleBase<SocketCommandContext>
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
            => ReplyAsync(echo);

        // ReplyAsync is a method on ModuleBase
    }

    public class ReplyModule2 : ModuleBase<SocketCommandContext>
    {
        // ~say hello world -> hello world
        [Command("say2")]
        [Summary("Echoes a message.")]
        public Task SayTest([Remainder] [Summary("The text to echo")] string echo)
        {
            var test = Context.User.Id;
            return ReplyAsync(test.ToString());
        }

        // ReplyAsync is a method on ModuleBase
    }
}
