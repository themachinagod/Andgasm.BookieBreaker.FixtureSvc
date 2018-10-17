using System;

namespace Andgasm.BookieBreaker.Models
{
    public class FixtureParticipantsModel
    {
        public DateTime KickOffTime { get; set; }
        public string FinalScore { get; set; }
        public string HomeClubCode { get; set; }
        public string AwayClubCode { get; set; }

        public string SeasonCode { get; set; }
        public string CountryCode { get; set; }
        public string FixtureCode { get; set; }
        public string TournamentCode { get; set; }
        public string RegionCode { get; set; }

        public int HomeGoalsScored { get; set; }
        public int HomeGoalsConceded { get; set; }

        public int AwayGoalsScored { get; set; }
        public int AwayGoalsConceded { get; set; }
    }
}
