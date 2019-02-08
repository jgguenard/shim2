using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class BasicAgentTrait : Trait
  {
    private BoardManager _board;
    public void Initialize(EventManager events, BoardManager board)
    {
      events.TurnAction += OnTurnAction;
      _board = board;
    }

    public void OnTurnAction(object sender, TurnActionEvent e)
    {
      var valuableDestinations = _board.GetReachableValuableTiles(e.Source.Position, e.Source.AvailableActionPoints);
      /*
       Own Turn APL
       - move towards a healer if dead
       - use recovery item if low HP
       - move to a healer if low HP
       - move closer to another player if we need to attack or use an item on him
       - use item to restraint another player from winning during his next turn
       - attack a weaker player if we can survive the ripost
       - move to a creature if we think we can defeat it or get help
       - move to a chest if we don't have enough items
       - move to a discovery
       - move to a blessing if we don't already have one
       - move to a chest if we have enough items already
       - move to an empty tile
       - move to a trap
       - stop
      */

      // move towards a healer if dead
      if (e.Source.IsDead)
      {
        // var nearestHealerTile = _board.Nearest();
        e.Type = TurnActionType.Move;
        e.Tile = null; // todo
      }

      e.Type = TurnActionType.Stop;
    }
  }
}