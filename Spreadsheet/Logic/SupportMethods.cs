using EithonBot.Spreadsheet.Handler;
using EithonBot.Spreadsheet.Models;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EithonBot.Spreadsheet.Logic
{
    class SupportMethods
    {
        internal static Dictionary<string, DatabaseField> GetDatabaseFieldsFromConsecutiveColumns(DatabaseSheet sheet, Dictionary<string, DatabaseColumn> columns, DatabaseRow row)
        {
            var databaseFields = new Dictionary<string, DatabaseField>();
            foreach (var column in columns)
            {
                var databaseField = new DatabaseField(sheet, column.Value, row);
                databaseFields.TryAdd(column.Key, databaseField);
            }
            return databaseFields;
        }

        internal static DatabaseField PopulateFieldWithValues(SheetsService service, string spreadsheetId, DatabaseField databaseField)
        {
            var values = SpreadsheetHandler.getValuesFromRange(service, spreadsheetId, databaseField.CellReference);
            if (values == null) return null;
            var flattenedValues = FlattenSpreadsheetValues(values);

            databaseField.CellValue = flattenedValues.FirstOrDefault();
            return databaseField;
        }

        internal static Dictionary<string, DatabaseField> PopulateConsecutiveFieldsWithValues(SheetsService service, string spreadsheetId, Dictionary<string, DatabaseField> fields)
        {
            var sheet = fields.FirstOrDefault().Value.Sheet;
            var startColumn = fields.FirstOrDefault().Value.Column;
            var endColumn = fields.LastOrDefault().Value.Column;
            var row = fields.FirstOrDefault().Value.Row;

            var values = SpreadsheetHandler.getValuesFromRange(service, spreadsheetId, $"{sheet.Name}!{startColumn.ColumnLetters}{row.RowNumber}:{endColumn.ColumnLetters}{row.RowNumber}");
            var flattenedValues = FlattenSpreadsheetValues(values);

            for(var i = 0; i < fields.Count(); i++)
            {
                fields.Values.ElementAt(i).CellValue = flattenedValues[i];
            }

            return fields;
        }

        internal static List<string> FlattenSpreadsheetValues(IList<IList<object>> values)
        {
            var stringList = values.SelectMany(v => v).OfType<string>().ToList();
            return stringList;
        }
    }
}
