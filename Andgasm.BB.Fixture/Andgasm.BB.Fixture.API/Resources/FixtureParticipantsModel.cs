using System;

namespace Andgasm.BB.Fixture.Resources
{
    public class FixtureParticipantsModel
    {
        public DateTime KickOffTime { get; set; }
        public string FinalScore { get; set; }
        public string HomeClubKey { get; set; }
        public string AwayClubKey { get; set; }

        public string SeasonKey { get; set; }
        public string CountryKey { get; set; }
        public string FixtureKey { get; set; }
        public string TournamentKey { get; set; }
        public string RegionKey { get; set; }

        public int HomeGoalsScored { get; set; }
        public int HomeGoalsConceded { get; set; }

        public int AwayGoalsScored { get; set; }
        public int AwayGoalsConceded { get; set; }
    }
}
