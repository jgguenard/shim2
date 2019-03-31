using Raido.Shim.Events;

namespace Raido.Shim
{
  public class EventManager
  {
    public event PlayerInitEventHandler PlayerInit;
    public event AuraActivatedEventHandler AuraActivated;

    public void OnPlayerInit(object sender, PlayerInitEvent args)
    {
      PlayerInit?.Invoke(sender, args);
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent args)
    {
      AuraActivated?.Invoke(sender, args);
    }
  }
}
