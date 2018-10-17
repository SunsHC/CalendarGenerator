using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalendarGenerator
{
    public class JsonDates
    {
        public List<JsonDate> Dates;
    }

    public class JsonDate
    {
        public string Date;
        public List<JsonGame> Games;
    }

    public class JsonGame
    {
        public JsonTeams Teams;
    }

    public class JsonTeams
    {
        public JsonTeam Away;
        public JsonTeam Home;
    }

    public class JsonTeam
    {
        public JsonTeamInfo Team;
    }

    public class JsonTeamInfo
    {
        public string Id;
        public string Name;
    }
}
