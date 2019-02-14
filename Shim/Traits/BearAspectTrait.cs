using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class BearAspectTrait : Trait
  {
    public const int BASE_DEFENSE_MODIFIER = 2;
    public const int FIGHTING_ALONE_STRENGTH_MODIFIER = 2;

    public BearAspectTrait() : base()
    {
      EventManager.AgentInit += OnAgentInit;
      EventManager.Attack += OnAttack;
    }

    public void OnAgentInit(object sender, AgentInitEvent e)
    {
      if (e.Agent.HasTrait(this))
      {
        e.BaseDefense += BASE_DEFENSE_MODIFIER;
        Log(this, $"{e.Agent.Name}'s base defense was increased by {e.Agent.BaseDefense}");
      }
    }

    public void OnAttack(object sender, AttackEvent e)
    {
      if (e.Attacker.HasTrait(this) && e.Helpers.Count == 0)
      {
        e.Strength += FIGHTING_ALONE_STRENGTH_MODIFIER;
        Log(this, $"{e.Attacker.Name}'s gained +{FIGHTING_ALONE_STRENGTH_MODIFIER} to strength for fighting alone");
      }
    }
  }
}