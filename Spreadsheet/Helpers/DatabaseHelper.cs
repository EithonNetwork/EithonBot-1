using EithonBot.Spreadsheet.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.NewFolder
{
    class DatabaseHelper
    {
        internal static List<string> GetColumnLetters()
        {
            var columnLetters = new List<string>
            {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH"
            };

            return columnLetters;
        }

        internal static DatabaseColumn GetClassColumn(Dictionary<string, DatabaseColumn> databaseColumns)
        {
            var header = "Class";
            var classColumn = databaseColumns.GetValueOrDefault(header);
            return classColumn;
        }

        internal static Dictionary<string, DatabaseColumn> GetGearColumns(Dictionary<string, DatabaseColumn> databaseColumns, bool getOnlyCommandEditableHeaders = false)
        {
            var headers = new List<string>
            {
                "Renown",
                "LVL",
                "AP",
                "AAP",
                "DP",
                "AlchStone",
                "Axe",
                "GearComment",
                "GearLink",
                "Gear last updated"
            };

            if (getOnlyCommandEditableHeaders)
            {
                headers.Remove("Renown");
                headers.Remove("Gear last updated");
            }

            var colums = new Dictionary<string, DatabaseColumn>();
            foreach (var header in headers) colums.TryAdd(header, databaseColumns.GetValueOrDefault(header));

            return colums;
        }

        internal static Dictionary<string, DatabaseColumn> GetActivityColumns(Dictionary<string, DatabaseColumn> databaseColumns, bool getOnlyCommandEditableHeaders = false)
        {
            var headers = new List<string>
            {
                "Tier",
                "GA",
                "GQ",
                "NW",
                "Villa",
                "Militia",
                "Seamonsters",
                "InactivityNotice",
                "Activity last updated"
            };

            if (getOnlyCommandEditableHeaders)
            {
                headers.Remove("Tier");
                headers.Remove("GA");
                headers.Remove("Activity last updated");
            }

            var colums = new Dictionary<string, DatabaseColumn>();
            foreach (var header in headers) colums.TryAdd(header, databaseColumns.GetValueOrDefault(header));

            return colums;
        }

        internal static Dictionary<string, DatabaseColumn> GetSignupColumns(Dictionary<string, DatabaseColumn> databaseColumns)
        {
            var headers = new List<string>
            {
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday",
                "SignupComment"
            };

            List<string> currentHeaders = new List<string>();
            foreach (var item in headers)
            {
                if (databaseColumns.ContainsKey(item)) currentHeaders.Add(item);
            }

            var colums = new Dictionary<string, DatabaseColumn>();
            foreach (var header in currentHeaders) colums.Add(header, databaseColumns.GetValueOrDefault(header));

            return colums;
        }
    }
}
