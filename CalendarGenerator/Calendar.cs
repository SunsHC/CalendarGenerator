using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalendarGenerator
{
    class Calendar
    {
        private List<Matchup> _matchups = new List<Matchup>();
        private int _numberOfWeeks = 0;

        
        public Dictionary<NHLTeam, string> TeamName = new Dictionary<NHLTeam, string>()
        {
            { NHLTeam.ANA, "Anaheim Ducks" },
            { NHLTeam.ARI, "Arizona Coyotes" },
            { NHLTeam.BOS, "Boston Bruins" },
            { NHLTeam.BUF, "Buffalo Sabres" },
            { NHLTeam.CAR, "Carolina Hurricanes" },
            { NHLTeam.CBJ, "Columbus Blue Jackets" },
            { NHLTeam.CGY, "Calgary Flames" },
            { NHLTeam.CHI, "Chicago Blackhawks" },
            { NHLTeam.COL, "Colorado Avalanche" },
            { NHLTeam.DAL, "Dallas Stars" },
            { NHLTeam.DET, "Detroit Red Wings" },
            { NHLTeam.EDM, "Edmonton Oilers" },
            { NHLTeam.FLA, "Florida Panthers" },
            { NHLTeam.LAK, "Los Angeles Kings" },
            { NHLTeam.MIN, "Minnesota Wild" },
            { NHLTeam.MTL, "Montreal Canadiens" },
            { NHLTeam.NJD, "New Jersey Devils" },
            { NHLTeam.NSH, "Nashville Predators" },
            { NHLTeam.NYI, "New York Islanders" },
            { NHLTeam.NYR, "New York Rangers" },
            { NHLTeam.OTT, "Ottawa Senators" },
            { NHLTeam.PHI, "Philadelphia Flyers" },
            { NHLTeam.PIT, "Pittsburgh Penguins" },
            { NHLTeam.SJS, "San Jose Sharks" },
            { NHLTeam.STL, "St. Louis Blues" },
            { NHLTeam.TBL, "Tampa Bay Lightning" },
            { NHLTeam.TOR, "Toronto Maple Leafs" },
            { NHLTeam.VAN, "Vancouver Canucks" },
            { NHLTeam.VGK, "Vegas Golden Knights" },
            { NHLTeam.WPG, "Winnipeg Jets" },
            { NHLTeam.WSH, "Washington Capitals" },
        };

        internal void CreateMatchups(DateTime startDate)
        {
            int parsedYear = startDate.Year;

            // This list contains the positions where the nchl weeks starts (from 1-366). We will use it as reference when assigning the week
            List<int> weekStart = new List<int>();

            // Add the first week, regardles of it's day in the week
            weekStart.Add(startDate.DayOfYear);

            // If the first game of the season was not on tuesday, we need to position the second week on a tuesday.
            // If the first game of the season was on tuesday, we are already correctly positionned.
            if (startDate.DayOfWeek != DayOfWeek.Tuesday)
            {
                // Get the first tuesday following the start date of the season, to determine nchl weeks
                while (startDate.DayOfWeek != DayOfWeek.Tuesday)
                {
                    startDate = startDate.AddDays(1);
                }

                weekStart.Add(startDate.DayOfYear);
            }

            //Fill the list with all the days of the years which are on a tuesday (do it for 40 weeks even if it's overkill)
            for (int i = 0; i < 40; i++)
            {
                startDate = startDate.AddDays(7);
                weekStart.Add(startDate.DayOfYear);
            }

            int currentWeek = 1;
            bool newYearTransition = false;

            using (StreamReader sr = new StreamReader("rawschedule.csv"))
            {
                

                while (sr.Peek() >= 0)
                {
                    // Read the current line to see if it's a date or a match
                    string lineRead = sr.ReadLine();

                    var matchupDate = DateTime.Parse(lineRead.Split(',')[0]);
                    var visitorTeam = TeamName.FirstOrDefault(tn => tn.Value == lineRead.Split(',')[2]).Key;
                    var homeTeam = TeamName.FirstOrDefault(tn => tn.Value == lineRead.Split(',')[3]).Key;

                    if (newYearTransition && matchupDate.DayOfYear <= weekStart[currentWeek])
                        newYearTransition = false;

                    if (matchupDate.DayOfYear >= weekStart[currentWeek])
                    {
                        if (!newYearTransition)
                        {
                            int prevWeek = weekStart[currentWeek];
                            currentWeek++;

                            if (weekStart[currentWeek] < prevWeek)
                                newYearTransition = true;
                        }
                    }

                    _matchups.Add(new Matchup(visitorTeam, homeTeam, matchupDate, currentWeek));
                }
            }

            _numberOfWeeks = currentWeek;
        }

        internal void OutputSeasonCalendar()
        {
        	using (StreamWriter sw = new StreamWriter("SeasonSchedule.csv"))
        	{
        		string lineToWrite = string.Empty;

        		for (int i = 1; i <= _numberOfWeeks; i++)
        		{
        			lineToWrite += "," + i.ToString();
        		}
        		sw.WriteLine(lineToWrite);

                foreach (NHLTeam team in TeamName.Keys)
    			{
    				lineToWrite = team.ToString();

    				for (int i = 1; i <= _numberOfWeeks; i++)
        			{
        				lineToWrite += "," + GetTeamMatchupsForWeek(team, i).Count;
        			}

        			sw.WriteLine(lineToWrite);
    			}        		        		
        	}
        }

        internal void OutputWeekCalendar()
        {
            for (int i = 1; i <= _numberOfWeeks; i++)
            {
                using (StreamWriter sw = new StreamWriter("Calendrier_Semaine_" + i +".csv"))
                {
                    string lineToWrite = string.Empty;

                    foreach (NHLTeam team in TeamName.Keys)
                    {
                        lineToWrite = team.ToString();
                        sw.WriteLine(lineToWrite);

                        foreach (Matchup match in GetTeamMatchupsForWeek(team, i))
                        {
                            lineToWrite = match.Date.ToShortDateString() + " ";
                            lineToWrite += match.VisitorTeam.ToString() + " vs. " + match.LocalTeam.ToString(); 
                            sw.WriteLine(lineToWrite);
                        }

                        sw.WriteLine(string.Empty);
                    }
                }
            }
        }

        internal List<Matchup> GetTeamMatchupsForWeek(NHLTeam team, int week)
        {
        	return _matchups.Where(match => (match.VisitorTeam == team || match.LocalTeam == team)
        	 && match.NCHLWeek == week).ToList();

        }
    }
}
