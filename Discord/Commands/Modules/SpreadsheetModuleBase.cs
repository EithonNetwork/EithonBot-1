using Discord.Addons.Interactive;
using EithonBot.Discord.Helpers;
using EithonBot.Spreadsheet.Logic;

namespace EithonBot.Discord.Commands
{

    public class SpreadsheetModuleBase : InteractiveBase
    {
        protected static SpreadsheetInstance SpreadsheetInstance => SpreadsheetInstance.Instance;

        public SpreadsheetModuleBase() { }
    }
}
