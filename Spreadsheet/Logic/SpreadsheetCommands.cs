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
using EithonBot.Spreadsheet.Models;
using EithonBot.Spreadsheet.Logic;

namespace EithonBot
{
    public class SpreadsheetCommands
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        private static SpreadsheetCommands _instance;
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Discord integration";
        private string _spreadsheetId;

        private DatabaseSheet _databaseSheet;
        private int _databaseColumnHeadersRow;
        internal Dictionary<string, DatabaseColumn> _databaseColumns;

        private DatabaseSheet _partiesSheet;
        private int _partyColumnHeadersRow;
        internal Dictionary<string, DatabaseColumn> _partyColumns;

        private string _databaseFamilyNamesColumn;
        //TODO: Update this after added/removed member so system works if you were added after the bot was started
        private Dictionary<string, DatabaseRow> _databaseRows;
        private SheetsService _service;
        
        public static SpreadsheetCommands Instance => _instance;

        static SpreadsheetCommands()
        {
            _instance = new SpreadsheetCommands("1pLMcQ7Uxha4g3c_poI7YTzZXla7omFwhQRUiCg8IzKI", "Guild database", "A", 3, "NW Parties", 3);
        }

        private SpreadsheetCommands(string spreadsheetId, string databaseSheet, string databaseFamilyNamesColumn, int databaseHeadersRow, string partiesSheet, int partiesSheetHeadersRow)
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
            _databaseSheet = new DatabaseSheet(databaseSheet);
            _databaseColumnHeadersRow = databaseHeadersRow;
            _databaseColumns = GetColumns(_databaseSheet, databaseHeadersRow);
            _databaseFamilyNamesColumn = databaseFamilyNamesColumn;
            _databaseRows = GetDatabaseRows();
            _partiesSheet = new DatabaseSheet(partiesSheet);
            _partyColumnHeadersRow = partiesSheetHeadersRow;
            _partyColumns = GetColumns(_partiesSheet, _partyColumnHeadersRow);
        }

        //TODO: Make a proper system for this
        internal bool MemberExists(string familyName)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return false;
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
                    _service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, _spreadsheetId, $"{_databaseSheet.Name}!A5:Y5");
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            var response = request.Execute();
            _databaseRows = GetDatabaseRows();
        }

        internal void RemoveMember(string familyName)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return;

            SpreadsheetHandler.DeleteRow(_service, _spreadsheetId, row.RowNumber);
            _databaseRows = GetDatabaseRows();
        }

        internal string ResetSignups()
        {
            string[] headersOfColumnsToReset = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "SignupComment" };
            clearColumns(_databaseColumns, headersOfColumnsToReset);

            return "Reset nodewar signups. Use !nodewarsignup in the announcement channel to create a new signup interface for next week.";
        }

        internal string ResetActivity()
        {
            string[] headersOfColumnsToReset = { "GQ", "NW", "VILLA", "MILITIA", "SEAMONSTERS"};
            clearColumns(_databaseColumns, headersOfColumnsToReset);

            return "Reset activity signups.";
        }

        private void clearColumns(Dictionary<string, DatabaseColumn> columns, string[] headerNameOfColumnsToReset)
        {
            foreach (var headerName in headerNameOfColumnsToReset)
            {
                var columnLetters = columns.GetValueOrDefault(headerName).ColumnLetters;
                if (columnLetters != null)
                {
                    ClearRange($"{_databaseSheet.Name}!{columnLetters}{_databaseColumnHeadersRow+2}:{columnLetters}");
                }
            }
        }

        //TODO: How do I make this proper async
        internal async void Signup(string familyName, string value, string signupMessage)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return;

            string columnHeader = null;
            var weekDay = new Weekday(DayEnum.Sunday);
            do
            {
                if (signupMessage.IndexOf(weekDay.ToString()) > -1) { columnHeader = weekDay.ToString(); }
                weekDay.NextDay();
            } while (weekDay.Day != DayEnum.Sunday);
            if (columnHeader == null) return;

            var columnLetters = _databaseColumns.GetValueOrDefault(columnHeader).ColumnLetters;
            if (columnLetters == null) return;
            
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId ,value, $"{_databaseSheet.Name}!{columnLetters}{row.RowNumber}");
        }

        internal MemberSignupData GetSignupData(string familyName)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var columns = DatabaseHelper.GetSignupColumns(_databaseColumns);
            
            var fields = SupportMethods.GetDatabaseFieldsFromConsecutiveColumns(_databaseSheet, columns, row);
            if (fields == null) return null;

            var fieldsWithValues = SupportMethods.PopulateConsecutiveFieldsWithValues(_service, _spreadsheetId, fields);

            var memberData = new MemberSignupData(fieldsWithValues.Values.ToList());

            return memberData;
        }

        internal DatabaseField UpdateField(string familyName, string columnHeader, string value)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var column = _databaseColumns.GetValueOrDefault(columnHeader);
            if (row == null) return null;

            var field = new DatabaseField(_databaseSheet, column, row, value);
            if (field == null) return null;
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, value, field.CellReference);

            return field;
        }

        internal Dictionary<string, DatabaseField> GetGear(string familyName)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var gearColumns = DatabaseHelper.GetGearColumns(_databaseColumns);
            var classColumn = DatabaseHelper.GetClassColumn(_databaseColumns);

            var fields = SupportMethods.GetDatabaseFieldsFromConsecutiveColumns(_databaseSheet, gearColumns, row);
            if (fields == null) return null;
            var classField = new DatabaseField(_databaseSheet, classColumn, row);
            if (classField == null) return null;

            SupportMethods.PopulateConsecutiveFieldsWithValues(_service, _spreadsheetId, fields);
            SupportMethods.PopulateFieldWithValues(_service, _spreadsheetId, classField);

            fields.Add(classField.Column.ColumnHeader, classField);

            return fields;
        }

        internal List<object> GetPartyMembers(string partyName)
        {
            var column = _partyColumns.GetValueOrDefault(partyName).ColumnLetters;
            if (column == null) return null;

            var partyMembers = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_partiesSheet}!{column}{_partyColumnHeadersRow + 1}:{column}");

            var result = partyMembers.SelectMany(i => i).ToList();

            return result;
        }

        internal string AddActivity(string familyName, string activityType)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return "Could not find family name";

            string columnHeader = activityType;

            var activityColumn = _databaseColumns.GetValueOrDefault(columnHeader);
            if (activityColumn == null) return null;
            var cellOldValue = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet.Name}!{activityColumn.ColumnLetters}{row.RowNumber}");
            int newValue;
            if (cellOldValue == null) newValue = 1;
            else newValue = Convert.ToInt32(cellOldValue[0][0]) + 1;
            var cellNewValue = newValue.ToString();

            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue, $"{_databaseSheet.Name}!{activityColumn.ColumnLetters}{row.RowNumber}");

            var dateTime = DateTime.Now;
            var lastUpdatedColumn = _databaseColumns.GetValueOrDefault("Activity last updated").ColumnLetters;
            if (activityColumn == null) return "Could not find the column \"Activity last updated\"";
            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, dateTime.ToString(), $"{_databaseSheet.Name}!{lastUpdatedColumn}{row.RowNumber}");

            return "Updated " + activityType + ".";
        }

        internal string RemoveActivity(string familyName, string activityType)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return "Could not find family name";

            string columnHeader = activityType;

            var activityColumn = _databaseColumns.GetValueOrDefault(columnHeader).ColumnLetters;
            if (activityColumn == null) return "Could not find the stat \"" + activityType + "\"";

            var cellOldValue = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet.Name}!{activityColumn}{row.RowNumber}");
            int newValue;
            if (cellOldValue == null) return null;
            else newValue = Convert.ToInt32(cellOldValue[0][0]) - 1;

            //If 0 put blank
            string cellNewValue;
            if (newValue == 0) cellNewValue = "";
            else cellNewValue = newValue.ToString();

            SpreadsheetHandler.UpdateCell(_service, _spreadsheetId, cellNewValue, $"{_databaseSheet.Name}!{activityColumn}{row.RowNumber}");

            return "Updated " + activityType + ".";
        }

        internal Dictionary<string, DatabaseField> GetActivity(string familyName)
        {
            var row = _databaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var columns = DatabaseHelper.GetActivityColumns(_databaseColumns);

            var fields = SupportMethods.GetDatabaseFieldsFromConsecutiveColumns(_databaseSheet, columns, row);
            if (fields == null) return null;

            SupportMethods.PopulateConsecutiveFieldsWithValues(_service, _spreadsheetId, fields);

            return fields;
        }

        private void ClearRange(string range)
        {
            SpreadsheetHandler.ClearRange(_service, _spreadsheetId, range);
        }

        private void DeleteRange(string range)
        {
            SpreadsheetHandler.ClearRange(_service, _spreadsheetId, range);
        }

        internal DatabaseRow GetRowOfValue(string value, IList<IList<object>> columnValues)
        {
            DatabaseRow row = null;
            int rowNumber = 0;
            for (int i = 0; i < columnValues.Count; i++)
            {
                var columnValuesFlattened = columnValues[i];
                if (columnValuesFlattened.Count == 0) continue;
                if (columnValuesFlattened[0].ToString() == value)
                {
                    rowNumber = i + 1;
                    row = new DatabaseRow(rowNumber, _databaseRows.FirstOrDefault(x => x.Value.RowNumber == rowNumber).Value.FamilyName);
                    break;
                }
            }
            return row;
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
            var columnsLetters = DatabaseHelper.GetColumnLetters();
            var columnLetters = columnsLetters[columnNumber - 1];

            return columnLetters;
        }

        private IList<IList<object>> GetColumnValues(string column)
        {
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet.Name}!{column}:{column}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return values;
        }

        private IList<string> GetRowValues(DatabaseSheet sheet, int row)
        {
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{sheet.Name}!{row}:{row}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            var result = values.SelectMany(i => i).ToList();
            var stringResults = result.Select(r => r.ToString()).ToList();
            return stringResults;
        }

        private Dictionary<string, DatabaseColumn> GetColumns(DatabaseSheet sheet, int headersRow)
        {
            var columns = new Dictionary<string, DatabaseColumn>();
            var headers = GetRowValues(sheet, headersRow);
            for (var i = 0; i< headers.Count(); i++)
            {
                var columnLetters = DatabaseHelper.GetColumnLetters()[i];
                var columnName = headers[i];
                var column = new DatabaseColumn(columnLetters, columnName);
                columns.TryAdd(columnName, column);
            }
            return columns;
        }

        private Dictionary<string, DatabaseRow> GetDatabaseRows()
        {
            var values = SpreadsheetHandler.getValuesFromRange(_service, _spreadsheetId, $"{_databaseSheet.Name}!{_databaseFamilyNamesColumn}:{_databaseFamilyNamesColumn}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }

            var rowDictionary = new Dictionary<string, DatabaseRow>();
            for (var i = 0; i < values.Count; i++)
            {
                try {
                    var familyName = values[i][0].ToString();
                    var databaseRow = new DatabaseRow(i + 1, familyName);
                    rowDictionary.TryAdd(familyName, databaseRow);
                }
                catch { continue; }
            }
            return rowDictionary;
        }
    }
}