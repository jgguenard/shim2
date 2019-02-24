using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class PantherAspectTrait : Trait
  {
    public const int ACTION_POINTS_MODIFIER = 1;
    public const int STRENGTH_MODIFIER = 1;

    public PantherAspectTrait() : base()
    {
      EventManager.AgentInit += OnAgentInit;
    }

    public void OnAgentInit(object sender, AgentInitEvent e)
    {
      if (e.Agent.HasTrait(this))
      {
        e.MaxActionPoints += ACTION_POINTS_MODIFIER;
        e.BaseStrength += STRENGTH_MODIFIER;
        Log(this, $"{e.Agent.Name}'s gains +{STRENGTH_MODIFIER} base strength");
        Log(this, $"{e.Agent.Name}'s gains +{ACTION_POINTS_MODIFIER} max action points");
      }
    }
  }
}