using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Spreadsheet.Logic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Discord.Commands
{

    public class SpreadsheetModuleBase : ModuleBase<SocketCommandContext>
    {
        protected static SpreadsheetInstance SpreadsheetInstance => SpreadsheetInstance.Instance;
    }
}
