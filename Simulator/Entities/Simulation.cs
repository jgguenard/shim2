using System.Collections.Generic;

namespace Simulator.Entities
{
  public class Simulation
  {
    public string SimulationId { get; set; }
    public int TotalRounds { get; set; }
    public int TotalAgents { get; set; }
    public string Parameters { get; set; }
    public virtual ICollection<Result> Results { get; set; }

    public Simulation()
    {
      Results = new List<Result>();
    }
  }
}