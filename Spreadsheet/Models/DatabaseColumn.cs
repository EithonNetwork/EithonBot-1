using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    public class DatabaseColumn
    {
        //TODO: Currently this is used for the parties sheet as well, is that good?
        public string ColumnLetters { get; private set; }
        public string ColumnHeader { get; private set; }
        public bool EditableByCommand { get; private set; }
        public ColumnSectionEnum ColumnSection { get; private set; }

        public enum ColumnSectionEnum
        {
            UnidentifiedSection,
            Identifiers,
            MemberInfo,
            Roles,
            Signup,
            Activity,
            Gear
        }

        public DatabaseColumn(string columnLetters, string columnHeader, bool editableByCommand, ColumnSectionEnum columnSection)
        {
            ColumnLetters = columnLetters;
            ColumnHeader = columnHeader;
            EditableByCommand = editableByCommand;
            ColumnSection = columnSection;
        }
    }
}
