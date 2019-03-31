using Microsoft.Extensions.Logging;
using Raido.Shim.Entities;
using Raido.Shim.Events;
using Raido.Shim.Library;
using System;
using System.Collections.Generic;

namespace Raido.Shim
{
  public class Engine
  {
    private readonly Settings _settings;
    private readonly EventManager _eventManager;
    private readonly ILogger _logger;

    private int _turn;
    private int _round;
    private bool _done;
    private bool _started;
    private readonly Deck<Aura> _gameEvents;
    private readonly List<ActiveAura> _activeAuras;
    private readonly List<Player> _players;

    public Player TurnPlayer
    {
      get
      {
        return _players[_turn - 1];
      }
    }

    public Engine(Settings settings, ILogger<Engine> logger, EventManager eventManager)
    {
      _settings = settings;
      _players = new List<Player>();
      _logger = logger;
      _eventManager = eventManager;
      _gameEvents = new Deck<Aura>("Game Events");
      _activeAuras = new List<ActiveAura>();
      _started = false;
      _done = false;
      _round = 1;
      _turn = 1;
    }

    public void AddPlayer(string name, Trait[] traits = null)
    {
      if (_started)
      {
        throw new Exception($"Cannot add player after the simulation has started");
      }

      Player player = new Player(name);
      if (traits != null) 
      {
        foreach (Trait trait in traits)
        {
          player.AssignTrait(trait);
        }
      }
      _players.Add(player);
    }

    public void AddGameEvent(Trait trait)
    {
      if (_started)
      {
        throw new Exception($"Cannot add game events after the simulation has started");
      }

      _gameEvents.Add(new Aura()
      {
        Source = AuraSource.GameEvent,
        Expiration = AuraExpiration.EndOfRound,
        Scope = AuraScope.All,
        Trait = trait
      });
    }

    public void Run()
    {
      // simulation state check
      if (_started)
      {
        throw new Exception($"Cannot run the same simulation twice");
      }

      // integrity checks
      if (_players.Count < _settings.MinPlayers || _players.Count > _settings.MaxPlayers)
      {
        throw new Exception($"Expected between {_settings.MinPlayers} and {_settings.MaxPlayers} players but got {_players.Count}");
      }

      Guid sid = Guid.NewGuid();
      try
      {
        _logger.LogInformation("============= SIMULATION {id} BEGIN ============", sid);
        _started = true;

        // players initialization
        _players.ForEach(player => InitializePlayer(player));

        while (!_done)
        {
          _logger.LogInformation("Beginning round {i}", _round);
        
          if (_settings.GameEventEnabled)
          {
            DrawEvent();
          }

          // Play turns
          for (_turn = 1; _turn <= _players.Count; _turn++)
          {
            ExecuteTurn();
            CheckActiveAurasExpiration();
          }

          // Prepare next round
          if (_round < _settings.MaxRounds)
          {
            _round++;
          }
          else
          {
            _done = true;
            throw new Exception($"Max number of rounds ({_settings.MaxRounds}) reached");
          }
        }
      } 
      catch (Exception ex)
      {
        _logger.LogError(ex, ex.Message);
      }
      _logger.LogInformation("============= SIMULATION {id} END ============", sid);
    }

    private void ExecuteTurn()
    {
      _logger.LogInformation("Beginning turn {i}", _turn);
    }

    private void DrawEvent()
    {
      Aura gameEvent = _gameEvents.Draw();
      _logger.LogInformation("Activating game event: {e}", gameEvent.Trait.Name);
      ActivateAura(gameEvent);
    }

    private void CheckActiveAurasExpiration()
    {
      for (int i = _activeAuras.Count - 1; i >= 0; i--)
      {
        ActiveAura activeAura = _activeAuras[i];
        AuraExpiration type = activeAura.Aura.Expiration;
        bool isActivatorTurn = activeAura.Activator == TurnPlayer;
        bool isEndOfActivatorTurn = (type == AuraExpiration.EndOfTurn && isActivatorTurn && _turn == activeAura.ActivationTurn);
        bool isEndOfActivatorNextTurn = (type == AuraExpiration.EndOfNextTurn && isActivatorTurn && _round > activeAura.ActivationRound);
        bool isEndOfRound = (type == AuraExpiration.EndOfRound && _turn == _players.Count);
        if (isEndOfActivatorTurn || isEndOfActivatorNextTurn || isEndOfRound)
        {
          DeactivateAura(activeAura);
        }
      }
    }

    private void ActivateAura(Aura aura, Agent activator = null)
    {
      ActiveAura activeAura = new ActiveAura()
      {
        Aura = aura,
        ActivationRound = _round,
        ActivationTurn = _turn,
        Activator = activator
      };

      // building target list
      switch (aura.Scope)
      {
        case AuraScope.Self:
          if (activator != null)
          {
            activeAura.Targets.Add(activator);
          }
          break;
        case AuraScope.Others:
          _players.ForEach((Player player) =>
          {
            if (player != activator)
            {
              activeAura.Targets.Add(player);
            }
          });
          break;
        case AuraScope.All:
          _players.ForEach((Player player) =>
          {
            activeAura.Targets.Add(player);
          });
          break;
      }

      activeAura.Targets.ForEach((Agent agent) =>
      {
        agent.AssignTrait(aura.Trait);
        if (agent is Player player)
        {
          var auraActivated = new AuraActivatedEvent()
          {
            Player = player,
            Aura = aura
          };
          _eventManager.OnAuraActivated(this, auraActivated);
          if (auraActivated.FavorModifier != 0)
          {
            player.ModifyFavor(auraActivated.FavorModifier);
          }
          if (auraActivated.HitPointsModifier != 0)
          {
            player.ModifyHitPoints(auraActivated.HitPointsModifier);
          }
        }
        _activeAuras.Add(activeAura);
        if (activeAura.Aura.Expiration == AuraExpiration.Now)
        {
          DeactivateAura(activeAura);
        }
      });
    }

    public void DeactivateAura(ActiveAura activeAura)
    {
      activeAura.Targets.ForEach((Agent agent) =>
      {
        agent.UnassignTrait(activeAura.Aura.Trait);
      });
      if (activeAura.Aura.Source == AuraSource.GameEvent)
      {
        _gameEvents.Discard(activeAura.Aura);
      }
      _activeAuras.Remove(activeAura);
    }

    private void InitializePlayer(Player player)
    {
      PlayerInitEvent playerInit = new PlayerInitEvent
      {
        Player = player,
        BaseDefense = _settings.DefaultBaseDefense,
        BaseStrength = _settings.DefaultBaseStrength,
        MaxHitPoints = _settings.MaxHitPoints
      };
      _eventManager.OnPlayerInit(this, playerInit);
      player.MaxHitPoints = playerInit.MaxHitPoints;
      player.BaseStrength = playerInit.BaseStrength;
      player.BaseDefense = playerInit.BaseDefense;
      player.ResetHitPoints();
    }
  }
}