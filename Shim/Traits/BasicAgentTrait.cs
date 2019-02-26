using Shim.Entities;
using Shim.Events;
using System.Collections.Generic;
using System.Linq;

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

    private readonly Dictionary<Agent, List<Tile>> _trips = new Dictionary<Agent, List<Tile>>();

    public BasicAgentTrait() : base()
    {
      EventManager.TurnAction += OnTurnAction;
      EventManager.Attack += OnAttack;
    }

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

    public void OnAttack(object sender, AttackEvent e)
    {
      // Determining helpers
      if (e.Attacker is Agent && e.Defender is Creature && e.Strength <= e.Defense)
      {
        var availableHelpers = e.GameState.Agents.Where(a => a != e.Attacker && a.AvailableBonusActionPoints > 0).ToList();
        foreach (Agent agent in availableHelpers)
        {
          int missingStrength = e.Defense - e.Strength;
          int availableStrength = agent.GetStrengthAgainst(e.Defender);
          if (missingStrength > 0 && availableStrength > 0)
          {
            int strengthModifier = (availableStrength > missingStrength) ? missingStrength : availableStrength;
            e.Helpers.Add(agent);
            e.Strength += strengthModifier;
            Log(this, $"Agent {agent.Name} decided to help {e.Attacker.Name} fight {e.Defender.Name} with {strengthModifier} strength");
          }
        }
      }
    }

    public void OnTurnAction(object sender, TurnActionEvent e)
    {
      /*
       Own Turn APL
       x resume current trip
       x move towards a healer if dead
       - use item
       x attack a reachable weaker player if we think we can survive the ripost
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
          CreateTrip(e.Source, BoardManager.ShortestPathToTileType(e.Source.Position, TileType.Healer));
          ResumeTrip(e);
          return;
        }

        // use item if needed
        var usableItems = e.Source.Items.Where(i => i.Aura != null && i.Aura.Trait.ActionPointCost <= e.Source.AvailableActionPoints).ToList();
        if (usableItems.Count > 0)
        {
          // todo...
          if (e.Type == TurnActionType.UseItem)
          {
            return;
          }
        }

        // attack a reachable weaker player if we think we can survive the ripost
        var agentAttackRange = e.Parameters.AgentAttackBaseRange; // todo: take modifiers from items and traits into account
        var possibleTargets = e.GameState.Agents
          .Where(agent => agent != e.Source)
          .Select(agent => {
            return new
            {
              Target = agent,
              Path = BoardManager.GetPath(e.Source.Position, agent.Position)
            };
          })
          .Where(
            entry => entry.Path.Count <= agentAttackRange && // is in range
            !e.Source.DefeatedAgents.Contains(entry.Target) && // not already defeated
            e.Source.GetStrengthAgainst(entry.Target) > entry.Target.GetDefenseAgainst(e.Source) && // is weaker then agent
            (entry.Target.GetStrengthAgainst(e.Source) - e.Source.GetDefenseAgainst(entry.Target) < e.Source.AvailableHitPoints) // will not kill agent while riposting
          )
          .OrderBy(entry => entry.Path.Count)
          .ToList();
        if (possibleTargets.Count > 0)
        {
          e.Type = TurnActionType.AttackAgent;
          e.Target = possibleTargets[0].Target;
          return;
        }

        // move to the most valuable point of interest
        var possibleTrips = BoardManager.ReachablePointsOfInterest(e.Source.Position, e.Source.MaxActionPoints, e.Source.PreviousPosition);
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