using EithonBot.Spreadsheet.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Models
{
    class MemberSignupData
    {
        public DatabaseField FirstEvent { get; internal set; }
        public DatabaseField SecondEvent { get; internal set; }
        public DatabaseField ThirdEvent { get; internal set; }
        public DatabaseField FourthEvent { get; internal set; }
        public DatabaseField SignupComment { get; internal set; }

        public MemberSignupData(IList<DatabaseField> fieldList)
        {
            FirstEvent = fieldList[0];
            SecondEvent = fieldList[1];
            ThirdEvent = fieldList[2];
            FourthEvent = fieldList[3];
            SignupComment = fieldList[4];
        }

        public Dictionary<string, DatabaseField> GetFields()
        {
            var dictionary = new Dictionary<string, DatabaseField>
            {
                { FirstEvent.Column.ColumnHeader, FirstEvent },
                { SecondEvent.Column.ColumnHeader, SecondEvent },
                { ThirdEvent.Column.ColumnHeader, ThirdEvent },
                { FourthEvent.Column.ColumnHeader, FourthEvent },
                { SignupComment.Column.ColumnHeader, SignupComment }
            };
            return dictionary;
        }
    }
}
