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
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return false;

            SpreadsheetHandler.DeleteRow(_service, _spreadsheet.Id, row.RowNumber);

            //Update rows
            _supportMethods.FetchAllDatabaseRowsFromSpreadsheet(_spreadsheet.DatabaseSheet.FamilyNamesColumn);
            return true;
        }

        internal DatabaseField UpdateGearStat(string familyName, string columnHeader, string value)
        {
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var column = _spreadsheet.DatabaseSheet.DatabaseColumns.GetValueOrDefault(columnHeader);
            if (column == null) return null;
            if (column.ColumnSection != DatabaseColumn.ColumnSectionEnum.Gear || !column.EditableByCommand) return null;

            var field = new DatabaseField(_spreadsheet.DatabaseSheet, column, row);
            field.CellValue = value;
            
            _supportMethods.UpdateField(field);
            return field;
        }

        internal string ResetSignups()
        {
            var columsToReset = _supportMethods.GetColumnsOfSection(_spreadsheet.DatabaseSheet.DatabaseColumns, DatabaseColumn.ColumnSectionEnum.Signup);
            _supportMethods.ClearColumns(columsToReset);

            return "Reset nodewar signups. Use !nodewarsignup in the announcement channel to create a new signup interface for next week.";
        }

        internal MemberSignupData GetSignupData(string familyName)
        {
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var columns = _supportMethods.GetColumnsOfSection(_spreadsheet.DatabaseSheet.DatabaseColumns, DatabaseColumn.ColumnSectionEnum.Signup);

            var fields = _supportMethods.GetDatabaseFieldsFromConsecutiveColumns(_spreadsheet.DatabaseSheet, columns, row);
            if (fields == null) return null;

            var fieldsWithValues = _supportMethods.PopulateConsecutiveFieldsWithTheirSpreadsheetValues(_service, _spreadsheet.Id, fields);

            var memberData = new MemberSignupData(fieldsWithValues.Values.ToList());

            return memberData;
        }

        internal Dictionary<string, DatabaseField> GetGear(string familyName)
        {
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var gearColumns = _supportMethods.GetColumnsOfSection(_spreadsheet.DatabaseSheet.DatabaseColumns, DatabaseColumn.ColumnSectionEnum.Gear);
            //TODO: How do I do this without adding hard coded class header
            var classColumn = _spreadsheet.DatabaseSheet.DatabaseColumns.GetValueOrDefault("Class");

            var fields = _supportMethods.GetDatabaseFieldsFromConsecutiveColumns(_spreadsheet.DatabaseSheet, gearColumns, row);
            if (fields == null) return null;
            var classField = new DatabaseField(_spreadsheet.DatabaseSheet, classColumn, row);
            if (classField == null) return null;

            _supportMethods.PopulateConsecutiveFieldsWithTheirSpreadsheetValues(_service, _spreadsheet.Id, fields);
            _supportMethods.PopulateFieldWithTheirSpreadsheetValue(_service, _spreadsheet.Id, classField);

            fields.Add(classField.Column.ColumnHeader, classField);

            return fields;
        }

        internal List<object> GetPartyMembers(string partyName)
        {
            var column = _spreadsheet.PartiesSheet.DatabaseColumns.GetValueOrDefault(partyName);
            if (column == null) return null;

            var partyMembers = SpreadsheetHandler.GetValuesFromRange(_service, _spreadsheet.Id, $"{_spreadsheet.PartiesSheet}!{column.ColumnLetters}{_spreadsheet.PartiesSheet.ColumnHeadersRow + 1}:{column.ColumnLetters}");

            var result = partyMembers.SelectMany(i => i).ToList();

            return result;
        }

        internal string ResetActivity()
        {
            var columsToReset = _supportMethods.GetColumnsOfSection(_spreadsheet.DatabaseSheet.DatabaseColumns, DatabaseColumn.ColumnSectionEnum.Activity);
            _supportMethods.ClearColumns(columsToReset);

            return "Reset activity signups.";
        }

        //TODO: Better system for "string addOrRemove"
        internal DatabaseField ChangeActivity(string familyName, string columnHeader, string addOrRemove)
        {
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var column = _supportMethods
                .GetColumnsOfSection(_spreadsheet.DatabaseSheet.DatabaseColumns, DatabaseColumn.ColumnSectionEnum.Activity)
                .GetValueOrDefault(columnHeader);
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
            
            column = _spreadsheet.DatabaseSheet.DatabaseColumns.GetValueOrDefault("Activity last updated");
            if (column == null) return null;
            SpreadsheetHandler.UpdateCell(_service, _spreadsheet.Id, DateTime.Now.ToString(), $"{_spreadsheet.DatabaseSheet.Name}!{column.ColumnLetters}{row.RowNumber}");

            return field;
        }

        internal Dictionary<string, DatabaseField> GetActivity(string familyName)
        {
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var columns = _supportMethods.GetColumnsOfSection(_spreadsheet.DatabaseSheet.DatabaseColumns, DatabaseColumn.ColumnSectionEnum.Activity);

            var fields = _supportMethods.GetDatabaseFieldsFromConsecutiveColumns(_spreadsheet.DatabaseSheet, columns, row);
            if (fields == null) return null;

            _supportMethods.PopulateConsecutiveFieldsWithTheirSpreadsheetValues(_service, _spreadsheet.Id, fields);

            return fields;
        }

        internal DatabaseField UpdateSingleField(string familyName, string columnHeader, string value)
        {
            var row = _spreadsheet.DatabaseSheet.DatabaseRows.GetValueOrDefault(familyName);
            if (row == null) return null;

            var column = _spreadsheet.DatabaseSheet.DatabaseColumns.GetValueOrDefault(columnHeader);
            if (column == null) return null;

            var field = new DatabaseField(_spreadsheet.DatabaseSheet, column, row);
            field.CellValue = value;

            _supportMethods.UpdateField(field);

            return field;
        }
    }
}