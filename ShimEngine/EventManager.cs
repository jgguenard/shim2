using Raido.Shim.Events;

namespace Raido.Shim
{
  public class EventManager
  {
    public event PlayerInitEventHandler PlayerInit;

    public void OnPlayerInit(object sender, PlayerInitEvent args)
    {
      PlayerInit?.Invoke(sender, args);
    }
  }
}
