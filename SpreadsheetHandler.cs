using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using Data = Google.Apis.Sheets.v4.Data;
using System.Linq;
using static EithonBot.Models.Weekday;
using EithonBot.Models;

namespace EithonBot
{
    class SpreadsheetHandler
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Discord integration";
        private string _spreadsheetId;
        private string _sheetName;
        private SheetsService _service;

        public SpreadsheetHandler(string spreadsheetId, string sheetName)
        {
            _spreadsheetId = spreadsheetId;
            _sheetName = sheetName;

            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        internal async void UpdateSpreadSheet(ISocketMessageChannel channel, Optional<SocketUserMessage> message)
        {
            String range = "Nodewar signup!D3:H6";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    _service.Spreadsheets.Values.Get(_spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                Console.WriteLine("Status, Sunday, Monday, Wednesday, Friday");
                foreach (var row in values)
                {
                    // Print columns A and E, which correspond to indices 0 and 4.
                    Console.WriteLine("{0}, {1}, {2}, {3}, {4}", row[0], row[1], row[2], row[3], row[4]);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        //TODO: How do I make this async
        internal string ResetSignups()
        {
            var rowValues = GetRowValues(3);
            if (rowValues == null) return "No columnheaders were found";

            var weekDay = new Weekday(DayEnum.Sunday);
            do
            {
                var column = GetColumnOfValue(rowValues, weekDay.ToString());
                if (column != null)
                {
                    ClearRange("Guild database!" + column + "4:" + column);
                }
                weekDay.NextDay();
            } while (weekDay.Day != DayEnum.Sunday);

            var commentColumn = GetColumnOfValue(rowValues, "Comment");
            if (commentColumn == null) return "Successfully reset the days but could not reset the comment column";
            ClearRange("Guild database!" + commentColumn + "4:" + commentColumn);
            return "Succesfully reset nodewar signups. Use !nodewarsignup in the announcement channel to create a new signup interface for next week.";
        }

        //TODO: How do I make this proper async
        internal async void Signup(string familyName, string value, string signupMessage)
        {
            var familyNameValues = GetColumnValues("A");
            if (familyNameValues == null) return;

            var columnHeaders = GetRowValues(3);
            if (columnHeaders == null) return;

            var updateValue = new List<object>() { value };
            ValueRange requestBody = new ValueRange { Values = new List<IList<object>> { updateValue } };

            var row = GetRowOfValue(familyName, familyNameValues);
            if (row == 0) return;

            string columnHeader = null;
            var weekDay = new Weekday(DayEnum.Sunday);
            do
            {
                if (signupMessage.IndexOf(weekDay.ToString()) > -1) { columnHeader = weekDay.ToString(); }
                weekDay.NextDay();
            } while (weekDay.Day != DayEnum.Sunday);
            if (columnHeader == null) return;

            var column = GetColumnOfValue(columnHeaders, columnHeader);
            if (column == null) return;
            UpdateRange(requestBody, "Guild database!" + column + row);
        }

        internal string updateStat(string familyName, string stat, string value)
        {
            var familyNameValues = GetColumnValues("A");
            if (familyNameValues == null) return "";

            var columnHeaders = GetRowValues(3);
            if (columnHeaders == null) return "";

            var updateValue = new List<object>() { value };
            ValueRange requestBody = new ValueRange { Values = new List<IList<object>> { updateValue } };

            var row = GetRowOfValue(familyName, familyNameValues);
            if (row == 0) return "";

            string columnHeader = stat;

            var column = GetColumnOfValue(columnHeaders, columnHeader);
            if (column == null) return "";
            UpdateRange(requestBody, "Guild database!" + column + row);
            return "";
        }

        private void UpdateRange(ValueRange requestBody, string range)
        {
            var updateRequest = _service.Spreadsheets.Values.Update(
                            body: requestBody,
                            spreadsheetId: _spreadsheetId,
                            range: range
                            );
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            var result = updateRequest.Execute();
        }

        private void ClearRange(string range)
        {
            Data.ClearValuesRequest requestBody = new Data.ClearValuesRequest();
            var clearRequest = _service.Spreadsheets.Values.Clear(
                            body: requestBody,
                            spreadsheetId: _spreadsheetId,
                            range: range
                            );
            var result = clearRequest.Execute();
        }

        private static int GetRowOfValue(string value, IList<IList<object>> columnValues)
        {
            int rownumber = 0;
            for (int i = 0; i < columnValues.Count; i++)
            {
                var row = columnValues[i];
                if (row.Count != 0)
                {
                    if (row[0].ToString() == value)
                    {
                        rownumber = i + 1;
                        break;
                    }
                }
            }
            return rownumber;
        }

        private static string GetColumnOfValue(IList<IList<object>> rowValues, string value)
        {
            var columnNumber = 0;
            for (int i = 0; i < rowValues[0].Count; i++)
            {
                var column = rowValues[0][i];
                if (column != null)
                {
                    if (column.ToString() == value)
                    {
                        columnNumber = i + 1;
                        break;
                    }
                }
            }
            if (columnNumber == 0) return null;
            string[] columnLetters = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH" };
            var columnLetter = columnLetters[columnNumber - 1];

            return columnLetter;
        }

        private IList<IList<object>> GetColumnValues(string column)
        {
            var request = _service.Spreadsheets.Values.Get(_spreadsheetId, "Guild Database!" + column + ":" + column);
            var response = request.Execute();
            var columnValues = response.Values;
            if (columnValues.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return columnValues;
        }

        private IList<IList<object>> GetRowValues(int row)
        {
            var request = _service.Spreadsheets.Values.Get(_spreadsheetId, "Guild Database!" + row + ":" + row);
            var response = request.Execute();
            var rowValues = response.Values;
            if (rowValues.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return rowValues;
        }
    }
}
