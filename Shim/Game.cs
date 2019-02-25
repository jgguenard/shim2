using Shim.Library;
using Shim.Entities;
using System.Collections.Generic;
using System;
using Shim.Events;

namespace Shim
{
  public class Game
  {
    private readonly GameParameters _parameters;
    private readonly GameState _state;
    private readonly Deck<Item> _items;
    private readonly Deck<Creature> _creatures;
    private readonly Deck<Aura> _gameEvents;
    private readonly Deck<Aura> _traps;
    private readonly Deck<Aura> _blessings;
    private bool _done;

    public Game(GameParameters parameters)
    {
      _parameters = parameters;
      _state = new GameState();
      _items = new Deck<Item>("Items");
      _traps = new Deck<Aura>("Traps");
      _blessings = new Deck<Aura>("Blessings");
      _gameEvents = new Deck<Aura>("Game Events");
      _creatures = new Deck<Creature>("Creatures");
      _done = false;
      Logger.Init();
      BoardManager.Initialize();
    }
    public GameState GetState()
    {
      return _state;
    }
    public void AddAgent(string name, Trait[] initialTraits = null)
    {
      Agent agent = new Agent(name) {
        MaxHitPoints = _parameters.MaxHitPoints
      };
      if (initialTraits != null)
      {
        foreach (Trait trait in initialTraits)
        {
          AgentManager.AssignTrait(trait, agent);
        }
      }
      _state.Agents.Add(agent);
      Logger.Log($"Agent {name} was added to simulation");
    }
    public void AddItem(Item item)
    {
      _items.Add(item);
    }
    public void AddCreature(Creature creature)
    {
      _creatures.Add(creature);
    }
    public void AddEvent(Trait trait)
    {
      _gameEvents.Add(new Aura()
      {
        Type = AuraType.GameEvent,
        Expiration = ExpirationType.EndOfRound,
        Scope = ScopeType.All,
        Trait = trait
      });
    }
    public void AddBlessing(Trait trait, ExpirationType expiration)
    {
      _blessings.Add(new Aura() {
        Trait = trait,
        Type = AuraType.Blessing,
        Scope = ScopeType.Self,
        Expiration = expiration
      });
    }
    public void AddTrap(Trait trait, ExpirationType expiration)
    {
      _traps.Add(new Aura()
      {
        Trait = trait,
        Type = AuraType.Other,
        Scope = ScopeType.Self,
        Expiration = expiration
      });
    }
    public List<string> GetLog()
    {
      return Logger.Lines;
    }
    public void Run()
    {
      try
      {
        // Integrity checks
        if (_done)
        {
          throw new Exception($"Cannot run the same simulation twice");
        }
        if (_state.Agents.Count < _parameters.MinAgents || _state.Agents.Count > _parameters.MaxAgents)
        {
          throw new Exception($"Expecting between {_parameters.MinAgents} and {_parameters.MaxAgents} agents but got {_state.Agents.Count}");
        }
        Logger.Log("Initialization", true);

        // Shuffle decks
        Logger.Log($"Shuffling decks");
        _items.Shuffle();
        _traps.Shuffle();
        _blessings.Shuffle();
        _creatures.Shuffle();

        // Initialize agents
        Logger.Log($"Initializing agents");
        for (var i = 0; i < _state.Agents.Count; i++)
        {
          Agent agent = _state.Agents[i];
          string startingTileId = _parameters.StartingTiles[i];
          if (startingTileId == null)
          {
            throw new Exception($"No starting tile for Agent {_state.Agents[i].Name}");
          }
          InitializeAgent(agent, BoardManager.GetTile(startingTileId));
        }

        // Starting item draft
        if (_parameters.StartingItemEnabled)
        {
          Logger.Log($"Starting items draft");
          _state.Agents.ForEach(agent => DrawItem(agent));
        }

        // Start main loop
        while (!_done)
        {
          // Set a new active event
          DrawEvent();
          // Play turns
          for (_state.Turn = 1; _state.Turn <= _state.Agents.Count; _state.Turn++)
          {
            ExecuteTurn();
            CheckActiveAurasExpiration();
          }
          // Prepare next round
          if (_state.Round < _parameters.MaxRounds)
          {
            _state.Round++;
          }
          else
          {
            _done = true;
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Log($"Exception: {ex.Message}");
      }
    }
    private void InitializeAgent(Agent agent, Tile position)
    {
      AgentInitEvent agentInit = new AgentInitEvent()
      {
        Agent = agent,
        BaseStrength = _parameters.DefaultBaseStrength,
        BaseDefense = _parameters.DefaultBaseDefense,
        MaxActionPoints = _parameters.DefaultMaxActionPoints,
        MaxBonusActionPoints = _parameters.DefaultMaxBonusActionPoints
      };
      EventManager.OnAgentInit(this, agentInit);
      AgentManager.ModifyBaseStrength(agent, agentInit.BaseStrength);
      AgentManager.ModifyBaseDefense(agent, agentInit.BaseDefense);
      AgentManager.ModifyMaxActionPoints(agent, agentInit.MaxActionPoints);
      AgentManager.ModifyMaxBonusActionPoints(agent, agentInit.MaxBonusActionPoints);
      AgentManager.ResetHitPoints(agent);
      AgentManager.SetPosition(agent, position);
    }
    private void ActivateAura(Aura aura, Agent activator = null)
    {
      ActiveAura activeAura = new ActiveAura()
      {
        Aura = aura,
        ActivationRound = _state.Round,
        ActivationTurn = _state.Turn
      };
      if (activator != null)
      {
        activeAura.Activator = activator;
      }
      switch (aura.Scope)
      {
        case ScopeType.Self:
          if (activator != null)
          {
            activeAura.Targets.Add(activator);
          }
          break;
        case ScopeType.Others:
          _state.Agents.ForEach((Agent agent) =>
          {
            if (agent != activator)
            {
              activeAura.Targets.Add(agent);
            }
          });
          break;
        case ScopeType.All:
          _state.Agents.ForEach((Agent agent) =>
          {
            activeAura.Targets.Add(agent);
          });
          break;
      }
      activeAura.Targets.ForEach((Agent agent) =>
      {
        AgentManager.AssignTrait(aura.Trait, agent);
        var auraActivated = new AuraActivatedEvent()
        {
          Agent = agent,
          Aura = aura
        };
        EventManager.OnAuraActivated(this, auraActivated);
        if (auraActivated.FavorModifier != 0)
        {
          AgentManager.ModifyFavor(agent, auraActivated.FavorModifier);
        }
        if (auraActivated.HitPointsModifier != 0)
        {
          AgentManager.ModifyHitPoints(agent, auraActivated.HitPointsModifier);
        }
        if (auraActivated.ActionPointsModifier != 0)
        {
          AgentManager.ModifyActionPoints(agent, auraActivated.ActionPointsModifier);
        }
        if (auraActivated.BonusActionPointsModifier != 0)
        {
          AgentManager.ModifyBonusActionPoints(agent, auraActivated.BonusActionPointsModifier);
        }
      });
      _state.ActiveAuras.Add(activeAura);
      if (activeAura.Aura.Expiration == ExpirationType.Now)
      {
        DeactivateAura(activeAura);
      }
    }
    private void CheckActiveAurasExpiration()
    {
      for (int i = _state.ActiveAuras.Count - 1; i >= 0; i--)
      {
        ActiveAura activeAura = _state.ActiveAuras[i];
        ExpirationType type = activeAura.Aura.Expiration;
        bool isActivatorTurn = activeAura.Activator == _state.TurnAgent;
        bool isEndOfActivatorTurn = (type == ExpirationType.EndOfTurn && isActivatorTurn && _state.Turn == activeAura.ActivationTurn);
        bool isEndOfActivatorNextTurn = (type == ExpirationType.EndOfNextTurn && isActivatorTurn && _state.Round > activeAura.ActivationRound);
        bool isEndOfRound = (type == ExpirationType.EndOfRound && _state.Turn == _state.Agents.Count);
        if (isEndOfActivatorTurn || isEndOfActivatorNextTurn || isEndOfRound)
        {
          DeactivateAura(activeAura);
        }
      }
    }
    private void DeactivateAura(ActiveAura activeAura)
    {
      activeAura.Targets.ForEach((Agent agent) =>
      {
        AgentManager.UnassignTrait(activeAura.Aura.Trait, agent);
      });
      if (activeAura.Aura.Type == AuraType.GameEvent)
      {
        _gameEvents.Discard(activeAura.Aura);
      }
      else if (activeAura.Aura.Type == AuraType.Blessing)
      {
        _blessings.Discard(activeAura.Aura);
      }
      _state.ActiveAuras.Remove(activeAura);
    }
    private void DrawItem(Agent agent)
    {
      Item item = _items.Draw();
      bool keepItem = true;
      if (item.IsPermanent && agent.PermanentItemCount == _parameters.MaxPermanentItemsPerAgent)
      {
        TooManyPermanentItemsEvent decision = new TooManyPermanentItemsEvent()
        {
          Source = agent,
          NewItem = item
        };
        EventManager.OnTooManyPermanentItems(this, decision);
        if (decision.ItemToDiscard != null)
        {
          AgentManager.UnassignItem(decision.ItemToDiscard, agent);
          _items.Discard(decision.ItemToDiscard);
        }
        else
        {
          keepItem = false;
        }
      }
      if (keepItem)
      {
        AgentManager.AssignItem(item, agent);
      }
      else
      {
        _items.Discard(item);
      }
    }
    private void DrawEvent()
    {
      Aura gameEvent = _gameEvents.Draw();
      Logger.Log($"New game event: {gameEvent.Trait.Name}");
      ActivateAura(gameEvent);
    }
    private void DrawTrap(Agent agent)
    {
      Aura trap = _traps.Draw();
      ActivateAura(trap, agent);
    }
    private void DrawBlessing(Agent agent)
    {
      Aura blessing = _blessings.Draw();
      var activeBlessings = _state.GetAgentActiveAurasByType(agent, AuraType.Blessing);
      if (activeBlessings.Count == _parameters.MaxActiveBlessingPerAgent)
      {
        DeactivateAura(activeBlessings[0]);
      }
      ActivateAura(blessing, agent);
    }
    private void DrawCreature(Agent agent)
    {
      Creature creature = _creatures.Draw();
      PerformAttack(creature, agent, out AttackEvent attack);
      if (!agent.IsDead && PerformAttack(agent, creature, out AttackEvent ripost))
      {
        var targetDefeat = new TargetDefeatedEvent()
        {
          Source = agent,
          Target = creature,
          Helpers = ripost.Helpers,
          FavorReward = creature.FavorReward
        };
        EventManager.OnTargetDefeated(this, targetDefeat);        
        AgentManager.ModifyFavor(agent, targetDefeat.FavorReward);
        targetDefeat.Helpers.ForEach((Agent helper) =>
        {
          AgentHelpedEvent agentHelped = new AgentHelpedEvent()
          {
            Target = creature,
            Helper = helper
          };
          EventManager.OnAgentHelped(this, agentHelped);
          AgentManager.ModifyBonusActionPoints(agentHelped.Helper, -1 * _parameters.BaseActionCost);
          AgentManager.ModifyFavor(agentHelped.Helper, agentHelped.FavorReward);
        });
      }
      _creatures.Discard(creature);
    }
    private bool PerformAttack(Target attacker, Target defender, out AttackEvent attack)
    {
      attack = new AttackEvent()
      {
        GameState = _state,
        Attacker = attacker,
        Defender = defender,
        Strength = attacker.GetStrengthAgainst(defender),
        Defense = defender.GetDefenseAgainst(attacker)
      };
      Logger.Log($"{attacker.Name} is attacking {defender.Name} ({attack.Strength} STR / {attack.Defense} DEF)");
      EventManager.OnAttack(this, attack);
      if (attack.Strength < 1)
      {
        Logger.Log($"{attacker.Name} could not attack");
        return false;
      }
      Logger.Log($"{attacker.Name} has attacked {defender.Name} ({attack.Strength} STR / {attack.Defense} DEF)");
      if (attack.Strength > attack.Defense)
      {
        Logger.Log($"{defender.Name} was defeated by {attacker.Name}");
        if (defender is Agent)
        {
          int damageTaken = (attack.Strength - attack.Defense);
          AgentManager.ModifyHitPoints((Agent) defender, damageTaken * -1);
        }
        return true;
      }
      else
      {
        Logger.Log($"{attacker.Name}'s attack was ineffective");
      }
      return false;
    }
    private void AttackAgent(Agent attacker, Agent defender)
    {
      AgentManager.ModifyActionPoints(attacker, -1);
      var victory = PerformAttack(attacker, defender, out AttackEvent attack);
      if (!defender.IsDead)
      {
        PerformAttack(defender, attacker, out AttackEvent ripost);
      }
      if (victory && !attacker.IsDead)
      {
        var targetDefeated = new TargetDefeatedEvent()
        {
          Target = defender,
          Source = attacker
        };
        Logger.Log($"Agent {targetDefeated.Source.Name} defeated agent {targetDefeated.Target.Name}");
        EventManager.OnTargetDefeated(this, targetDefeated);
        AgentManager.ModifyFavor(targetDefeated.Source, targetDefeated.FavorReward);
        AgentManager.RegisterDuelVictory(targetDefeated.Source, (Agent)targetDefeated.Target);
      }
    }
    private void Move(Agent agent, Tile tile)
    {
      MoveEvent move = new MoveEvent()
      {
        Agent = agent,
        ActionPointCost = _parameters.BaseMovementCost,
        Tile = tile
      };
      EventManager.OnMove(this, move);
      AgentManager.ModifyActionPoints(agent, move.ActionPointCost * -1);
      AgentManager.SetPosition(move.Agent, move.Tile);
      switch (move.Tile.Type)
      {
        case TileType.Blessing:
          DrawBlessing(move.Agent);
          break;
        case TileType.Creature:
          DrawCreature(move.Agent);
          break;
        case TileType.Discovery:
          AgentManager.ModifyFavor(move.Agent, tile.IntValue);
          break;
        case TileType.Gate:
          EnterGate(move.Agent, tile.StringValue);
          break;
        case TileType.Healer:
          VisitHealer(move.Agent);
          break;
        case TileType.Item:
          DrawItem(move.Agent);
          break;
        case TileType.Trap:
          DrawTrap(move.Agent);
          break;
      }
    }
    private void EnterGate(Agent agent, string exitId)
    {
      Tile gateExit = BoardManager.GetTile(exitId);
      if (gateExit == null)
      {
        Logger.Log($"Error: Gate exit {exitId} doesn't exists!");
        return;
      }
      AgentManager.SetPosition(agent, gateExit);
    }
    private void VisitHealer(Agent agent)
    {
      if (agent.IsDead)
      {
        AgentResurrectedEvent agentResurrection = new AgentResurrectedEvent()
        {
          Agent = agent,
          HitPoints = _parameters.HitPointsAfterResurrection
        };
        EventManager.OnAgentResurrected(this, agentResurrection);
        AgentManager.ModifyHitPoints(agentResurrection.Agent, agentResurrection.HitPoints);
      }
      else
      {
        AgentManager.ModifyHitPoints(agent, _parameters.HitPointsRestoredByHealer);
      }
    }
    private void UseItem(Item item, Agent source, Agent target)
    {
      Logger.Log("UseItem(): not yet implemented");
    }
    private void ExecuteTurn()
    {
      Logger.Log($"Turn {_state.Turn} ({_state.Agents[_state.Turn-1].Name}) of round {_state.Round}", true);
      AgentManager.ResetActionPoints(_state.TurnAgent);
      bool endOfTurn = false;
      int maxActions = _parameters.MaxActionsPerTurn;
      int actionsDone = 0;
      while (!endOfTurn)
      {
        var nextAction = new TurnActionEvent()
        {
          Parameters = _parameters,
          GameState = _state,
          Type = TurnActionType.Undecided,
          Source = _state.TurnAgent
        };
        EventManager.OnTurnAction(this, nextAction);
        switch (nextAction.Type)
        {
          case TurnActionType.AttackAgent:
            AttackAgent(nextAction.Source, nextAction.Target);
            break;
          case TurnActionType.Move:
            Move(nextAction.Source, nextAction.Tile);
            break;
          case TurnActionType.UseItem:
            UseItem(nextAction.Item, nextAction.Source, nextAction.Target);
            break;
          case TurnActionType.Stop:
            Logger.Log($"Agent {_state.TurnAgent.Name} has decided to end his turn");
            endOfTurn = true;
            break;
          case TurnActionType.Undecided:
            Logger.Log($"Agent {_state.TurnAgent.Name} couldn't decide what to do next");
            endOfTurn = true;
            break;
        }
        actionsDone++;
        if (actionsDone > maxActions)
        {
          Logger.Log($"Agent {_state.TurnAgent.Name} has done too many actions during his turn ({maxActions})");
          endOfTurn = true;
        }
      }
      Logger.Log($"End of turn: {_state.Turn} ({_state.Agents[_state.Turn - 1].Name}) of round {_state.Round}");
    }
  }
}