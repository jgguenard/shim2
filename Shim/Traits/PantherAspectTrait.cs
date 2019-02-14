using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class PantherAspectTrait : Trait
  {
    public const int ACTION_POINTS_MODIFIER = 1;

    public PantherAspectTrait() : base()
    {
      EventManager.AgentInit += OnAgentInit;
    }

    public void OnAgentInit(object sender, AgentInitEvent e)
    {
      if (e.Agent.HasTrait(this))
      {
        e.MaxActionPoints += ACTION_POINTS_MODIFIER;
        Log(this, $"{e.Agent.Name}'s has now {e.Agent.MaxActionPoints} max action points");
      }
    }
  }
}