using Shim.Library;
using Shim.Entities;
using System.Collections.Generic;
using System;
using Shim.Events;

namespace Shim
{
  public class Simulation
  {
    public EventManager _eventManager;
    public AgentManager _agentManager;
    public BoardManager _boardManager;
    private SimulationParameters _parameters;
    private readonly List<Agent> _agents;
    private readonly Deck<Item> _items;
    private readonly Deck<Creature> _creatures;
    private readonly Deck<Aura> _gameEvents;
    private readonly Deck<Aura> _traps;
    private readonly Deck<Aura> _blessings;
    private readonly List<ActiveAura> _activeAuras;
    private int _round;
    private int _turn;
    private Agent _turnAgent;
    private bool _done;

    public Simulation(SimulationParameters parameters)
    {
      _parameters = parameters;
      _activeAuras = new List<ActiveAura>();
      _agents = new List<Agent>();
      _items = new Deck<Item>();
      _traps = new Deck<Aura>();
      _blessings = new Deck<Aura>();
      _gameEvents = new Deck<Aura>();
      _creatures = new Deck<Creature>();
      _agentManager = new AgentManager();
      _eventManager = new EventManager();
      _boardManager = new BoardManager();
      _round = 1;
      _turn = 1;
      _done = false;
      Logger.Init();
      TraitManager.Initialize(_eventManager, _boardManager);
      _boardManager.Initialize();
    }

