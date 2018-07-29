using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Discord.Commands
{

    public class SpreadsheetModuleBase : ModuleBase<SocketCommandContext>
    {
        public SpreadsheetLogic _spreadsheetLogic;

        public SpreadsheetModuleBase()
        {
            _spreadsheetLogic = new SpreadsheetLogic("1pLMcQ7Uxha4g3c_poI7YTzZXla7omFwhQRUiCg8IzKI", "Nodewar Signup", "A", 3);
        }
    }
}
