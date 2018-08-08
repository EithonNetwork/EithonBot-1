using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    public class DatabaseRow
    {
        internal int RowNumber { get; set; }
        internal DatabaseMember Member { get; set; }

        public DatabaseRow(int rowNumber, DatabaseMember member)
        {
            RowNumber = rowNumber;
            Member = member;
        }
    }
}
