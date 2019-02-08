using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class WolfAspectTrait : Trait
  {
    public const int BASE_STRENGTH_MODIFIER = 1;
    public const int BONUS_ACTION_POINTS_MODIFIER = 1;
    public const int STRENGTH_MODIFIER_WHEN_HELPING = 1;
    public override void Initialize(EventManager events)
    {
      events.AgentInit += OnAgentInit;
      events.Attack += OnAttack;
    }

    public void OnAgentInit(object sender, AgentInitEvent e)
    {
      if (e.NewAgent.HasTrait(this))
      {
        e.NewAgent.BaseStrength += BASE_STRENGTH_MODIFIER;
        Log(this, $"{e.NewAgent.Name}'s base strength is now {e.NewAgent.BaseStrength}");
        e.NewAgent.MaxBonusActionPoints += BONUS_ACTION_POINTS_MODIFIER;
        Log(this, $"{e.NewAgent.Name}'s has now {e.NewAgent.MaxBonusActionPoints} bonus action points");
      }
    }

    public void OnAttack(object sender, AttackEvent e)
    {
      bool isWolfInParty = (e.Attacker.HasTrait(this) || e.Helpers.Find(h => h.HasTrait(this)) != null);
      if (e.Defender is Creature && isWolfInParty)
      {
        int modifier = e.Helpers.Count * STRENGTH_MODIFIER_WHEN_HELPING;
        if (modifier > 0)
        {
          e.Strength += modifier;
          Log(this, $"{e.Attacker.Name}'s gained +{modifier} to strength for fighting with helpers");
        }
      }
    }
  }
}