using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andgasm.BB.Fixture.Models
{
    public class PlayerFixtureAppearance
    {
        [Key]
        public string Key
        {
            get
            {
                return $"{FixtureKey}-{ClubKey}-{PlayerKey}";
            }
            set { }
        }
        public string ClubKey { get; set; }
        public string FixtureKey { get; set; }
        public string PlayerKey { get; set; }

        public bool IsStartingEleven { get; set; }
        public string PositionPlayed { get; set; }
        public decimal Rating { get; set; }
        public int Touches { get; set; }
    }
}
