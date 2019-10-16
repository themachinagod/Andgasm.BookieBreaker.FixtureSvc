using Andgasm.BB.Harvest;
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

namespace Andgasm.BB.Fixture.Core
{
    public class FixturePlayerHarvester : DataHarvest
    {
        #region Fields
        ILogger<FixtureHarvester> _logger;
        ApiSettings _settings;

        string _fixturesapiroot;
        string _registrationsApiPath;
        #endregion

        #region Properties
        public string FixtureKey { get; set; }
        public string TournamentKey { get; set; }
        public string RegionKey { get; set; }
        #endregion

        #region Contructors
        public FixturePlayerHarvester(ApiSettings settings, ILogger<FixtureHarvester> logger, HarvestRequestManager requestmanager)
        {
            _logger = logger;
            _requestmanager = requestmanager;

            _fixturesapiroot = settings.FixturesDbApiRootKey;
            _registrationsApiPath = settings.FixturePlayerAppearancesApiPath;
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
                JObject jsondata = null;
                HtmlDocument responsedoc = await ExecuteRequest();
                if (responsedoc != null)
                {
                    var playerapps = new List<ExpandoObject>();
                    jsondata = CleanJsonData(responsedoc);
                    playerapps.AddRange(CreatePlayerAppearances(jsondata, true));
                    playerapps.AddRange(CreatePlayerAppearances(jsondata, false));
                    if (playerapps.Count > 0)
                    {
                        await HttpRequestFactory.Post(playerapps, _fixturesapiroot, _registrationsApiPath);
                        _logger.LogDebug(string.Format("Stored & comitted fixture player appearances for fixture '{0}' in data store.", FixtureKey));
                    }
                }
                else
                {
                    _logger.LogDebug(string.Format("Failed to store & commit player appearances for fixture '{0}'", FixtureKey));
                }
                HarvestHelper.FinaliseTimer(_timer);
            }
        }
        #endregion

        #region Entity Creation Helpers
        private string CreateUrl()
        {
            return string.Format(WhoScoredConstants.MatchesUrl, FixtureKey);
        }

        private string CreateRefererUrl()
        {
            // TODO: constant!!
            return ($"https://www.whoscored.com/Regions/{RegionKey}/Tournaments/{TournamentKey}/");
        }

        private async Task<HtmlDocument> ExecuteRequest()
        {
            var url = CreateUrl();
            var refer = CreateRefererUrl();
            var ctx = HarvestHelper.ConstructRequestContext(null, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8", refer, CookieString, "en-GB,en;q=0.9,en-US;q=0.8,th;q=0.7", false, true, true);
            var p = await _requestmanager.MakeRequest(url, ctx);
            CookieString = ctx.Cookies["Cookie"];
            return p;
        }

        private JObject CleanJsonData(HtmlDocument doc)
        {
            var rawdata = doc.DocumentNode.InnerHtml;
            var startIndex = rawdata.IndexOf("var matchCentreData = ");
            var endIndex = rawdata.IndexOf("var matchCentreEventTypeJson =");
            return JObject.Parse(rawdata.Substring(startIndex + 22, (endIndex - (startIndex + 22))).Replace(";", ""));
        }

        private List<ExpandoObject> CreatePlayerAppearances(JObject jsondata, bool ishome)
        {
            var homeaway = ishome ? "home" : "away";
            var players = new List<ExpandoObject>();
            JArray playersdata = (JArray)jsondata[homeaway]["players"];
            foreach (var pl in playersdata)
            {
                var clubcode = jsondata[homeaway]["teamId"].ToString();
                var clubname = jsondata[homeaway]["name"].ToString();
                var ns = CreatePlayerAppearance(clubname, clubcode, pl, ishome);
                players.Add(ns);
            }
            return players;
        }

        private ExpandoObject CreatePlayerAppearance(string clubname, string clubcode, JToken playerdata, bool playsforhometeam)
        {
            dynamic app = new ExpandoObject();
            app.ClubKey = clubcode;
            app.FixtureKey = FixtureKey;
            app.PlayerKey = playerdata["playerId"].ToString();
            app.PositionPlayed = playerdata["position"].ToString();
            app.Rating = playerdata["stats"]["ratings"] != null ? (decimal)playerdata["stats"]["ratings"].Values<JProperty>().Values<int>().Average() : 0M;
            app.Touches = playerdata["stats"]["touches"] != null ? playerdata["stats"]["touches"].Values<JProperty>().Values<int>().Count() : 0;
            app.IsStartingEleven = playerdata["isFirstEleven"] == null ? false : Convert.ToBoolean(playerdata["isFirstEleven"].ToString());
            return app;
        }
        #endregion
    }
}
