using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Discord.Commands
{

    public class SpreadsheetModuleBase : ModuleBase<SocketCommandContext>
    {
        protected static SpreadsheetLogic SpreadsheetLogic => SpreadsheetLogic.Instance;
    }
}
