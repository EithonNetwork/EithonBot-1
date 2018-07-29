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

        //TODO: Do something about these messages
        public static string createGearMessage(IList<IList<object>> gearHeaders, IList<IList<object>> gearValues)
        {
            var gearMessage = "```" + gearHeaders[0][0] + ": " + gearValues[0][0] + ", " + gearHeaders[0][1] + ": " + gearValues[0][1] + ", " + gearHeaders[0][2] + ": " + gearValues[0][2] + ", " + gearHeaders[0][3] + ": " + gearValues[0][3] + "\n" +
                 gearHeaders[0][4] + ": " + gearValues[0][4] + ", " + gearHeaders[0][5] + ": " + gearValues[0][5] + "\n" +
                 gearHeaders[0][6] + ": " + gearValues[0][6] + "\n\n" +
                 gearHeaders[0][7] + ": " + gearValues[0][7] + "```";
            return gearMessage;
        }

        public static string createActivityMessage(IList<IList<object>> activityHeaders, IList<IList<object>> activityValues)
        {
            var activityMessage = "```" + activityHeaders[0][0] + ": " + activityValues[0][0] + ", " + activityHeaders[0][1] + ": " + activityValues[0][1] + ", " + activityHeaders[0][2] + ": " + activityValues[0][2] + ", " + activityHeaders[0][3] + ": " + activityValues[0][3] + ", " + activityHeaders[0][4] + ": " + activityValues[0][4] + "\n\n" +
                 activityHeaders[0][5] + ": " + activityValues[0][5] + "```";
            return activityMessage;
        }
    }
}