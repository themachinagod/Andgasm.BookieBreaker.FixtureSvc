using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Andgasm.BB.Fixture.Models
{
    public class Fixture
    {
        [Key]
        public string Key { get; set; }
        public DateTime KickOffTime { get; set; }
        public string SeasonKey { get; set; }
        public string HomeClubKey { get; set; }
        public string AwayClubKey { get; set; }
        public string FinalScore { get; set; }
    }
}
