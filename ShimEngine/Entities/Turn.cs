namespace Raido.Shim.Entities
{
  public enum TurnActionType
  {
    Undecided,
    Explore,
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
  }
}