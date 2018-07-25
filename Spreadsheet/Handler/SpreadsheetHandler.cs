using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Handler
{
    class SpreadsheetHandler
    {
        internal static IList<IList<object>> getValuesFromRange(SheetsService service, string spreadsheetId, string range)
        {
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            var values = response.Values;
            return values;
        }
        
        internal static void ClearRange(SheetsService service, string spreadsheetId, string range)
        {
            ClearValuesRequest requestBody = new ClearValuesRequest();
            var clearRequest = service.Spreadsheets.Values.Clear(
                            body: requestBody,
                            spreadsheetId: spreadsheetId,
                            range: range
                            );
            clearRequest.Execute();
        }


        internal static void UpdateCell(SheetsService service, string spreadsheetId, string value, string cellReference)
        {
            var updateValue = new List<object>() { value };
            ValueRange requestBody = new ValueRange { Values = new List<IList<object>> { updateValue } };

            var updateRequest = service.Spreadsheets.Values.Update(
                            body: requestBody,
                            spreadsheetId: spreadsheetId,
                            range: cellReference
                            );
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var result = updateRequest.Execute();
        }

    }
}
