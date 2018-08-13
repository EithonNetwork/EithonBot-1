using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    public class Spreadsheet
    {
        internal string Name { get; set; }
        internal string Id { get; private set; }
        internal DatabaseSheet DatabaseSheet { get; set; }
        internal DatabaseSheet PartiesSheet { get; set; }

        public Spreadsheet(string spreadsheetId)
        {
            Id = spreadsheetId;
        }
    }
}
