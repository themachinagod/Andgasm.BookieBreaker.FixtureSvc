using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andgasm.BB.Fixture.Models;
using Andgasm.BB.Fixture.Resources;
using Andgasm.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Andgasm.BB.Fixture.Controllers
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

        [HttpGet(Name = "GetAllFixtures")]
        public async Task<IActionResult> GetAll()
        {
            var d = await _context.Fixtures.Select(x => new FixtureParticipantsModel()
            {
                FixtureKey = x.Key,
                AwayClubKey = x.AwayClubKey,
                FinalScore = x.FinalScore,
                HomeClubKey = x.HomeClubKey,
                KickOffTime = x.KickOffTime,
                SeasonKey = x.SeasonKey,
            }).ToListAsync();
            return Ok(d);
        }

        [HttpPost(Name = "CreateClubSeasonRegistration")]
        public async Task<IActionResult> CreateBatch([FromBody]List<FixtureParticipantsModel> model)
        {
            try
            {
                // create player dto from model and save if key is new
                bool dochange = false;
                foreach (var p in model)
                {
                    if (!_context.Fixtures.Any(x => x.Key == p.FixtureKey))
                    {
                        dochange = true;
                        var fixture = new Models.Fixture()
                        {
                            Key = p.FixtureKey,
                            SeasonKey = p.SeasonKey,
                            HomeClubKey = p.HomeClubKey,
                            AwayClubKey = p.AwayClubKey,
                            KickOffTime = p.KickOffTime,
                            FinalScore = p.FinalScore,
                        };
                        _context.Fixtures.Add(fixture);
                        await _fixturebus.SendEvent(BuildNewFixtureEvent(p.FixtureKey, p.RegionKey, p.TournamentKey));
                    }
                    if (!_context.ClubFixtureAppearances.Any(x => x.FixtureKey == p.FixtureKey &&
                                                                  x.ClubKey == p.HomeClubKey &&
                                                                  x.IsHomeTeam))
                    {
                        dochange = true;
                        var homeassociation = new ClubFixtureAppearance()
                        {
                            ClubKey = p.HomeClubKey,
                            SeasonKey = p.SeasonKey,
                            FixtureKey = p.FixtureKey,
                            GoalsScored = p.HomeGoalsScored,
                            GoalsConceded = p.HomeGoalsConceded,
                            IsHomeTeam = true
                        };
                        _context.ClubFixtureAppearances.Add(homeassociation);
                    }
                    if (!_context.ClubFixtureAppearances.Any(x => x.FixtureKey == p.FixtureKey &&
                                                                  x.ClubKey == p.AwayClubKey &&
                                                                  !x.IsHomeTeam))
                    {
                        dochange = true;
                        var awayassociation = new ClubFixtureAppearance()
                        {
                            ClubKey = p.AwayClubKey,
                            SeasonKey = p.SeasonKey,
                            FixtureKey = p.FixtureKey,
                            GoalsScored = p.AwayGoalsScored,
                            GoalsConceded = p.AwayGoalsConceded,
                            IsHomeTeam = false
                        };
                        _context.ClubFixtureAppearances.Add(awayassociation);
                    }
                }
                if (dochange) await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException pkex)
            {
                // TODO: we are seeing this occaisionally due to async processing from multiple instances
                //       its ok to swallow as we dont support data updates and if the key exists there is no need for dupe store

                return Conflict($"A primary key violation occured while saving player data: { pkex.Message }");
            }
        }

        static BusEventBase BuildNewFixtureEvent(string fixturecode, string regioncode, string tournycode)
        {
            dynamic jsonpayload = new ExpandoObject();
            jsonpayload.FixtureKey = fixturecode;
            jsonpayload.RegionKey = regioncode;
            jsonpayload.TournamentKey = tournycode;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
