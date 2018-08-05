using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    class DatabaseRow
    {
        internal int RowNumber { get; set; }
        internal string FamilyName { get; set; }

        public DatabaseRow(int rowNumber, string familyName)
        {
            RowNumber = rowNumber;
            FamilyName = familyName;
        }
    }
}
