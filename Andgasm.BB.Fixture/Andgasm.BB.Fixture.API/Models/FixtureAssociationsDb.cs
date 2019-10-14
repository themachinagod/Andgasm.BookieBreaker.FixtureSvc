using Microsoft.EntityFrameworkCore;
using System;

namespace Andgasm.BB.Fixture.Models
{
    public class FixtureDb : DbContext
    {
        public FixtureDb(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<ClubFixtureAppearance> ClubFixtureAppearances { get; set; }
        public DbSet<PlayerFixtureAppearance> PlayerFixtureAppearances { get; set; }

        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
    }
}