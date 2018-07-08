using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EithonBot
{

    public static class MessageHelper
    {
        public static async Task<IUserMessage> SendMessageWithReactionsAsync(SocketMessage context, string content, bool isTTS = false, Embed embed = null, params IEmote[] emotes)
        {
            var msg = await context.Channel.SendMessageAsync(content, isTTS, embed);
            foreach (var emote in emotes)
            {
                await msg.AddReactionAsync(emote);
            }

            return msg;
        }

        public static async Task AddReactionsAsync(this IUserMessage msg, params IEmote[] emotes)
        {
            foreach (var emote in emotes)
            {
                await msg.AddReactionAsync(emote);
            }
        }
    }
}