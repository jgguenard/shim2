using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class WolfAspectTrait : Trait
  {
    public const int BASE_STRENGTH_MODIFIER = 1;
    public const int BONUS_ACTION_POINTS_MODIFIER = 1;
    public const int STRENGTH_MODIFIER_WHEN_HELPING = 1;

    public WolfAspectTrait() : base()
    {
      EventManager.AgentInit += OnAgentInit;
      EventManager.Attack += OnAttack;
    }

    public void OnAgentInit(object sender, AgentInitEvent e)
    {
      if (e.Agent.HasTrait(this))
      {
        e.BaseStrength += BASE_STRENGTH_MODIFIER;
        e.MaxBonusActionPoints += BONUS_ACTION_POINTS_MODIFIER;
        Log(this, $"{e.Agent.Name} gained {e.Agent.BaseStrength} base STR and {e.MaxBonusActionPoints} bonus AP");
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