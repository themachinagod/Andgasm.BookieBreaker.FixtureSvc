using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andgasm.BB.Fixture.Models
{
    public class ClubFixtureAppearance
    {
        [Key]
        public string Key
        {
            get
            {
                return $"{SeasonKey}-{FixtureKey}-{ClubKey}";
            }
            set { }
        }
        public string ClubKey { get; set; }
        public string FixtureKey { get; set; }
        public string SeasonKey { get; set; }

        public bool IsHomeTeam { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
    }
}
