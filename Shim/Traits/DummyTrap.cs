using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class DummyTrap : Trait
  {
    public const int HP_LOSS = 2;

    public DummyTrap() : base()
    {
      EventManager.AuraActivated += OnAuraActivated;
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent e)
    {
      if (e.Aura.Trait == this)
      {
        e.HitPointsModifier = -1 * HP_LOSS;
        Log(this, $"{e.Agent.Name} is losing {HP_LOSS} hit points");
      }
    }
  }
}