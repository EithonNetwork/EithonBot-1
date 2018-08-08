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
    public class CommandsParser
    {
        internal SheetsService _service;
        internal Spreadsheet.Models.Spreadsheet _spreadsheet;
        internal SupportMethods _supportMethods;

        public CommandsParser(SheetsService service, Spreadsheet.Models.Spreadsheet spreadsheet, SupportMethods supportMethods)
        {
            _service = service;
            _spreadsheet = spreadsheet;
            _supportMethods = supportMethods;
        }

        internal DatabaseMember AddMember(string familyName, string characterName)
        {
            //TODO: throw?
            if (_supportMethods.MemberExists(familyName)) return null;
            var member = new DatabaseMember(familyName, characterName);
            var values = _supportMethods.ParseToConsecutiveSpreadsheetValues(new List<string> { familyName, characterName });

            SpreadsheetHandler.NewMember(_service, _spreadsheet.Id, _spreadsheet.DatabaseSheet, values);
            //Update rows
            _supportMethods.FetchAllDatabaseRowsFromSpreadsheet(_spreadsheet.DatabaseSheet.FamilyNamesColumn);
            return member;
        }

        internal bool RemoveMember(string familyName)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return false;

            SpreadsheetHandler.DeleteRow(_service, _spreadsheet.Id, row.RowNumber);

            //Update rows
            _supportMethods.FetchAllDatabaseRowsFromSpreadsheet(_spreadsheet.DatabaseSheet.FamilyNamesColumn);
            return true;
        }

        internal DatabaseField UpdateField(string familyName, string columnHeader, string value)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return null;

            var column = _spreadsheet.GetDatabaseColumns().GetValueOrDefault(columnHeader);
            if (column == null) return null;

            var field = new DatabaseField(_spreadsheet.DatabaseSheet, column, row);
            field.CellValue = value;

            SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, field.CellValue, field.CellReference);
            return field;
        }

        internal string ResetSignups()
        {
            var columsToReset = DatabaseHelper.GetSignupColumns(_spreadsheet.GetDatabaseColumns());
            _supportMethods.ClearColumns(columsToReset);

            return "Reset nodewar signups. Use !nodewarsignup in the announcement channel to create a new signup interface for next week.";
        }

        internal MemberSignupData GetSignupData(string familyName)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return null;

            var columns = DatabaseHelper.GetSignupColumns(_spreadsheet.GetDatabaseColumns());

            var fields = _supportMethods.GetDatabaseFieldsFromConsecutiveColumns(_spreadsheet.DatabaseSheet, columns, row);
            if (fields == null) return null;

            var fieldsWithValues = _supportMethods.PopulateConsecutiveFieldsWithValues(_service, _spreadsheet.Id, fields);

            var memberData = new MemberSignupData(fieldsWithValues.Values.ToList());

            return memberData;
        }

        internal Dictionary<string, DatabaseField> GetGear(string familyName)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return null;

            var gearColumns = DatabaseHelper.GetGearColumns(_spreadsheet.GetDatabaseColumns());
            var classColumn = DatabaseHelper.GetClassColumn(_spreadsheet.GetDatabaseColumns());

            var fields = _supportMethods.GetDatabaseFieldsFromConsecutiveColumns(_spreadsheet.DatabaseSheet, gearColumns, row);
            if (fields == null) return null;
            var classField = new DatabaseField(_spreadsheet.DatabaseSheet, classColumn, row);
            if (classField == null) return null;

            _supportMethods.PopulateConsecutiveFieldsWithValues(_service, _spreadsheet.Id, fields);
            _supportMethods.PopulateFieldWithValues(_service, _spreadsheet.Id, classField);

            fields.Add(classField.Column.ColumnHeader, classField);

            return fields;
        }

        internal List<object> GetPartyMembers(string partyName)
        {
            var column = _spreadsheet.GetPartiesColumns().GetValueOrDefault(partyName);
            if (column == null) return null;

            var partyMembers = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{_spreadsheet.PartiesSheet}!{column.ColumnLetters}{_spreadsheet.PartiesSheet.ColumnHeadersRow + 1}:{column.ColumnLetters}");

            var result = partyMembers.SelectMany(i => i).ToList();

            return result;
        }

        internal string ResetActivity()
        {
            var columsToReset = DatabaseHelper.GetActivityColumns(_spreadsheet.GetDatabaseColumns(), true);
            _supportMethods.ClearColumns(columsToReset);

            return "Reset activity signups.";
        }

        //TODO: Better system for "string addOrRemove"
        internal DatabaseField ChangeActivity(string familyName, string columnHeader, string addOrRemove)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return null;

            var column = DatabaseHelper.GetActivityColumns(_spreadsheet.GetDatabaseColumns(), true).GetValueOrDefault(columnHeader);
            if (column == null) return null;
            var cellOldValue = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");

            var field = new DatabaseField(_spreadsheet.DatabaseSheet, column, row);
            int newValue;
            if (addOrRemove == "add")
            {
                if (cellOldValue == null) newValue = 1;
                else newValue = Convert.ToInt32(cellOldValue[0][0]) + 1;
                field.CellValue = newValue.ToString();
            }

            if (addOrRemove == "remove")
            {
                if (cellOldValue == null) return null;
                else newValue = Convert.ToInt32(cellOldValue[0][0]) - 1;

                //If 0 put blank
                if (newValue == 0) field.CellValue = "";
                else field.CellValue = newValue.ToString();
            }

            SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, field.CellValue, $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");
            
            column = _spreadsheet.GetDatabaseColumns().GetValueOrDefault("Activity last updated");
            if (column == null) return null;
            SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, DateTime.Now.ToString(), $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");

            return field;
        }

        //internal string RemoveActivity(string familyName, string activityType)
        //{
        //    var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
        //    if (row == null) return "Could not find family name";

        //    string columnHeader = activityType;

        //    var column = _spreadsheet.GetDatabaseColumns().GetValueOrDefault(columnHeader);
        //    if (column == null) return "Could not find the stat \"" + activityType + "\"";

        //    var cellOldValue = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");
        //    int newValue;
        //    if (cellOldValue == null) return null;
        //    else newValue = Convert.ToInt32(cellOldValue[0][0]) - 1;

        //    //If 0 put blank
        //    string cellNewValue;
        //    if (newValue == 0) cellNewValue = "";
        //    else cellNewValue = newValue.ToString();

        //    SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, cellNewValue, $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");

        //    var dateTime = DateTime.Now;
        //    column = _spreadsheet.GetDatabaseColumns().GetValueOrDefault("Activity last updated");
        //    if (column == null) return "Could not find the column \"Activity last updated\"";
        //    SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, DateTime.Now.ToString(), $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");

        //    return "Updated " + activityType + ".";
        //}

        internal Dictionary<string, DatabaseField> GetActivity(string familyName)
        {
            var row = _spreadsheet.GetDatabaseRows().GetValueOrDefault(familyName);
            if (row == null) return null;

            var columns = DatabaseHelper.GetActivityColumns(_spreadsheet.GetDatabaseColumns());

            var fields = _supportMethods.GetDatabaseFieldsFromConsecutiveColumns(_spreadsheet.DatabaseSheet, columns, row);
            if (fields == null) return null;

            _supportMethods.PopulateConsecutiveFieldsWithValues(_service, _spreadsheet.Id, fields);

            return fields;
        }
    }
}