using Raido.Shim.Events;

namespace Raido.Shim
{
  public class EventManager
  {
    public event PlayerInitEventHandler PlayerInit;
    public event AuraActivatedEventHandler AuraActivated;
    public event AttackEventHandler Attack;
    public event TargetDefeatedEventHandler TargetDefeated;
    public event PlayerAssistEventHandler PlayerAssist;

    public void OnPlayerInit(object sender, PlayerInitEvent args)
    {
      PlayerInit?.Invoke(sender, args);
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent args)
    {
      AuraActivated?.Invoke(sender, args);
    }

    public void OnAttack(object sender, AttackEvent args)
    {
      Attack?.Invoke(sender, args);
    }

    public void OnTargetDefeated(object sender, TargetDefeatedEvent args)
    {
      TargetDefeated?.Invoke(sender, args);
    }

    public void OnPlayerAssist(object sender, PlayerAssistEvent args)
    {
      PlayerAssist?.Invoke(sender, args);
    }
  }
}
