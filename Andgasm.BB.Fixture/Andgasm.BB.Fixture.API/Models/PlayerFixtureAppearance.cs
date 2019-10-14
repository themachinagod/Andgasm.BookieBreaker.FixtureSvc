using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andgasm.BB.Fixture.Models
{
    public class PlayerFixtureAppearance
    {
        [Key]
        public string Key { get; set; }
        public string ClubKey { get; set; }
        public string FixtureKey { get; set; }
        public string PlayerKey { get; set; }

        public string PlayerCode { get; set; }
        public string ClubCode { get; set; }
        public string FixtureCode { get; set; }

        public bool IsStartingEleven { get; set; }
        public string PositionPlayed { get; set; }
        public decimal Rating { get; set; }
        public int Touches { get; set; }
    }
}
