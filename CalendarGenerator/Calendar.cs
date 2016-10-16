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
            { NHLTeam.ANA, "Anaheim" },
            { NHLTeam.ARI, "Arizona" },
            { NHLTeam.BOS, "Boston" },
            { NHLTeam.BUF, "Buffalo" },
            { NHLTeam.CAR, "Carolina" },
            { NHLTeam.CBJ, "Columbus" },
            { NHLTeam.CGY, "Calgary" },
            { NHLTeam.CHI, "Chicago" },
            { NHLTeam.COL, "Colorado" },
            { NHLTeam.DAL, "Dallas" },
            { NHLTeam.DET, "Detroit" },
            { NHLTeam.EDM, "Edmonton" },
            { NHLTeam.FLA, "Florida" },
            { NHLTeam.LAK, "Los Angeles" },
            { NHLTeam.MIN, "Minnesota" },
            { NHLTeam.MTL, "Montreal" },
            { NHLTeam.NJD, "New Jersey" },
            { NHLTeam.NSH, "Nashville" },
            { NHLTeam.NYI, "N.Y. Islanders" },
            { NHLTeam.NYR, "N.Y. Rangers" },
            { NHLTeam.OTT, "Ottawa" },
            { NHLTeam.PHI, "Philadelphia" },
            { NHLTeam.PIT, "Pittsburgh" },
            { NHLTeam.SJS, "San Jose" },
            { NHLTeam.STL, "St. Louis" },
            { NHLTeam.TBL, "Tampa Bay" },
            { NHLTeam.TOR, "Toronto" },
            { NHLTeam.VAN, "Vancouver" },
            { NHLTeam.WPG, "Winnipeg" },
            { NHLTeam.WSH, "Washington" },
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


            DateTime currentDate = startDate;
            int currentWeek = 1;
            int parsedDay = 1;
            int parsedMonth = 1;
            int previouslyParsedMonth = 1;
            bool newYearTransition = false;

            using (StreamReader sr = new StreamReader("fullschedule.txt"))
            {
                

                while (sr.Peek() >= 0)
                {
                    // Read the current line to see if it's a date or a match
                    string lineRead = sr.ReadLine();
                    bool isDayLine = false;
                    foreach (string day in Enum.GetNames(typeof(DayOfWeek)))
                    {
                        if (lineRead.StartsWith(day))
                        {
                            isDayLine = true;
                            break;
                        }
                    }

                    if (isDayLine)
                    {
                        string[] dayLineSplit = lineRead.Split(' ');

                        foreach (string month in Enum.GetNames(typeof(Months)))
                        {
                            if (dayLineSplit[1] == month)
                            {
                                parsedMonth = Convert.ToInt32(Enum.Parse(typeof(Months), month));

                                // Set next year on transition from december to january
                                if (previouslyParsedMonth == (int)Months.December && parsedMonth == (int)Months.January)
                                    parsedYear++;

                                previouslyParsedMonth = parsedMonth;

                                break;
                            }
                        }

                        parsedDay = Convert.ToInt32(dayLineSplit[2]);

                        currentDate = new DateTime(parsedYear, parsedMonth, parsedDay, new System.Globalization.GregorianCalendar());

                        if (newYearTransition && currentDate.DayOfYear <= weekStart[currentWeek])
                            newYearTransition = false;

                        if (currentDate.DayOfYear >= weekStart[currentWeek])
                        {
                            if (!newYearTransition)
                            {
                                int prevWeek = weekStart[currentWeek];
                                currentWeek++;

                                if (weekStart[currentWeek] < prevWeek)
                                    newYearTransition = true;
                            }
                        }

                        
                    }
                    else
                    {
                        //Read the visiting and local team
                        lineRead = lineRead.Replace(" at ", "!");
                        string[] matchupLineSplit = lineRead.Split('!');

                        //Add the matchup

                        NHLTeam visitor = NHLTeam.ANA;
                        NHLTeam home = NHLTeam.ANA;
                        foreach (KeyValuePair<NHLTeam, string> team in TeamName)
                        {
                        	if (matchupLineSplit[0].StartsWith(team.Value))
                        		visitor = team.Key;

                        	if (matchupLineSplit[1].StartsWith(team.Value))
                        		home = team.Key;
                        }

                        _matchups.Add(new Matchup(visitor, home, currentDate, currentWeek));
                    }
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
