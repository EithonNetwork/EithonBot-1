using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Models
{
    class Weekday
    {
        public DayEnum Day { get; private set; }

        public enum DayEnum {
            Monday = 0,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        };

        public Weekday(DayEnum day)
        {
            Day = day;
        }

        public void NextDay()
        {
            if (Day == DayEnum.Sunday) {
                Day = DayEnum.Monday;
                return;
            }
            Day++;
        }

        public override string ToString()
        {
            return Day.ToString();
        }

    }
}
