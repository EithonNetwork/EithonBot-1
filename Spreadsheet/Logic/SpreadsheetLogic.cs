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
        private static SpreadsheetLogic _instance;
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Discord integration";
        private string _spreadsheetId;

        private string _databaseSheet;
        private int _databaseColumnHeadersRow;
        private IList<IList<object>> _databaseColumnHeaders;

        private string _partiesSheet;
        private int _partyColumnHeadersRow;
        private IList<IList<object>> _partyColumnHeaders;

        private string _databaseFamilyNamesColumn;
        //TODO: Update this after added/removed member so system works if you were added after the bot was started
        private IList<IList<object>> _databaseFamilyNames;
        private SheetsService _service;
        
        public static SpreadsheetLogic Instance => _instance;

        static SpreadsheetLogic()
        {
            _instance = new SpreadsheetLogic("1pLMcQ7Uxha4g3c_poI7YTzZXla7omFwhQRUiCg8IzKI", "Guild database", "A", 3, "NW Parties", 3);
        }

        private SpreadsheetLogic(string spreadsheetId, string databaseSheet, string databaseFamilyNamesColumn, int databaseHeadersRow, string partiesSheet, int partiesSheetHeadersRow)
        {
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

            _spreadsheetId = spreadsheetId;
            _databaseSheet = databaseSheet;
            _databaseColumnHeadersRow = databaseHeadersRow;
            _databaseColumnHeaders = GetRowValues(_databaseSheet, databaseHeadersRow);
            _partiesSheet = partiesSheet;
            _partyColumnHeadersRow = partiesSheetHeadersRow;
            _partyColumnHeaders = GetRowValues(_partiesSheet, _partyColumnHeadersRow);
            _databaseFamilyNamesColumn = databaseFamilyNamesColumn;
            _databaseFamilyNames = GetColumnValues(databaseFamilyNamesColumn);
        }

        //TODO: Make a proper system for this
        internal bool MemberExists(string familyName)
        {
            var row = GetRowOfValue(familyName, _databaseFamilyNames);
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
                    _service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, _spreadsheetId, $"{_databaseSheet}!A5:Y5");
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            var response = request.Execute();
            _databaseFamilyNames = GetColumnValues(_databaseFamilyNamesColumn);
        }

        internal void RemoveMember(string familyName)
        {
            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return;

            SpreadsheetHandler.DeleteRow(_service, _spreadsheetId, row);
            _databaseFamilyNames = GetColumnValues(_databaseFamilyNamesColumn);
        }

        internal string ResetSignups()
        {
            string[] headersOfColumnsToReset = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "SignupComment" };
            clearColumnsBasedOnHeaders(_databaseColumnHeaders, headersOfColumnsToReset);

            return "Reset nodewar signups. Use !nodewarsignup in the announcement channel to create a new signup interface for next week.";
        }

        internal string ResetActivity()
        {
            string[] headersOfColumnsToReset = { "GQ", "NW", "VILLA", "MILITIA", "SEAMONSTERS"};
            clearColumnsBasedOnHeaders(_databaseColumnHeaders, headersOfColumnsToReset);

            return "Reset activity signups.";
        }

        private void clearColumnsBasedOnHeaders(IList<IList<object>> rowValues, string[] headerOfColumnsToReset)
        {
            foreach (var i in headerOfColumnsToReset)
            {
                var column = GetColumnOfValue(rowValues, i);
                if (column != null)
                {
                    ClearRange($"{_databaseSheet}!{column}{_databaseColumnHeadersRow+2}:{column}");
                }
            }
        }

        //TODO: How do I make this proper async
        internal async void Signup(string familyName, string value, string signupMessage)
        {
            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return;

            string columnHeader = null;
            var weekDay = new Weekday(DayEnum.Sunday);
            do
            {
                if (signupMessage.IndexOf(weekDay.ToString()) > -1) { columnHeader = weekDay.ToString(); }
                weekDay.NextDay();
            } while (weekDay.Day != DayEnum.Sunday);
            if (columnHeader == null) return;

            var column = GetColumnOfValue(_databaseColumnHeaders, columnHeader);
            if (column == null) return;
            
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId ,value, $"{_databaseSheet}!{column}{row}");
        }

        internal MemberSignupData GetSignupData(string familyName)
        {
            var dictionary = new Dictionary<string, string>();

            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return null;

            var columnHeaders = DatabaseHelper.GetSignupHeaders();

            List<string> currentHeaders = new List<string>(); 
            foreach (var item in columnHeaders)
            {
                var result = _databaseColumnHeaders.SelectMany(i => i).ToList();

                if (result.Contains(item)) currentHeaders.Add(item);
            }

            var columnLetters = DatabaseHelper.GetColumnLetters();

            var startColumn = GetColumnOfValue(_databaseColumnHeaders, currentHeaders.FirstOrDefault());
            var endColumn = GetColumnOfValue(_databaseColumnHeaders, currentHeaders.LastOrDefault());
            var indexOfEndColumn = columnLetters.IndexOf(endColumn);

            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet}!{startColumn}{row}:{endColumn}{row}");
            if (values == null) return null;

            for (var i = 0; i < currentHeaders.Count(); i++)
            {
                string value;
                try
                {
                    value = values[0][i].ToString();
                }
                catch
                {
                    value = "N/A";
                }
                if (value == "") value = "N/A";
                dictionary.TryAdd(currentHeaders[i], value);
            }

            //TODO: Better way of doing this (So I don't have to update headers in multiple places)
            var memberSignupData = new MemberSignupData();
            memberSignupData.FirstEvent.Name = currentHeaders[0];
            memberSignupData.FirstEvent.Value = dictionary.GetValueOrDefault(currentHeaders[0]);

            memberSignupData.SecondEvent.Name = currentHeaders[1];
            memberSignupData.SecondEvent.Value = dictionary.GetValueOrDefault(currentHeaders[1]);

            memberSignupData.ThirdEvent.Name = currentHeaders[2];
            memberSignupData.ThirdEvent.Value = dictionary.GetValueOrDefault(currentHeaders[2]);

            memberSignupData.FourthEvent.Name = currentHeaders[3];
            memberSignupData.FourthEvent.Value = dictionary.GetValueOrDefault(currentHeaders[3]);

            memberSignupData.SignupComment.Name = currentHeaders.LastOrDefault();
            memberSignupData.SignupComment.Value = dictionary.GetValueOrDefault(currentHeaders.LastOrDefault());
            
            return memberSignupData;
        }

        internal string UpdateField(string familyName, string header, string value)
        {
            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return "Could not find family name";

            var column = GetColumnOfValue(_databaseColumnHeaders, header);
            if (column == null) return "Could not find the header \"" + header + "\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, value, $"{_databaseSheet}!{column}{row}");

            return "Updated " + header + ".";
        }

        internal MemberGear GetGear(string familyName)
        {
            var dictionary = new Dictionary<string, string>();

            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return null;

            var columnHeaders = DatabaseHelper.GetGearHeaders();

            var startColumn = GetColumnOfValue(_databaseColumnHeaders, columnHeaders.FirstOrDefault());
            var endColumn = GetColumnOfValue(_databaseColumnHeaders, columnHeaders.LastOrDefault());
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet}!{startColumn}{row}:{endColumn}{row}");
            if (values == null) return null;

            for (var i = 0; i < columnHeaders.Count();i++)
            {
                string value;
                try
                {
                    value = values[0][i].ToString();
                }
                catch
                {
                    value = "N/A";
                }
                if (value == "") value = "N/A";
                dictionary.TryAdd(columnHeaders[i], value);
            }

            //TODO: Better way of doing this (So I don't have to update gear headers in multiple places)
            var memberGear = new MemberGear();
            memberGear.LVL = dictionary.GetValueOrDefault("LVL");
            memberGear.Renown = dictionary.GetValueOrDefault("Renown");

            memberGear.AP = dictionary.GetValueOrDefault("AP");
            memberGear.AAP = dictionary.GetValueOrDefault("AAP");
            memberGear.DP = dictionary.GetValueOrDefault("DP");

            //memberGear.Class = gearValues.GetValueOrDefault("Class");
            memberGear.AlchStone = dictionary.GetValueOrDefault("AlchStone");
            memberGear.Axe = dictionary.GetValueOrDefault("Axe");

            memberGear.GearComment = dictionary.GetValueOrDefault("GearComment");
            memberGear.GearLink = dictionary.GetValueOrDefault("GearLink");
            memberGear.GearLastUpdated = dictionary.GetValueOrDefault("Gear last updated");
            return memberGear;
        }

        internal List<object> GetPartyMembers(string partyName)
        {
            var column = GetColumnOfValue(_partyColumnHeaders, partyName);
            if (column == null) return null;

            var partyMembers = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_partiesSheet}!{column}{_partyColumnHeadersRow + 1}:{column}");

            var result = partyMembers.SelectMany(i => i).ToList();

            return result;
        }

        internal string AddActivity(string familyName, string activityType)
        {
            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return "Could not find family name";

            string columnHeader = activityType;

            var activityColumn = GetColumnOfValue(_databaseColumnHeaders, columnHeader);
            if (activityColumn == null) return "Could not find the stat \"" + activityType + "\"";
            var cellOldValue = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet}!{activityColumn}{row}");
            int newValue;
            if (cellOldValue == null) newValue = 1;
            else newValue = Convert.ToInt32(cellOldValue[0][0]) + 1;
            var cellNewValue = newValue.ToString();

            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue, $"{_databaseSheet}!{activityColumn}{row}");

            var dateTime = DateTime.Now;
            var lastUpdatedColumn = GetColumnOfValue(_databaseColumnHeaders, "Activity last updated");
            if (activityColumn == null) return "Could not find the column \"Activity last updated\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, dateTime.ToString(), $"{_databaseSheet}!{lastUpdatedColumn}{row}");

            return "Updated " + activityType + ".";
        }

        internal string RemoveActivity(string familyName, string activityType)
        {
            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            if (row == 0) return "Could not find family name";

            string columnHeader = activityType;

            var activityColumn = GetColumnOfValue(_databaseColumnHeaders, columnHeader);
            if (activityColumn == null) return "Could not find the stat \"" + activityType + "\"";

            var cellOldValue = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet}!{activityColumn}{row}");
            int newValue;
            if (cellOldValue == null) return null;
            else newValue = Convert.ToInt32(cellOldValue[0][0]) - 1;

            //If 0 put blank
            string cellNewValue;
            if (newValue == 0) cellNewValue = "";
            else cellNewValue = newValue.ToString();

            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue, $"{_databaseSheet}!{activityColumn}{row}");

            return "Updated " + activityType + ".";
        }

        internal MemberActivity GetActivity(string familyName)
        {
            var dictionary = new Dictionary<string, string>();

            var row = GetRowOfValue(familyName, _databaseFamilyNames);
            //if (row == 0) return "Could not find family name";

            var columnHeaders = DatabaseHelper.GetActivityHeaders();

            var startColumn = GetColumnOfValue(_databaseColumnHeaders, columnHeaders.FirstOrDefault());
            var endColumn = GetColumnOfValue(_databaseColumnHeaders, columnHeaders.LastOrDefault());
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet}!{startColumn}{row}:{endColumn}{row}");
            if (values == null) return null;

            for (var i = 0; i < columnHeaders.Count(); i++)
            {
                string value;
                try
                {
                    value = values[0][i].ToString();
                }
                catch
                {
                    value = "N/A";
                }
                if (value == "") value = "N/A";
                dictionary.TryAdd(columnHeaders[i], value);
            }

            //TODO: Better way of doing this (So I don't have to update gear headers in multiple places
            var memberActivity = new MemberActivity();
            memberActivity.Tier = dictionary.GetValueOrDefault("Tier");
            memberActivity.GA = dictionary.GetValueOrDefault("GA");

            memberActivity.GQ = dictionary.GetValueOrDefault("GQ");
            memberActivity.NW = dictionary.GetValueOrDefault("NW");
            memberActivity.Villa = dictionary.GetValueOrDefault("Villa");
            memberActivity.Militia = dictionary.GetValueOrDefault("Militia");
            memberActivity.Seamonsters = dictionary.GetValueOrDefault("Seamonsters");

            memberActivity.InactivityNotice = dictionary.GetValueOrDefault("InactivityNotice");
            memberActivity.ActivityLastUpdated = dictionary.GetValueOrDefault("Activity last updated");
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
            var columnLetters = DatabaseHelper.GetColumnLetters();
            var columnLetter = columnLetters[columnNumber - 1];

            return columnLetter;
        }

        private IList<IList<object>> GetColumnValues(string column)
        {
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet}!{column}:{column}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return values;
        }

        private IList<IList<object>> GetRowValues(string sheet, int row)
        {
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{sheet}!{row}:{row}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return values;
        }
    }
}