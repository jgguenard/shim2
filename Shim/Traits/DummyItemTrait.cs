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
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent e)
    {
      if (e.Aura.Trait == this)
      {
        e.HitPointsModifier = HP_GAIN;
        Log(this, $"{e.Agent.Name} is losing {HP_GAIN} hit points");
      }
    }
  }
}