using Shim.Events;

namespace Shim.Entities
{
  public static class EventManager
  {
    public static event TooManyPermanentItemsEventHandler TooManyPermanentItems;
    public static event AgentInitEventHandler AgentInit;
    public static event TurnActionEventHandler TurnAction;
    public static event TargetDefeatedEventHandler TargetDefeated;
    public static event AttackEventHandler Attack;
    public static event MoveEventHandler Move;
    public static event FavorGainedEventHandler FavorGained;
    public static event AgentResurrectedEventHandler AgentResurrected;
    public static event AgentHelpedEventHandler AgentHelped;
    public static event AuraActivatedEventHandler AuraActivated;
    public static event EvaluateStatEventHandler EvaluateStat;
    public static event EvaluateItemUseEventHandler EvaluateItemUse;

    public static void OnTooManyPermanentItems(object sender, TooManyPermanentItemsEvent args)
    {
      TooManyPermanentItems?.Invoke(sender, args);
    }

    public static void OnAgentInit(object sender, AgentInitEvent args)
    {
      AgentInit?.Invoke(sender, args);
    }

    public static void OnTurnAction(object sender, TurnActionEvent args)
    {
      TurnAction?.Invoke(sender, args);
    }

    public static void OnTargetDefeated(object sender, TargetDefeatedEvent args)
    {
      TargetDefeated?.Invoke(sender, args);
    }

    public static void OnAttack(object sender, AttackEvent args)
    {
      Attack?.Invoke(sender, args);
    }

    public static void OnMove(object sender, MoveEvent args)
    {
      Move?.Invoke(sender, args);
    }

    public static void OnFavorGained(object sender, FavorGainedEvent args)
    {
      FavorGained?.Invoke(sender, args);
    }

    public static void OnAgentResurrected(object sender, AgentResurrectedEvent args)
    {
      AgentResurrected?.Invoke(sender, args);
    }

    public static void OnAgentHelped(object sender, AgentHelpedEvent args)
    {
      AgentHelped?.Invoke(sender, args);
    }

    public static void OnAuraActivated(object sender, AuraActivatedEvent args)
    {
      AuraActivated?.Invoke(sender, args);
    }

    public static void OnEvaluateStat(object sender, EvaluateStatEvent args)
    {
      EvaluateStat?.Invoke(sender, args);
    }

    public static void OnEvaluateItemUse(object sender, EvaluateItemUseEvent args)
    {
      EvaluateItemUse?.Invoke(sender, args);
    }
  }
}