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

        public MemberSignupData()
        {
            FirstEvent = new DatabaseField();
            SecondEvent = new DatabaseField();
            ThirdEvent = new DatabaseField();
            FourthEvent = new DatabaseField();
            SignupComment = new DatabaseField();
        }
    }
}
