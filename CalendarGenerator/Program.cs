using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            Calendar cal = new Calendar();
            cal.CreateMatchups();

            cal.OutputSeasonCalendar();
            cal.OutputWeekCalendar();
        }
    }
}
