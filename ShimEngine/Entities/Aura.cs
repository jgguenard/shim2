using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public enum AuraScope
  {
    Self,
    Others,
    All
  }
  public enum AuraExpiration
  {
    Now,
    Never,
    NextUse,
    EndOfTurn,
    EndOfNextTurn,
    EndOfRound
  }
  public enum AuraSource
  {
    GameEvent,
    Blessing,
    Trial,
    Equipment,
    Potion,
    Other
  }
  public class Aura
  {
    public AuraSource Source { get; set; }
    public AuraScope Scope { get; set; }
    public AuraExpiration Expiration { get; set; }
    public Trait Trait { get; set; }
    public int ActivationCost { get; set; }
    public Aura()
    {
      ActivationCost = 0;
      Source = AuraSource.Other;
    }
  }
  public class ActiveAura
  {
    public Character Activator { get; set; }
    public Aura Aura { get; set; }
    public int ActivationRound { get; set; }
    public int ActivationTurn { get; set; }
    public List<Character> Targets { get; set; }
    public ActiveAura()
    {
      Targets = new List<Character>();
    }
  }
}
