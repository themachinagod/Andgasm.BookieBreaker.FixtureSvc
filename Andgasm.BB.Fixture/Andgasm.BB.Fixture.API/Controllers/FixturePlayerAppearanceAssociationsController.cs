using Andgasm.BB.Fixture.Models;
using Andgasm.BB.Fixture.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andgasm.BB.Fixture.Controllers
{
    [Route("api/[controller]")]
    public class FixturePlayerAppearanceAssociationsController : Controller
    {
        #region Fields
        FixtureDb _context;
        ILogger _logger;
        #endregion

        #region Constructors
        public FixturePlayerAppearanceAssociationsController(FixtureDb context, ILogger<FixtureClubAppearanceAssociationsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        #endregion

        [HttpPost]
        public async Task<bool> StoreFixtureAssociations([FromBody]List<PlayerAppearanceModel> model)
        {
            try
            {
                // create player dto from model and save if key is new
                bool dochange = false;
                foreach (var p in model)
                {
                    if (!_context.PlayerFixtureAppearances.Any(x => x.Key == p.Key))
                    {
                        dochange = true;
                        var player = new PlayerFixtureAppearance()
                        {
                            Key = p.Key,
                            ClubCode = p.ClubCode,
                            ClubKey = p.ClubKey,
                            FixtureCode = p.FixtureCode,
                            FixtureKey = p.FixtureKey,
                            PlayerCode = p.PlayerCode,
                            PlayerKey = p.PlayerKey,
                            IsStartingEleven = p.IsStartingEleven,
                            PositionPlayed = p.PositionPlayed,
                            Rating = p.Rating,
                            Touches = p.Touches
                        };
                        _context.PlayerFixtureAppearances.Add(player);
                    }
                }
                if (dochange) await _context.SaveChangesAsync();
                return dochange;
            }
            catch (DbUpdateException pkex)
            {
                // TODO: we are seeing this occaisionally due to async processing from multiple instances
                //       its ok to swallow as we dont support data updates and if the key exists there is no need for dupe store
                Console.WriteLine($"A primary key violation occured while saving player data: { pkex.Message }");
                return false;
            }
        }
    }
}
