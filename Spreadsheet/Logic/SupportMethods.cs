using EithonBot.Spreadsheet.Handler;
using EithonBot.Spreadsheet.Models;
using EithonBot.Spreadsheet.NewFolder;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EithonBot.Spreadsheet.Logic
{
    public class SupportMethods
    {
        private SheetsService _service;

        internal Models.Spreadsheet _spreadsheet;

        public SupportMethods(SheetsService service, Models.Spreadsheet spreadsheet)
        {
            _service = service;
            _spreadsheet = spreadsheet;
        }

        internal Dictionary<string, DatabaseColumn> GetColumns(DatabaseSheet sheet, int headersRow)
        {
            var columns = new Dictionary<string, DatabaseColumn>();
            var headers = GetRowValues(sheet, headersRow);
            for (var i = 0; i < headers.Count(); i++)
            {
                var columnLetters = DatabaseHelper.GetColumnLetters()[i];
                var columnName = headers[i];
                var column = new DatabaseColumn(columnLetters, columnName);
                columns.TryAdd(columnName, column);
            }
            return columns;
        }

        internal Dictionary<string, DatabaseRow> FetchAllDatabaseRowsFromSpreadsheet(string familyNamesColumn)
        {
            var familyNames = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{_spreadsheet.DatabaseSheet.Name}!{familyNamesColumn}:{familyNamesColumn}");
            var characterNames = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{_spreadsheet.DatabaseSheet.Name}!{familyNamesColumn}:{familyNamesColumn}");
            if (familyNames.Count == 0) return null;

            var rowDictionary = new Dictionary<string, DatabaseRow>();
            for (var i = 0; i < familyNames.Count; i++)
            {
                try
                {
                    var familyName = familyNames[i][0].ToString();
                    var characterName = characterNames[i][0].ToString();
                    var member = new DatabaseMember(familyName, characterName);
                    var databaseRow = new DatabaseRow(i + 1, member);
                    rowDictionary.TryAdd(familyName, databaseRow);
                }
                catch { continue; }
            }

            _spreadsheet.DatabaseSheet.DatabaseRows = rowDictionary;
            return rowDictionary;
        }

        internal IList<IList<object>> GetColumnValues(DatabaseSheet sheet, string column)
        {
            var values = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{sheet.Name}!{column}:{column}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            return values;
        }

        internal IList<string> GetRowValues(DatabaseSheet sheet, int row)
        {
            var values = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{sheet.Name}!{row}:{row}");
            if (values.Count == 0)
            {
                Console.WriteLine("No data in range found");
                return null;
            }
            var result = values.SelectMany(i => i).ToList();
            var stringResults = result.Select(r => r.ToString()).ToList();
            return stringResults;
        }

        internal IList<IList<object>> ParseToConsecutiveSpreadsheetValues(List<string> values)
        {
            IList<Object> obj = new List<Object>(values);
            IList<IList<Object>> spreadsheetFriendlyValues = new List<IList<Object>> { obj };

            return spreadsheetFriendlyValues;
        }

        internal bool MemberExists(string familyName)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return false;
            return true;
        }

        internal Dictionary<string, DatabaseField> GetDatabaseFieldsFromConsecutiveColumns(DatabaseSheet sheet, Dictionary<string, DatabaseColumn> columns, DatabaseRow row)
        {
            var databaseFields = new Dictionary<string, DatabaseField>();
            foreach (var column in columns)
            {
                var databaseField = new DatabaseField(sheet, column.Value, row);
                databaseFields.TryAdd(column.Key, databaseField);
            }
            return databaseFields;
        }

        internal DatabaseField PopulateFieldWithValues(SheetsService service, string spreadsheetId, DatabaseField databaseField)
        {
            var values = SpreadsheetHandler.GetValuesFromRange(service, spreadsheetId, databaseField.CellReference);
            if (values == null) return null;
            var flattenedValues = FlattenSpreadsheetValues(values);

            databaseField.CellValue = flattenedValues.FirstOrDefault();
            return databaseField;
        }

        internal Dictionary<string, DatabaseField> PopulateConsecutiveFieldsWithValues(SheetsService service, string spreadsheetId, Dictionary<string, DatabaseField> fields)
        {
            var sheet = fields.FirstOrDefault().Value.Sheet;
            var startColumn = fields.FirstOrDefault().Value.Column;
            var endColumn = fields.LastOrDefault().Value.Column;
            var row = fields.FirstOrDefault().Value.Row;

            var values = SpreadsheetHandler.GetValuesFromRange(service, spreadsheetId, $"{sheet.Name}!{startColumn.ColumnLetters}{row.RowNumber}:{endColumn.ColumnLetters}{row.RowNumber}");
            if (values == null) return null;
            var flattenedValues = FlattenSpreadsheetValues(values);

            for (var i = 0; i < fields.Count(); i++)
            {
                fields.Values.ElementAt(i).CellValue = flattenedValues[i];
            }

            return fields;
        }

        internal List<string> FlattenSpreadsheetValues(IList<IList<object>> values)
        {
            var stringList = values.SelectMany(v => v).OfType<string>().ToList();
            return stringList;
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
                    row = new DatabaseRow(rowNumber, _spreadsheet.GetDatabaseRows().FirstOrDefault(x => x.Value.RowNumber == rowNumber).Value.Member);
                    break;
                }
            }
            return row;
        }

        internal string GetColumnOfValue(IList<IList<object>> rowValues, string value)
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

        internal void ClearColumns(Dictionary<string, DatabaseColumn> columnsToReset)
        {
            foreach (var i in columnsToReset)
            {
                var column = i.Value;
                SpreadsheetHandler.ClearRange(_service, _spreadsheet.Id, $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{_spreadsheet.DatabaseSheet.ColumnHeadersRow + 2}:{column.ColumnLetters}");
            }
        }
    }
}
