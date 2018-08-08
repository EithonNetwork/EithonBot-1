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

        public Dictionary<string, DatabaseRow> GetDatabaseRows()
        {
            return DatabaseSheet.DatabaseRows;
        }

        public Dictionary<string, DatabaseColumn> GetDatabaseColumns()
        {
            return DatabaseSheet.DatabaseColumns;
        }

        public Dictionary<string, DatabaseColumn> GetPartiesColumns()
        {
            return PartiesSheet.DatabaseColumns;
        }
    }
}
