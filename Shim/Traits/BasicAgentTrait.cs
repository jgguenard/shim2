using Shim.Entities;
using Shim.Events;
using System.Collections.Generic;

namespace Shim.Traits
{
  public class BasicAgentTrait : Trait
  {
    public const int LOW_HIT_POINTS_THRESHOLD = 4;

    public const int SCORE_CREATURE_SOLOABLE = 70;
    public const int SCORE_HEALER_WHEN_LOW_HP = 65;
    public const int SCORE_ITEM_WHEN_NOT_FULL = 60;
    public const int SCORE_BLESSING_WHEN_NOT_FULL = 55;
    public const int SCORE_CREATURE_NEED_HELP = 50;
    public const int SCORE_DISCOVERY_BASE = 45; // takes reward into account: 1 favor = 48, 2 favor = 51, 3 favor = 54
    public const int SCORE_HEALER_WHEN_NOT_FULL = 40;
    public const int SCORE_ITEM_WHEN_FULL = 35;
    public const int SCORE_GATE_BASE = 30;
    public const int SCORE_BLESSING_WHEN_FULL = 25;
    public const int SCORE_HEALER_WHEN_FULL = 20;
    public const int SCORE_TRAP = 15;
    public const int SCORE_CREATURE_LOW_HIT_POINTS = 10;

    private BoardManager _board;
    private readonly Dictionary<Agent, List<Tile>> _trips = new Dictionary<Agent, List<Tile>>();

    private bool ResumeTrip(TurnActionEvent e)
    {
      if (!_trips.ContainsKey(e.Source) || _trips[e.Source].Count == 0)
      {
        return false;
      }
      e.Type = TurnActionType.Move;
      e.Tile = _trips[e.Source][0];
      _trips[e.Source].RemoveAt(0);
      return true;
    }

    public void CreateTrip(Agent agent, List<Tile> path)
    {
      _trips[agent] = path;
    }

    public void Initialize(EventManager events, BoardManager board)
    {
      events.TurnAction += OnTurnAction;
      _board = board;
    }

    public void OnTurnAction(object sender, TurnActionEvent e)
    {
      /*
       Own Turn APL
       x resume current trip
       x move towards a healer if dead
       - use recovery item if low HP
       - attack a reachable weaker player if we can survive the ripost
       x move to the most valuable point of interest (see CONSTANTS for priority)
       x stop
      */

      if (e.Source.AvailableActionPoints > 0)
      {
        // resume current trip
        if (ResumeTrip(e))
        {
          return;
        }

        // move towards a healer if dead
        if (e.Source.IsDead)
        {
          CreateTrip(e.Source, _board.ShortestPathToTileType(e.Source.Position, TileType.Healer));
          ResumeTrip(e);
          return;
        }

        // move to the most valuable point of interest
        var possibleTrips = _board.ReachablePointsOfInterest(e.Source.Position, e.Source.AvailableActionPoints, e.Source.PreviousPosition);
        List<Tile> mostValuableTrip = null;
        int bestScore = 0;
        foreach (var trip in possibleTrips)
        {
          var distance = trip.Count;
          var destination = trip[distance - 1];
          var score = 0;
          switch (destination.Type)
          {
            case TileType.Blessing:
              if (e.GameState.GetAgentActiveAurasByType(e.Source, AuraType.Blessing).Count == e.Parameters.MaxActiveBlessingPerAgent)
              {
                score = SCORE_BLESSING_WHEN_FULL;
              }
              else
              {
                score = SCORE_BLESSING_WHEN_NOT_FULL;
              }
              break;
            case TileType.Creature:
              score = SCORE_CREATURE_NEED_HELP;
              break;
            case TileType.Discovery:
              score = SCORE_DISCOVERY_BASE + (destination.IntValue * 2);
              break;
            case TileType.Gate:
              score = SCORE_GATE_BASE;
              break;
            case TileType.Healer:
              if (e.Source.AvailableHitPoints < e.Source.MaxHitPoints)
              {
                if (e.Source.AvailableActionPoints <= LOW_HIT_POINTS_THRESHOLD)
                {
                  score = SCORE_HEALER_WHEN_LOW_HP;
                }
                else
                {
                  score = SCORE_HEALER_WHEN_NOT_FULL;
                }
              }
              else
              {
                score = SCORE_HEALER_WHEN_FULL;
              }
              break;
            case TileType.Item:
              if (e.Source.PermanentItemCount < e.Parameters.MaxPermanentItemsPerAgent)
              {
                score = SCORE_ITEM_WHEN_NOT_FULL;
              }
              else
              {
                score = SCORE_ITEM_WHEN_FULL;
              }
              break;
            case TileType.Trap:
              score = SCORE_TRAP;
              break;
          }
          if (score > bestScore)
          {
            mostValuableTrip = trip;
            bestScore = score;
          }
        }
        if (mostValuableTrip != null)
        {
          CreateTrip(e.Source, mostValuableTrip);
          ResumeTrip(e);
          return;
        }
      }

      // stop
      e.Type = TurnActionType.Stop;
    }
  }
}