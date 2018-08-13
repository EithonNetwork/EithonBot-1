using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    class DatabaseField
    {
        public DatabaseSheet Sheet { get; private set; }
        public DatabaseColumn Column { get; private set; }
        public DatabaseRow Row { get; private set; }
        public string CellReference { get; private set; }
        public string CellValue { get; set; }

        public DatabaseField(DatabaseSheet sheet, DatabaseColumn column, DatabaseRow row)
        {
            Sheet = sheet;
            Column = column;
            Row = row;
            CellReference = $"{sheet.Name}!{column.ColumnLetters}{row.RowNumber}";
        }

        public DatabaseField(DatabaseSheet sheet, DatabaseColumn column, DatabaseRow row, string cellValue)
        {
            Sheet = sheet;
            Column = column;
            Row = row;
            CellReference = $"{sheet.Name}!{column.ColumnLetters}{row.RowNumber}";
            CellValue = cellValue;
        }
    }
}
