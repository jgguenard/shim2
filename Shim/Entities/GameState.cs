using System.Collections.Generic;

namespace Shim.Entities
{
  public class GameState
  {
    public List<Agent> Agents { get; set; }
    public List<ActiveAura> ActiveAuras { get; set; }
    public int Round;
    public int Turn;
    public Agent TurnAgent {
      get {
        return Agents[Turn - 1];
      }
    }

    public GameState()
    {
      ActiveAuras = new List<ActiveAura>();
      Agents = new List<Agent>();
      Turn = 1;
      Round = 1;
    }

    public List<ActiveAura> GetAgentActiveAurasByType(Agent agent, AuraType type)
    {
      return ActiveAuras.FindAll(a => a.Aura.Type == type && a.Targets.Contains(agent));
    }
  }
}
