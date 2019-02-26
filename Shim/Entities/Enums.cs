namespace Shim.Entities
{
  public enum ScopeType
  {
    Self,
    Others,
    All
  }

  public enum ExpirationType
  {
    Now,
    Never,
    NextUse,
    EndOfTurn,
    EndOfNextTurn,
    EndOfRound
  }

  public enum TileType
  {
    Empty,
    Creature,
    Trap,
    Blessing,
    Healer,
    Discovery,
    Gate,
    Item 
  }

  public enum TurnActionType
  {
    Undecided,
    AttackAgent,
    Move,
    UseItem,
    Stop
  }

  public enum AuraType
  {
    GameEvent,
    Blessing,
    Trap,
    Item,
    Other
  }

  public enum StatType
  {
    Strength,
    Defense
  }

  public enum LogType
  {
    Message,
    ItemGain,
    ItemLoss,
    TraitAdded,
    TraitRemoved
  }
}