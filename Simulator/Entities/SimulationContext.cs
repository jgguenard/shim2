using Microsoft.EntityFrameworkCore;
using Shim.Library;

namespace Simulator.Entities
{
  public class SimulationContext : DbContext
  {
    public DbSet<Simulation> Simulations { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<Log> Logs { get; set; }

    public void Truncate()
    {
      Database.ExecuteSqlCommand("DELETE FROM [Logs]");
      Database.ExecuteSqlCommand("DELETE FROM [Results]");
      Database.ExecuteSqlCommand("DELETE FROM [Simulations]");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite("Data Source=simulations.db");
    }
  }
}