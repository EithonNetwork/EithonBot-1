using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    public class DatabaseColumn
    {
        //TODO: Currently this is used for the parties sheet as well, is that good?
        public string ColumnLetters { get; private set; }
        public string ColumnHeader { get; private set; }

        public DatabaseColumn(string columnLetters, string columnHeader)
        {
            ColumnLetters = columnLetters;
            ColumnHeader = columnHeader;
        }
    }
}
