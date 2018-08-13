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

        internal static DatabaseColumn GetColumn(string columnLetters, string columnHeader)
        {
            bool editableByCommand = false;
            DatabaseColumn.ColumnSectionEnum columnSection = DatabaseColumn.ColumnSectionEnum.UnidentifiedSection;
            
            //TODO: figure out a better way of doing this
            var columns = GetIdentifierColumnsInfo();
            if (columns.ContainsKey(columnHeader))
            {
                editableByCommand = columns.GetValueOrDefault(columnHeader);
                columnSection = DatabaseColumn.ColumnSectionEnum.Identifiers;
            }
            else
            {
                columns = GetMemberInfoColumnsInfo();
                if (columns.ContainsKey(columnHeader))
                {
                    editableByCommand = columns.GetValueOrDefault(columnHeader);
                    columnSection = DatabaseColumn.ColumnSectionEnum.MemberInfo;
                }
                else
                {
                    columns = GetRolesColumnsInfo();
                    if (columns.ContainsKey(columnHeader))
                    {
                        editableByCommand = columns.GetValueOrDefault(columnHeader);
                        columnSection = DatabaseColumn.ColumnSectionEnum.Roles;
                    }
                    else
                    {
                        columns = GetSignupColumnsInfo();
                        if (columns.ContainsKey(columnHeader))
                        {
                            editableByCommand = columns.GetValueOrDefault(columnHeader);
                            columnSection = DatabaseColumn.ColumnSectionEnum.Signup;
                        }
                        else
                        {
                            columns = GetActivityColumnsInfo();
                            if (columns.ContainsKey(columnHeader))
                            {
                                editableByCommand = columns.GetValueOrDefault(columnHeader);
                                columnSection = DatabaseColumn.ColumnSectionEnum.Activity;
                            }
                            else
                            {
                                columns = GetGearColumnsInfo();
                                if (columns.ContainsKey(columnHeader))
                                {
                                    editableByCommand = columns.GetValueOrDefault(columnHeader);
                                    columnSection = DatabaseColumn.ColumnSectionEnum.Gear;
                                }
                            }
                        }
                    }
                }
            }
            var column = new DatabaseColumn(columnLetters, columnHeader, editableByCommand, columnSection);
            return column;
        }

        private static Dictionary<string, bool> GetIdentifierColumnsInfo()
        {
            //string = ColumnHeader, bool = EditableByCommand
            var headers = new Dictionary<string, bool>
            {
                {"FamilyName", false},
                {"CharacterName", false},
            };
            return headers;
        }

        private static Dictionary<string, bool> GetMemberInfoColumnsInfo()
        {
            //string = ColumnHeader, bool = EditableByCommand
            var headers = new Dictionary<string, bool>
            {
                {"Class", false},
                {"DiscordUsername", false},
            };
            return headers;
        }

        private static Dictionary<string, bool> GetRolesColumnsInfo()
        {
            //string = ColumnHeader, bool = EditableByCommand
            var headers = new Dictionary<string, bool>
            {
                {"Officer", false},
                {"PartyLeader", false},
            };
            return headers;
        }

        internal static Dictionary<string, bool> GetGearColumnsInfo()
        {
            //string = ColumnHeader, bool = EditableByCommand
            var headers = new Dictionary<string, bool>
            {
                {"Renown", false},
                {"TerritoryFame", true},
                {"LVL", true},
                {"AP", true},
                {"AAP", true},
                {"DP", true},
                {"AlchStone", true},
                {"Axe", true},
                {"GearComment", true},
                {"GearLink", true},
                {"Gear last updated", true}
            };
            return headers;
        }

        internal static Dictionary<string, bool> GetActivityColumnsInfo()
        {
            //string = ColumnHeader, bool = EditableByCommand
            var headers = new Dictionary<string, bool>
            {
                {"Tier", true},
                {"GA", true},
                {"GQ", true},
                {"NW", true},
                {"Villa", true},
                {"Militia", true},
                {"Seamonsters", true},
                {"InactivityNotice", true},
                {"Activity last updated", true}
            };
            return headers;
        }

        internal static Dictionary<string, bool> GetSignupColumnsInfo()
        {
            //string = ColumnHeader, bool = EditableByCommand
            var headers = new Dictionary<string, bool>
            {
                {"Sunday", true},
                {"Monday", true},
                {"Tuesday", true},
                {"Wednesday", true},
                {"Thursday", true},
                {"Friday", true},
                {"Saturday", true},
                {"SignupComment", true}
            };
            return headers;
        }
    }
}
