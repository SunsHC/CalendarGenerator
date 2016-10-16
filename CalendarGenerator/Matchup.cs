using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarGenerator
{
    public class Matchup
    {
        public NHLTeam VisitorTeam { get; private set; }
        public NHLTeam LocalTeam { get; private set; }
        public DateTime Date { get; private set; }
        public int NCHLWeek { get; private set; }

        public Matchup(NHLTeam visitorTeam, NHLTeam localTeam, DateTime date, int nchlWeek)
        {
           VisitorTeam = visitorTeam;
           LocalTeam = localTeam;
           Date = date;
           NCHLWeek = nchlWeek;
        }
    }
}
