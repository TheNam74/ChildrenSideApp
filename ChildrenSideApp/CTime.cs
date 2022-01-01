using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChildrenSideApp
{
    public class CTime
    {
        public CTime(int Hour, int Minute, int Hour2, int Minute2,int Duration, int Interupt,int Sum)
        {
            To = new Interval(Hour2, Minute2);
            From = new Interval(Hour, Minute);
            this.Duration = Duration;
            this.Interupt = Interupt;
            this.Sum = Sum;
        }
        public Interval From { get; set; }
        public Interval To { get; set; }
        public int Duration { get; set; }
        public int Interupt { get; set; }   
        public int Sum { get; set; }


    }
    public class Interval
    {
        public Interval(int Hour, int Minute)
        {
            this.Hour = Hour;
            this.Minute = Minute;
        }
        public int Hour { get; set; }
        public int Minute { get; set; }
    }
}
