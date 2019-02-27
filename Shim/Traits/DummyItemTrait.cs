using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class DummyItemTrait : Trait
  {
    public const int HP_GAIN = 2;

    public DummyItemTrait() : base()
    {
      EventManager.AuraActivated += OnAuraActivated;
      EventManager.EvaluateItemUse += OnEvaluateItemUse;
    }

    public void OnEvaluateItemUse(object sender, EvaluateItemUseEvent e)
    {
      if (e.Item.Aura != null && e.Item.Aura.Trait == this)
      {
        int missingHitPoints = (e.Source.MaxHitPoints - e.Source.AvailableHitPoints);
        if (missingHitPoints >= HP_GAIN)
        {
          e.Target = e.Source;
          e.Score += 1;
        }
      }
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent e)
    {
      if (e.Aura.Trait == this)
      {
        e.HitPointsModifier = HP_GAIN;
        Log(this, $"{e.Agent.Name} is recovering {HP_GAIN} hit points");
      }
    }
  }
}