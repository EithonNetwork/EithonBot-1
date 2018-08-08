using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    //TODO: Rename this to just "Sheets"? Currently used for both parties sheet and database sheet (maybe split?)
    class DatabaseSheet
    {
        internal string Name { get; set; }
        internal Dictionary<string, DatabaseColumn> DatabaseColumns { get; set; }
        internal Dictionary<string, DatabaseRow> DatabaseRows { get; set; }
        //TODO: maybe rename to "RowIdentifiersColumn" and "ColumnIdentifiersRow"?
        internal string FamilyNamesColumn { get; set; }
        internal int ColumnHeadersRow { get; set; }

        public DatabaseSheet(string name)
        {
            Name = name;
        }
    }
}
