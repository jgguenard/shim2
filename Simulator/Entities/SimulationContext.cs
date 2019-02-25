using Microsoft.EntityFrameworkCore;
using Shim;

namespace Simulator.Entities
{
  public class SimulationContext : DbContext
  {
    public DbSet<Simulation> Simulations { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite("Data Source=simulations.db");
    }
  }
}