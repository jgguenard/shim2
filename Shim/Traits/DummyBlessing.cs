using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class DummyBlessing : Trait
  {
    public override void Initialize(EventManager events)
    {
      events.AuraActivated += OnAuraActivated;
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent e)
    {
      if (e.Aura.Trait == this)
      {
        e.ActionPointsModifier = (e.Agent.AvailableActionPoints < e.Agent.MaxActionPoints) ? 1 : 0;
      }
    }
  }
}