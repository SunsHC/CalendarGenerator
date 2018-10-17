using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace CalendarGenerator
{
    class Calendar
    {
        private List<Matchup> _matchups = new List<Matchup>();
        private int _numberOfWeeks = 0;

        
        public Dictionary<int, NHLTeam> TeamName = new Dictionary<int, NHLTeam>()
        {
            { 24, NHLTeam.ANA },
            { 53, NHLTeam.ARI },
            { 6 , NHLTeam.BOS},
            { 7 , NHLTeam.BUF},
            { 12, NHLTeam.CAR },
            { 29, NHLTeam.CBJ },
            { 20, NHLTeam.CGY },
            { 16, NHLTeam.CHI },
            { 21, NHLTeam.COL },
            { 25, NHLTeam.DAL },
            { 17, NHLTeam.DET },
            { 22, NHLTeam.EDM },
            { 13, NHLTeam.FLA },
            { 26, NHLTeam.LAK },
            { 30, NHLTeam.MIN },
            { 8 , NHLTeam.MTL},
            { 1 , NHLTeam.NJD},
            { 18, NHLTeam.NSH },
            { 2 , NHLTeam.NYI},
            { 3 , NHLTeam.NYR},
            { 9 , NHLTeam.OTT},
            { 4 , NHLTeam.PHI},
            { 5 , NHLTeam.PIT},
            { 28, NHLTeam.SJS },
            { 19, NHLTeam.STL },
            { 14, NHLTeam.TBL },
            { 10, NHLTeam.TOR },
            { 23, NHLTeam.VAN },
            { 54, NHLTeam.VGK },
            { 52, NHLTeam.WPG },
            { 15, NHLTeam.WSH },
        };

        internal void CreateMatchups()
        {
            DateTime startDate = new DateTime(2018, 10, 3);
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



            using (WebClient client = new WebClient())
            {
                //client.Proxy = new MtxProxy();               
                string gamesJsonHTMLLink = $"https://statsapi.web.nhl.com/api/v1/schedule?site=en_nhl&startDate=2018-10-03&endDate=2019-04-06";

                string result = client.DownloadString(gamesJsonHTMLLink);
                JsonDates jsonDates = JsonConvert.DeserializeObject<JsonDates>(result);


                foreach (var date in jsonDates.Dates)
                {
                    var matchupDate = DateTime.Parse(date.Date);

                    foreach (var game in date.Games)
                    {
                        try
                        {
                            NHLTeam awayTeam = TeamName[Convert.ToInt32(game.Teams.Away.Team.Id)];
                            NHLTeam homeTeam = TeamName[Convert.ToInt32(game.Teams.Home.Team.Id)];

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

                            _matchups.Add(new Matchup(awayTeam, homeTeam, matchupDate, currentWeek));
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"New team or european team: {ex.Message}");
                            continue;
                        }
                    }
                }
            }

            _numberOfWeeks = currentWeek;
        }

        internal void OutputSeasonCalendar()
        {
        	using (StreamWriter sw = new StreamWriter("Calendrier_Saison.csv"))
        	{
        		string lineToWrite = string.Empty;

        		for (int i = 1; i <= _numberOfWeeks; i++)
        		{
        			lineToWrite += "," + i.ToString();
        		}
        		sw.WriteLine(lineToWrite);

                foreach (NHLTeam team in TeamName.Values)
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

                    foreach (NHLTeam team in TeamName.Values)
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

    internal class MtxProxy : IWebProxy
    {
        public ICredentials Credentials
        {
            get
            {
                return new NetworkCredential("alamarch", "iaia11()");
            }
            set { throw new NotImplementedException(); }
        }

        public Uri GetProxy(Uri destination)
        {
            return new Uri("http://webproxy.matrox.com:9090");
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }
    }
}
