using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public enum TurnActionType
  {
    Undecided,
    Explore,
    Visit,
    Duel,
    UseSkill,
    UsePotion,
    Stop
  }

  public class TurnState
  {
    public int Round { get; set; }
    public int Turn { get; set; }
    public Player Player { get; set; }
    public bool CanExplore { get; set; }
    public List<Entity> DiscardedChoices { get; set; }

    public TurnState()
    {
      DiscardedChoices = new List<Entity>();
    }
  }
}