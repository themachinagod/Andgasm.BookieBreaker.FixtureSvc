using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andgasm.BookieBreaker.Database.Core.DTOs
{
    public class ClubFixtureAppearance
    {
        [Key]
        public string Key { get; set; }
        public string ClubKey { get; set; }
        public string FixtureKey { get; set; }
        public string SeasonKey { get; set; }

        public bool IsHomeTeam { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
    }
}
