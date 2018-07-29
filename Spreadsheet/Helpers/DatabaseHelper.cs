using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.NewFolder
{
    class DatabaseHelper
    {
        public static List<string> GetGearHeaders(bool getOnlyCommandEditableHeaders = false)
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

        public static List<string> GetActivityHeaders(bool getOnlyCommandEditableHeaders = false)
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

    }
}
