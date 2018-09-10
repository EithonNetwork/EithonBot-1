using EithonBot.Spreadsheet.Models;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EithonBot.Spreadsheet.Handler
{
    class SpreadsheetHandler
    {
        internal static void NewMember(SheetsService service, string spreadsheetId, DatabaseSheet sheet, IList<IList<object>> values)
        {
            SpreadsheetsResource.ValuesResource.AppendRequest request =
                                service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, spreadsheetId, $"{sheet.Name}!{sheet.DatabaseColumns.FirstOrDefault().Value.ColumnLetters}{sheet.ColumnIdentifiersRow+2}:{sheet.DatabaseColumns.LastOrDefault().Value.ColumnLetters}{sheet.ColumnIdentifiersRow + 2}");
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            var response = request.Execute();
        }

        internal static IList<IList<object>> GetValuesFromRange(SheetsService service, string spreadsheetId, string range)
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

        internal static void DeleteRow(SheetsService service, string spreadsheetId, int row)
        {
            Request RequestBody = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = 593128798,
                        Dimension = "ROWS",
                        StartIndex = row-1,
                        EndIndex = row
                    }
                }
            };

            List<Request> RequestContainer = new List<Request>();
            RequestContainer.Add(RequestBody);

            BatchUpdateSpreadsheetRequest DeleteRequest = new BatchUpdateSpreadsheetRequest();
            DeleteRequest.Requests = RequestContainer;

            SpreadsheetsResource.BatchUpdateRequest Deletion = new SpreadsheetsResource.BatchUpdateRequest(service, DeleteRequest, spreadsheetId);
            Deletion.Execute();
        }
        
        //TODO: change signature? (switch place on value and cellreference)
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
