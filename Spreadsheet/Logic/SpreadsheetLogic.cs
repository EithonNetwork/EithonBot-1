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
using EithonBot.Spreadsheet.Handler;

namespace EithonBot
{
    public class SpreadsheetLogic
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Discord integration";
        private string _spreadsheetId;
        private string _sheetName;
        private int _columnHeadersRow;
        private IList<IList<object>> _columnHeaders;
        private string _familyNamesColumn;
        //TODO: Update this after added/removed member so system works if you were added after the bot was started
        private IList<IList<object>> _familyNames;
        private SheetsService _service;

        public SpreadsheetLogic(string spreadsheetId, string sheetName, string familyNamesColumn, int columnHeadersRow)
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

            _columnHeadersRow = columnHeadersRow;
            _columnHeaders = GetRowValues(columnHeadersRow);
            _familyNamesColumn = familyNamesColumn;
            _familyNames = GetColumnValues(familyNamesColumn);
        }

        //TODO: Make a proper system for this
        internal bool MemberExists(string familyName)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return false;
            return true;
        }

        internal void AddMember(string familyName, string characterName)
        {
            IList<Object> obj = new List<Object>();
            obj.Add(familyName);
            obj.Add(characterName);
            IList<IList<Object>> values = new List<IList<Object>>();
            values.Add(obj);

            SpreadsheetsResource.ValuesResource.AppendRequest request =
                    _service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, _spreadsheetId, "Guild database!A4:Y4");
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            var response = request.Execute();
            _familyNames = GetColumnValues(_familyNamesColumn);
        }

        internal void RemoveMember(string familyName)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return;

            SpreadsheetHandler.DeleteRow(_service, _spreadsheetId, row);
            _familyNames = GetColumnValues(_familyNamesColumn);
        }

        internal string ResetSignups()
        {
            string[] headersOfColumnsToReset = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Signup Comments" };
            clearColumnsBasedOnHeaders(_columnHeaders, headersOfColumnsToReset);

            return "Reset nodewar signups. Use !nodewarsignup in the announcement channel to create a new signup interface for next week.";
        }

        internal string ResetActivity()
        {
            string[] headersOfColumnsToReset = { "GQ", "NW", "VILLA", "MILITIA", "SEAMONSTERS"};
            clearColumnsBasedOnHeaders(_columnHeaders, headersOfColumnsToReset);

            return "Reset activity signups.";
        }

        private void clearColumnsBasedOnHeaders(IList<IList<object>> rowValues, string[] headerOfColumnsToReset)
        {
            foreach (var i in headerOfColumnsToReset)
            {
                var column = GetColumnOfValue(rowValues, i);
                if (column != null)
                {
                    ClearRange("Guild database!" + column + "4:" + column);
                }
            }
        }

        //TODO: How do I make this proper async
        internal async void Signup(string familyName, string value, string signupMessage)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return;

            string columnHeader = null;
            var weekDay = new Weekday(DayEnum.Sunday);
            do
            {
                if (signupMessage.IndexOf(weekDay.ToString()) > -1) { columnHeader = weekDay.ToString(); }
                weekDay.NextDay();
            } while (weekDay.Day != DayEnum.Sunday);
            if (columnHeader == null) return;

            var column = GetColumnOfValue(_columnHeaders, columnHeader);
            if (column == null) return;
            
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId ,value, "Guild database!" + column + row);
        }

        internal string updateStat(string familyName, string stat, string value)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            string columnHeader = stat;

            var statColumn = GetColumnOfValue(_columnHeaders, columnHeader);
            if (statColumn == null) return "Could not find the stat \"" + stat + "\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, value, "Guild database!" + statColumn + row);

            var dateTime = DateTime.Now;
            var lastUpdatedColumn = GetColumnOfValue(_columnHeaders, "Gear last updated");
            if (statColumn == null) return "Could not find the column \"Gear last updated\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, dateTime.ToString(), "Guild database!" + lastUpdatedColumn + row);

            return "Updated " + stat + ".";
        }

        internal string GetGear(string familyName)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            var gearStartColumn = GetColumnOfValue(_columnHeaders, "LVL");
            if (gearStartColumn == null) return "Error: Could not find the column \"LVL\"";
            var gearEndColumn = GetColumnOfValue(_columnHeaders, "Gear last updated");
            if (gearEndColumn == null) return "Could not find the column \"Gear last updated\"";

            var gearHeaders = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + gearStartColumn + _columnHeadersRow + ":" + gearEndColumn + _columnHeadersRow);
            var gearValues = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + gearStartColumn + row + ":" + gearEndColumn + row);

            var gearMessage = MessageHelper.createGearMessage(gearHeaders, gearValues);
            return gearMessage;
        }

        internal string addActivity(string familyName, string activity)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            string columnHeader = activity;

            var statColumn = GetColumnOfValue(_columnHeaders, columnHeader);
            if (statColumn == null) return "Could not find the stat \"" + activity + "\"";
            var cellOldValue = Convert.ToInt32(SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + statColumn + row)[0][0]);
            var cellNewValue = cellOldValue + 1;
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue.ToString(), "Guild database!" + statColumn + row);

            var dateTime = DateTime.Now;
            var lastUpdatedColumn = GetColumnOfValue(_columnHeaders, "Activity last updated");
            if (statColumn == null) return "Could not find the column \"Activity last updated\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, dateTime.ToString(), "Guild database!" + lastUpdatedColumn + row);

            return "Updated " + activity + ".";
        }

        internal string GetActivity(string familyName)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            var activityStartColumn = GetColumnOfValue(_columnHeaders, "GQ");
            if (activityStartColumn == null) return "Error: Could not find the column \"GQ\"";
            var activityEndColumn = GetColumnOfValue(_columnHeaders, "Activity last updated");
            if (activityEndColumn == null) return "Could not find the column \"Activity last updated\"";

            var activityHeaders = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + activityStartColumn + _columnHeadersRow + ":" + activityEndColumn + _columnHeadersRow);
            var activityValues = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + activityStartColumn + row + ":" + activityEndColumn + row);

            var activityMessage = MessageHelper.createActivityMessage(activityHeaders, activityValues);
            return activityMessage;
        }

        private void ClearRange(string range)
        {
            SpreadsheetHandler.ClearRange(_service, _spreadsheetId, range);
        }

        private void DeleteRange(string range)
        {
            SpreadsheetHandler.ClearRange(_service, _spreadsheetId, range);
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
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild Database!" + column + ":" + column);
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return values;
        }

        private IList<IList<object>> GetRowValues(int row)
        {
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild Database!" + row + ":" + row);
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return values;
        }
    }
}