    public void AddAgent(string name, Trait[] initialTraits = null)
    {
      Agent agent = new Agent(name) {
        MaxHitPoints = _parameters.MaxHitPoints,
        MaxActionPoints = _parameters.DefaultMaxActionPoints,
        MaxBonusActionPoints = _parameters.DefaultMaxBonusActionPoints
      };
      if (initialTraits != null)
      {
        foreach (Trait trait in initialTraits)
        {
          _agentManager.AssignTrait(trait, agent);
        }
      }
      _agents.Add(agent); 
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
        Expiration = ExpirationType.EndOfRound,
        Scope = ScopeType.All,
        Trait = trait
      });
    }

    public void AddBlessing(Aura aura)
    {
      _blessings.Add(aura);
    }

    public void AddTrap(Aura aura)
    {
      _traps.Add(aura);
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
        if (_agents.Count < _parameters.MinAgents || _agents.Count > _parameters.MaxAgents)
        {
          throw new Exception($"Expecting between {_parameters.MinAgents} and {_parameters.MaxAgents} agents but got {_agents.Count}");
        }

        // Shuffle decks
        Logger.Log($"Shuffling decks");
        _items.Shuffle();
        _traps.Shuffle();
        _blessings.Shuffle();
        _creatures.Shuffle();

        // Initialize agents
        Logger.Log($"Initializing agents");
        _agents.ForEach(agent =>
        {
          _agentManager.ResetHitPoints(agent);
          _eventManager.OnAgentInit(this, new AgentInitEvent() { NewAgent = agent });
        });

        // Starting item draft
        if (_parameters.StartingItemEnabled)
        {
          Logger.Log($"Starting items draft");
          _agents.ForEach(agent => DrawItem(agent));
        }

        // Start main loop
        while (!_done)
        {
          // Set a new active event
          DrawEvent();
          // Play turns
          for (_turn = 1; _turn <= _agents.Count; _turn++)
          {
            _turnAgent = _agents[_turn - 1];
            ExecuteTurn();
            CheckActiveAurasExpiration();
          }
          // Prepare next round
          if (++_round > _parameters.MaxRounds)
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

    private void ActivateAura(Aura aura, Agent activator = null)
    {
      ActiveAura activeAura = new ActiveAura()
      {
        Aura = aura,
        ActivationRound = _round,
        ActivationTurn = _turn
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
          _agents.ForEach((Agent agent) =>
          {
            if (agent != activator)
            {
              activeAura.Targets.Add(agent);
            }
          });
          break;
        case ScopeType.All:
          _agents.ForEach((Agent agent) =>
          {
            activeAura.Targets.Add(agent);
          });
          break;
      }
      if (activeAura.Aura.Expiration != ExpirationType.Now)
      {
        activeAura.Targets.ForEach((Agent agent) =>
        {
          _agentManager.AssignTrait(aura.Trait, agent);
        });
        _activeAuras.Add(activeAura);
      }
    }

    private void CheckActiveAurasExpiration()
    {
      for (int i = _activeAuras.Count - 1; i >= 0; i--)
      {
        ActiveAura activeAura = _activeAuras[i];
        ExpirationType type = activeAura.Aura.Expiration;
        bool isActivatorTurn = activeAura.Activator == _turnAgent;
        bool isEndOfActivatorTurn = (type == ExpirationType.EndOfTurn && isActivatorTurn && _turn == activeAura.ActivationTurn);
        bool isEndOfActivatorNextTurn = (type == ExpirationType.EndOfNextTurn && isActivatorTurn && _round > activeAura.ActivationRound);
        bool isEndOfRound = (type == ExpirationType.EndOfRound && _turn == _agents.Count);
        if (isEndOfActivatorTurn || isEndOfActivatorNextTurn || isEndOfRound)
        {
          activeAura.Targets.ForEach((Agent agent) =>
          {
            _agentManager.UnassignTrait(activeAura.Aura.Trait, agent);
          });
          if (activeAura.IsGameEvent)
          {
            _gameEvents.Discard(activeAura.Aura);
          }
          _activeAuras.Remove(activeAura);
        }
      }
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
        _eventManager.OnTooManyPermanentItems(this, decision);
        if (decision.ItemToDiscard != null)
        {
          _agentManager.UnassignItem(decision.ItemToDiscard, agent);
          _items.Discard(decision.ItemToDiscard);
        }
        else
        {
          keepItem = false;
        }
      }
      if (keepItem)
      {
        _agentManager.AssignItem(item, agent);
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
      Aura trap = _blessings.Draw();
      ActivateAura(trap, agent);
    }

    private void DrawCreature(Agent agent)
    {
      Creature creature = _creatures.Draw();
      PerformAttack(creature, agent);
      if (!agent.IsDead && PerformAttack(agent, creature))
      {
        var targetDefeat = new TargetDefeatedEvent()
        {
          Source = agent,
          Target = creature,
          FavorReward = creature.FavorReward
        };
        _eventManager.OnTargetDefeated(this, targetDefeat);
        RewardFavor(agent, targetDefeat.FavorReward);
        targetDefeat.Helpers.ForEach((Agent helper) =>
        {
          AgentHelpedEvent agentHelp = new AgentHelpedEvent()
          {
            Target = creature,
            Helper = helper
          };
          _eventManager.OnAgentHelped(this, agentHelp);
          RewardFavor(agentHelp.Helper, agentHelp.FavorReward);
        });
      }
      _creatures.Discard(creature);
    }

    private bool PerformAttack(Target attacker, Target defender)
    {
      AttackEvent attack = new AttackEvent()
      {
        Attacker = attacker,
        Defender = defender,
        Strength = attacker.GetStrengthAgainst(defender),
        Defense = defender.GetDefenseAgainst(attacker)
      };
      Logger.Log($"{attacker.Name} is attacking {defender.Name} ({attack.Strength} STR / {attack.Defense} DEF)");
      _eventManager.OnAttack(this, attack);
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
          _agentManager.ModifyHitPoints((Agent) defender, damageTaken * -1);
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
      _agentManager.ModifyActionPoints(attacker, -1);
      PerformAttack(attacker, defender);
      if (!defender.IsDead)
      {
        PerformAttack(defender, attacker);
      }
      if (!attacker.IsDead)
      {
        var targetDefeated = new TargetDefeatedEvent()
        {
          Target = defender,
          Source = attacker
        };
        Logger.Log($"Agent {targetDefeated.Source.Name} defeated agent {targetDefeated.Target.Name}");
        _eventManager.OnTargetDefeated(this, targetDefeated);
        RewardFavor(targetDefeated.Source, targetDefeated.FavorReward);
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
      _eventManager.OnMove(this, move);
      _agentManager.ModifyActionPoints(agent, move.ActionPointCost * -1);
      _agentManager.SetPosition(move.Agent, move.Tile);
      switch (move.Tile.Type)
      {
        case TitleType.Blessing:
          DrawBlessing(move.Agent);
          break;
        case TitleType.Creature:
          DrawCreature(move.Agent);
          break;
        case TitleType.Discovery:
          RewardFavor(move.Agent, tile.IntValue);
          break;
        case TitleType.Gate:
          EnterGate(move.Agent, tile.StringValue);
          break;
        case TitleType.Healer:
          VisitHealer(move.Agent);
          break;
        case TitleType.Item:
          DrawItem(move.Agent);
          break;
        case TitleType.Trap:
          DrawTrap(move.Agent);
          break;
      }
    }

    private void EnterGate(Agent agent, string exitId)
    {
      Tile gateExit = _boardManager.GetTile(exitId);
      if (gateExit == null)
      {
        Logger.Log($"Error: Gate exit {exitId} doesn't exists!");
        return;
      }
      _agentManager.SetPosition(agent, gateExit);
    }

    private void RewardFavor(Agent agent, int amount)
    {
      FavorGainedEvent favorGain = new FavorGainedEvent()
      {
        Agent = agent,
        Amount = amount
      };
      _eventManager.OnFavorGained(this, favorGain);
      _agentManager.ModifyFavor(favorGain.Agent, favorGain.Amount);
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
        _eventManager.OnAgentResurrected(this, agentResurrection);
        _agentManager.ModifyHitPoints(agentResurrection.Agent, agentResurrection.HitPoints);
      }
      else
      {
        _agentManager.ModifyHitPoints(agent, _parameters.HitPointsRestoredByHealer);
      }
    }

    private void UseItem(Item item, Agent source, Agent target)
    {
      Logger.Log("UseItem(): not yet implemented");
    }

    private void ExecuteTurn()
    {
      Logger.Log($"Turn: {_turn} ({_agents[_turn-1].Name}) of round {_round}");
      _agentManager.ResetActionPoints(_turnAgent);
      bool endOfTurn = false;
      int maxActions = _parameters.MaxActionsPerTurn;
      int actionsDone = 0;
      while (!endOfTurn)
      {
        var nextAction = new TurnActionEvent()
        {
          Type = TurnActionType.Undecided,
          Source = _turnAgent 
        };
        _eventManager.OnTurnAction(this, nextAction);
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
            Logger.Log($"Agent {_turnAgent.Name} has decided to end his turn");
            endOfTurn = true;
            break;
          case TurnActionType.Undecided:
            Logger.Log($"Agent {_turnAgent.Name} couldn't decide what to do next");
            endOfTurn = true;
            break;
        }
        actionsDone++;
        if (actionsDone > maxActions)
        {
          Logger.Log($"Agent {_turnAgent.Name} has done too many actions during his turn ({maxActions})");
          endOfTurn = true;
        }
      }
      Logger.Log($"End of turn: {_turn} ({_agents[_turn - 1].Name}) of round {_round}");
    }
  }
}