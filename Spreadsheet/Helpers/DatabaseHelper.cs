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

        internal static List<string> GetGearHeaders(bool getOnlyCommandEditableHeaders = false)
        {
            var gearHeaders = new List<string>
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
                gearHeaders.Remove("Renown");
                gearHeaders.Remove("Gear last updated");
            }

            return gearHeaders;
        }

        internal static List<string> GetActivityHeaders(bool getOnlyCommandEditableHeaders = false)
        {
            var gearHeaders = new List<string>
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
                gearHeaders.Remove("Tier");
                gearHeaders.Remove("GA");
                gearHeaders.Remove("Activity last updated");
            }

            return gearHeaders;
        }

        internal static List<string> GetSignupHeaders()
        {
            var gearHeaders = new List<string>
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

            return gearHeaders;
        }
    }
}
