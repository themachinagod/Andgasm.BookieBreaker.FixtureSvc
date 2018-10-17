using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andgasm.BookieBreaker.Data;
using Andgasm.BookieBreaker.Database.Core.DTOs;
using Andgasm.BookieBreaker.Models;
using Andgasm.BookieBreaker.ServiceBus;
using Andgasm.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Andgasm.BookieBreaker.SeasonParticipants.API.Controllers
{
    [Route("api/[controller]")]
    public class FixtureClubAppearanceAssociationsController : Controller
    {
        #region Fields
        IBusClient _fixturebus;
        FixtureDb _context;
        ILogger _logger;
        #endregion

        #region Constructors
        public FixtureClubAppearanceAssociationsController(FixtureDb context, IBusClient fixturebus, ILogger<FixtureClubAppearanceAssociationsController> logger)
        {
            _context = context;
            _fixturebus = fixturebus;
            _logger = logger;
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> StoreFixtureAssociations([FromBody]List<FixtureParticipantsModel> model)
        {
            try
            {
                // create player dto from model and save if key is new
                bool dochange = false;
                foreach (var p in model)
                {
                    if (!_context.Fixtures.Any(x => x.Key == p.FixtureCode))
                    {
                        dochange = true;
                        var fixture = new Fixture()
                        {
                            Key = p.FixtureCode,
                            SeasonKey = p.SeasonCode,
                            FixtureCode = p.FixtureCode,
                            HomeClubCode = p.HomeClubCode,
                            HomeClubKey = p.HomeClubCode,
                            AwayClubCode = p.AwayClubCode,
                            AwayClubKey = p.AwayClubCode,
                            KickOffTime = p.KickOffTime,
                            FinalScore = p.FinalScore,
                        };
                        _context.Fixtures.Add(fixture);
                        //await _fixturebus.SendEvent(EventFactory.BuildNewFixtureEvent(p.CountryCode, p.HomeClubCode, p.AwayClubCode, p.SeasonCode, p.FixtureCode, p.RegionCode, p.TournamentCode));
                    }
                    if (!_context.ClubFixtureAppearances.Any(x => x.FixtureKey == p.FixtureCode &&
                                                                  x.ClubKey == p.HomeClubCode &&
                                                                  x.IsHomeTeam))
                    {
                        dochange = true;
                        var homeassociation = new ClubFixtureAppearance()
                        {
                            Key = p.HomeAppearance.Key,
                            ClubKey = p.HomeAppearance.ClubKey,
                            SeasonKey = p.HomeAppearance.SeasonKey,
                            FixtureKey = p.HomeAppearance.FixtureKey,
                            GoalsScored = p.HomeAppearance.GoalsScored,
                            GoalsConceded = p.HomeAppearance.GoalsConceded,
                            IsHomeTeam = p.HomeAppearance.IsHomeTeam
                        };
                        _context.ClubFixtureAppearances.Add(homeassociation);
                    }
                    if (!_context.ClubFixtureAppearances.Any(x => x.FixtureKey == p.FixtureCode &&
                                                                  x.ClubKey == p.AwayClubCode &&
                                                                  !x.IsHomeTeam))
                    {
                        dochange = true;
                        var awayassociation = new ClubFixtureAppearance()
                        {
                            Key = p.AwayAppearance.Key,
                            ClubKey = p.AwayAppearance.ClubKey,
                            SeasonKey = p.AwayAppearance.SeasonKey,
                            FixtureKey = p.AwayAppearance.FixtureKey,
                            GoalsScored = p.AwayAppearance.GoalsScored,
                            GoalsConceded = p.AwayAppearance.GoalsConceded,
                            IsHomeTeam = p.AwayAppearance.IsHomeTeam
                        };
                        _context.ClubFixtureAppearances.Add(awayassociation);
                    }
                }
                if (dochange) await _context.SaveChangesAsync();
                return dochange;
            }
            catch (DbUpdateException pkex)
            {
                // TODO: we are seeing this occaisionally due to async processing from multiple instances
                //       its ok to swallow as we dont support data updates and if the key exists there is no need for dupe store

                return Conflict($"A primary key violation occured while saving player data: { pkex.Message }");
            }
        }
    }
}
