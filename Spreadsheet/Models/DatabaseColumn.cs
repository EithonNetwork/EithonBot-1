using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    class DatabaseColumn
    {
        public string ColumnLetters { get; private set; }
        public string ColumnHeader { get; private set; }

        public DatabaseColumn(string columnLetters, string columnHeader)
        {
            ColumnLetters = columnLetters;
            ColumnHeader = columnHeader;
        }
    }
}
