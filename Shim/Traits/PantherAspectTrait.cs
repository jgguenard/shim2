using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class PantherAspectTrait : Trait
  {
    public const int ACTION_POINTS_MODIFIER = 1;

    public override void Initialize(EventManager events)
    {
      events.AgentInit += OnAgentInit;
    }

    public void OnAgentInit(object sender, AgentInitEvent e)
    {
      if (e.NewAgent.HasTrait(this))
      {
        e.NewAgent.MaxActionPoints += ACTION_POINTS_MODIFIER;
        Log(this, $"{e.NewAgent.Name}'s has now {e.NewAgent.MaxActionPoints} max action points");
      }
    }
  }
}