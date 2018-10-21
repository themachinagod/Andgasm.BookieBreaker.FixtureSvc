using Andgasm.BookieBreaker.Harvest;
using Andgasm.Http;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Fixture.Core
{
    public class FixtureHarvester : DataHarvest
    {
        #region Fields
        ILogger<FixtureHarvester> _logger;
        ApiSettings _settings;

        string _fixturesapiroot;
        string _registrationsApiPath;
        #endregion

        #region Properties
        public string StageCode { get; set; }
        public string SeasonCode { get; set; }
        public string TournamentCode { get; set; }
        public string RegionCode { get; set; }
        public string CountryCode { get; set; }
        public DateTime RequestPeriod { get; set; }
        #endregion

        #region Contructors
        public FixtureHarvester(ApiSettings settings, ILogger<FixtureHarvester> logger, HarvestRequestManager requestmanager)
        {
            _logger = logger;
            _requestmanager = requestmanager;

            _fixturesapiroot = settings.FixturesDbApiRootKey;
            _registrationsApiPath = settings.FixtureClubAppearancesApiPath;
            _settings = settings;
        }

        #endregion

        #region Execution Operations
        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return true;
        }

        public async override Task Execute()
        {
            if (CanExecute())
            {
                _timer.Start();
                var lastmodekey = await DetermineLastModeKey();
                var pdate = RequestPeriod;
                HtmlDocument responsedoc = await ExecuteRequest(pdate.Year, GetIso8601WeekOfYear(pdate), lastmodekey);
                if (responsedoc != null)
                {
                    var fixtures = new List<ExpandoObject>();
                    foreach (var fx in ParseFixturesFromResponse(responsedoc))
                    {
                        var fixture = CreateFixture(fx);
                        fixtures.Add(fixture);
                    }
                    if (fixtures.Count > 0)
                    {
                        await HttpRequestFactory.Post(fixtures, _fixturesapiroot, _registrationsApiPath);
                        _logger.LogDebug(string.Format("Stored season fixtures to database for season and period '{0}' - '{1}", SeasonCode, pdate.ToShortDateString()));
                    } else { _logger.LogDebug(string.Format("No seasons identified for storage for season and period '{0}' - '{1}", SeasonCode, pdate.ToShortDateString())); }
                }
                else
                {
                    _logger.LogDebug(string.Format("Failed to store & commit fixtures for period '{0}' in data store.", pdate.ToShortDateString()));
                }
            };
            HarvestHelper.FinaliseTimer(_timer);
        }
        #endregion

        #region Entity Creation Helpers
        private string CreateRequestUrl(int year, int week)
        {
            return string.Format(WhoScoredConstants.TournamentsStatisticsFeedUrl, StageCode, year, week.ToString());
        }

        private string CreateRefererUrl()
        {
            return string.Format(WhoScoredConstants.FixturesUrl, RegionCode, TournamentCode, SeasonCode, StageCode);
        }

        private async Task<string> DetermineLastModeKey()
        {
            var referer = CreateRefererUrl();
            var ctx = HarvestHelper.ConstructRequestContext(null, "text/html,application/xhtml+xml,image/jxr,*/*", null,
                                                            CookieString,
                                                            null, false, false, false);
            var p = await _requestmanager.MakeRequest(referer, ctx);
            if (p != null)
            {
                return GetLastModeKey(p.DocumentNode.InnerText);
            }
            return null;
        }

        private async Task<HtmlDocument> ExecuteRequest(int year, int week, string lastmodekey)
        {
            var url = CreateRequestUrl(year, week);
            var referer = CreateRefererUrl();
            var ctx = HarvestHelper.ConstructRequestContext(lastmodekey, "en -GB,en;q=0.9,en-US;q=0.8,th;q=0.7", referer,
                                                            CookieString,
                                                            null, true, false, false);
            var p = await _requestmanager.MakeRequest(url, ctx);
            CookieString = ctx.Cookies["Cookie"];
            return p;
        }

        private JArray ParseFixturesFromResponse(HtmlDocument response)
        {
            var rawdata = response.DocumentNode.InnerHtml;
            var jsondata = JsonConvert.DeserializeObject<JArray>(rawdata);
            return jsondata;
        }

        private ExpandoObject CreateFixture(JToken fixturedata)
        {
            dynamic fixture = new ExpandoObject();
            fixture.KickOffTime = DateTime.Parse(string.Format("{0} {1}", fixturedata[2].ToString(), fixturedata[3].ToString()));
            fixture.FinalScore = fixturedata[10].ToString();
            fixture.HomeClubCode = fixturedata[4].ToString();
            fixture.AwayClubCode = fixturedata[7].ToString();
            fixture.SeasonCode = SeasonCode;
            fixture.CountryCode = CountryCode;
            fixture.FixtureCode = fixturedata[0].ToString();
            fixture.TournamentCode = TournamentCode;
            fixture.RegionCode = RegionCode;
            fixture.HomeGoalsScored = ParseGoalsFromScore(true, fixture.FinalScore);
            fixture.HomeGoalsConceded = ParseGoalsFromScore(false, fixture.FinalScore);
            fixture.AwayGoalsScored = ParseGoalsFromScore(false, fixture.FinalScore);
            fixture.AwayGoalsConceded = ParseGoalsFromScore(true, fixture.FinalScore);
            return fixture;
        }

        private int ParseGoalsFromScore(bool hometeam, string score)
        {
            var index = hometeam ? 0 : 1;
            var spl = score.Split(':');
            if (spl.Count() == 2)
            {
                return int.Parse(score.Split(':')[index].Replace(" ", ""));
            }
            return 0;
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        #endregion
    }
}
