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

  public enum TitleType
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
}