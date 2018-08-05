using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Spreadsheet.Models
{
    class DatabaseSheet
    {
        public string Name { get; set; }

        public DatabaseSheet(string name)
        {
            Name = name;
        }
    }
}
