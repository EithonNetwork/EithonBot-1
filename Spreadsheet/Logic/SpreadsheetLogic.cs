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
//TODO: Where do I rename this to .Helpers
using EithonBot.Spreadsheet.NewFolder;

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

        internal string updateGearStat(string familyName, string stat, string value)
        {
            string columnHeader = stat;

            var message = updateField(familyName, stat, value);

            var dateTime = DateTime.Now;
            updateField(familyName, "Gear last updated", dateTime.ToString());

            return message;
        }

        internal string updateField(string familyName, string header, string value)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            var headerColumn = GetColumnOfValue(_columnHeaders, header);
            if (headerColumn == null) return "Could not find the header \"" + header + "\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, value, "Guild database!" + headerColumn + row);

            return "Updated " + header + ".";
        }

        internal MemberGear GetGear(string familyName)
        {
            var gearValues = new Dictionary<string, string>();

            var row = GetRowOfValue(familyName, _familyNames);
            //if (row == 0) return "Could not find family name";

            var columnHeaders = DatabaseHelper.GetGearHeaders();

            var startColumn = GetColumnOfValue(_columnHeaders, columnHeaders.FirstOrDefault());
            var endColumn = GetColumnOfValue(_columnHeaders, columnHeaders.LastOrDefault());
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + startColumn + row + ":" + endColumn + row);
            if (values == null) return null;

            for (var i = 0; i < columnHeaders.Count();i++)
            {
                var value = values[0][i];
                if ((string)value == "") value = "N/A";
                gearValues.TryAdd(columnHeaders[i], value.ToString());
            }

            //TODO: Better way of doing this (So I don't have to update gear headers in multiple places
            var memberGear = new MemberGear();
            memberGear.LVL = gearValues.GetValueOrDefault("LVL");
            memberGear.Renown = gearValues.GetValueOrDefault("Renown");

            memberGear.AP = gearValues.GetValueOrDefault("AP");
            memberGear.AAP = gearValues.GetValueOrDefault("AAP");
            memberGear.DP = gearValues.GetValueOrDefault("DP");

            //memberGear.Class = gearValues.GetValueOrDefault("Class");
            memberGear.AlchStone = gearValues.GetValueOrDefault("AlchStone");
            memberGear.Axe = gearValues.GetValueOrDefault("Axe");

            memberGear.GearComment = gearValues.GetValueOrDefault("GearComment");
            memberGear.GearLink = gearValues.GetValueOrDefault("GearLink");
            memberGear.GearLastUpdated = gearValues.GetValueOrDefault("Gear last updated");
            return memberGear;
        }

        internal string AddActivity(string familyName, string activityType)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            string columnHeader = activityType;

            var statColumn = GetColumnOfValue(_columnHeaders, columnHeader);
            if (statColumn == null) return "Could not find the stat \"" + activityType + "\"";
            var cellOldValue = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + statColumn + row);
            int newValue;
            if (cellOldValue == null) newValue = 1;
            else newValue = Convert.ToInt32(cellOldValue[0][0]) + 1;
            var cellNewValue = newValue.ToString();

            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue, "Guild database!" + statColumn + row);

            var dateTime = DateTime.Now;
            var lastUpdatedColumn = GetColumnOfValue(_columnHeaders, "Activity last updated");
            if (statColumn == null) return "Could not find the column \"Activity last updated\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, dateTime.ToString(), "Guild database!" + lastUpdatedColumn + row);

            return "Updated " + activityType + ".";
        }

        internal string RemoveActivity(string familyName, string activityType)
        {
            var row = GetRowOfValue(familyName, _familyNames);
            if (row == 0) return "Could not find family name";

            string columnHeader = activityType;

            var statColumn = GetColumnOfValue(_columnHeaders, columnHeader);
            if (statColumn == null) return "Could not find the stat \"" + activityType + "\"";

            var cellOldValue = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + statColumn + row);
            int newValue;
            if (cellOldValue == null) return null;
            else newValue = Convert.ToInt32(cellOldValue[0][0]) - 1;

            //If 0 put blank
            string cellNewValue;
            if (newValue == 0) cellNewValue = "";
            else cellNewValue = newValue.ToString();

            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue, "Guild database!" + statColumn + row);

            var dateTime = DateTime.Now;
            var lastUpdatedColumn = GetColumnOfValue(_columnHeaders, "Activity last updated");
            if (statColumn == null) return "Could not find the column \"Activity last updated\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, dateTime.ToString(), "Guild database!" + lastUpdatedColumn + row);

            return "Updated " + activityType + ".";
        }

        internal MemberActivity GetActivity(string familyName)
        {
            var activityValues = new Dictionary<string, string>();

            var row = GetRowOfValue(familyName, _familyNames);
            //if (row == 0) return "Could not find family name";

            var columnHeaders = DatabaseHelper.GetActivityHeaders();

            var startColumn = GetColumnOfValue(_columnHeaders, columnHeaders.FirstOrDefault());
            var endColumn = GetColumnOfValue(_columnHeaders, columnHeaders.LastOrDefault());
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, "Guild database!" + startColumn + row + ":" + endColumn + row);
            if (values == null) return null;

            for (var i = 0; i < columnHeaders.Count(); i++)
            {
                var value = values[0][i];
                if ((string)value == "") value = "N/A";
                activityValues.TryAdd(columnHeaders[i], value.ToString());
            }

            //TODO: Better way of doing this (So I don't have to update gear headers in multiple places
            var memberActivity = new MemberActivity();
            memberActivity.Tier = activityValues.GetValueOrDefault("Tier");
            memberActivity.GA = activityValues.GetValueOrDefault("GA");

            memberActivity.GQ = activityValues.GetValueOrDefault("GQ");
            memberActivity.NW = activityValues.GetValueOrDefault("NW");
            memberActivity.Villa = activityValues.GetValueOrDefault("Villa");
            memberActivity.Militia = activityValues.GetValueOrDefault("Militia");
            memberActivity.Seamonsters = activityValues.GetValueOrDefault("Seamonsters");

            memberActivity.InactivityNotice = activityValues.GetValueOrDefault("InactivityNotice");
            memberActivity.ActivityLastUpdated = activityValues.GetValueOrDefault("Activity last updated");
            return memberActivity;
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
