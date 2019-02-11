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
    Other
  }
}