using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    public class DatabaseMember
    {
        public string FamilyName { get; set; }
        public string CharacterName { get; set; }

        public DatabaseMember(string familyName, string characterName)
        {
            FamilyName = familyName;
            CharacterName = characterName;
        }
    }
}
