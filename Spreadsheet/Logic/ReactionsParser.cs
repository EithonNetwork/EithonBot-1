using EithonBot.Models;
using EithonBot.Spreadsheet.Handler;
using EithonBot.Spreadsheet.Models;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Text;
using static EithonBot.Models.Weekday;

namespace EithonBot.Spreadsheet.Logic
{
    public class ReactionsParser
    {
        private SheetsService _service;
        private Models.Spreadsheet _spreadsheet;
        private SupportMethods _supportMethods;

        public ReactionsParser(SheetsService service, Models.Spreadsheet spreadsheet, SupportMethods supportMethods)
        {
            _service = service;

            _spreadsheet = spreadsheet;

            _supportMethods = supportMethods;
        }

        //TODO: How do I make this proper async
        internal async void Signup(string familyName, string value, string signupMessage)
        {
            var row = _spreadsheet.BDOMembersSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return;

            string columnHeader = null;
            var weekDay = new Weekday(DayEnum.Sunday);
            do
            {
                if (signupMessage.IndexOf(weekDay.ToString()) > -1) { columnHeader = weekDay.ToString(); }
                weekDay.NextDay();
            } while (weekDay.Day != DayEnum.Sunday);
            if (columnHeader == null) return;

            var column = _spreadsheet.BDOMembersSheet.DatabaseColumns.GetValueOrDefault(columnHeader);
            if (column == null) return;

            SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, value, $"{_spreadsheet.BDOMembersSheet.Name}!{column.ColumnLetters}{row.RowNumber}");
        }
    }
}
