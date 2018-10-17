using Andgasm.BookieBreaker.Database.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System;

namespace Andgasm.BookieBreaker.Data
{
    public class FixtureDb : DbContext
    {
        public FixtureDb() : base()
        {
        }

        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<ClubFixtureAppearance> ClubFixtureAppearances { get; set; }

        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
    }
}