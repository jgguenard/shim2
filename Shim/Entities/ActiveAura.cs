using System.Collections.Generic;

namespace Shim.Entities
{
  public class ActiveAura
  {
    public Agent Activator { get; set; }
    public Aura Aura { get; set; }
    public int ActivationRound { get; set; }
    public int ActivationTurn { get; set; }
    public List<Agent> Targets { get; set; }

    public ActiveAura()
    {
      Targets = new List<Agent>();
    }
  }
}