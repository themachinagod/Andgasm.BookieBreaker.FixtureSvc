using System;

namespace Andgasm.BB.Fixture.Resources
{
    public class PlayerAppearanceModel
    {
        public string Key
        {
            get
            {
                return $"{PlayerKey}-{FixtureKey}";
            }
        }
        public string FixtureKey { get; set; }
        public string PlayerKey { get; set; }
        public string ClubKey { get; set; }

        public string PlayerCode { get; set; }
        public string ClubCode { get; set; }
        public string FixtureCode { get; set; }

        public bool IsStartingEleven { get; set; }
        public string PositionPlayed { get; set; }
        public bool IsManOfMatch { get; set; }

        public decimal Rating { get; set; }
        public int Touches { get; set; }

        //public int Goals { get; set; }
        //public int OwnGoals { get; set; }
    }
}
