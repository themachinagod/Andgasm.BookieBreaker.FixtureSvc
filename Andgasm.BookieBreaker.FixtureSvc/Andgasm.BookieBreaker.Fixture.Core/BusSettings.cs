using System;
using System.Collections.Generic;
using System.Text;

namespace Andgasm.BookieBreaker.Fixture.Core
{
    public class BusSettings
    {
        public string ServiceBusHost { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public string NewClubSeasonAssociationTopicName { get; set; }
        public string NewClubSeasonAssociationSubscriptionName { get; set; }

        public string NewFixtureTopicName { get; set; }
        public string NewFixtureSubscriptionName { get; set; }
    }

    public class ApiSettings
    {
        public string FixturesDbApiRootKey { get; set; }
        public string ClubSeasonRegistrationsApiPath { get; set; }
    }
}
