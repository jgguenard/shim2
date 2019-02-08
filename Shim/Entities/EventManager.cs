using Shim.Events;

namespace Shim.Entities
{
  public class EventManager
  {
    public event TooManyPermanentItemsEventHandler TooManyPermanentItems;
    public event AgentInitEventHandler AgentInit;
    public event TurnActionEventHandler TurnAction;
    public event TargetDefeatedEventHandler TargetDefeated;
    public event AttackEventHandler Attack;
    public event MoveEventHandler Move;
    public event FavorGainedEventHandler FavorGained;
    public event AgentResurrectedEventHandler AgentResurrected;
    public event AgentHelpedEventHandler AgentHelped;

    public void OnTooManyPermanentItems(object sender, TooManyPermanentItemsEvent args)
    {
      TooManyPermanentItems?.Invoke(this, args);
    }

    public void OnAgentInit(object sender, AgentInitEvent args)
    {
      AgentInit?.Invoke(this, args);
    }

    public void OnTurnAction(object sender, TurnActionEvent args)
    {
      TurnAction?.Invoke(this, args);
    }

    public void OnTargetDefeated(object sender, TargetDefeatedEvent args)
    {
      TargetDefeated?.Invoke(this, args);
    }

    public void OnAttack(object sender, AttackEvent args)
    {
      Attack?.Invoke(this, args);
    }

    public void OnMove(object sender, MoveEvent args)
    {
      Move?.Invoke(this, args);
    }

    public void OnFavorGained(object sender, FavorGainedEvent args)
    {
      FavorGained?.Invoke(this, args);
    }

    public void OnAgentResurrected(object sender, AgentResurrectedEvent args)
    {
      AgentResurrected?.Invoke(this, args);
    }

    public void OnAgentHelped(object sender, AgentHelpedEvent args)
    {
      AgentHelped?.Invoke(this, args);
    }
  }
}