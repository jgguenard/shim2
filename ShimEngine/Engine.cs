using Microsoft.Extensions.Logging;
using Raido.Shim.Entities;
using Raido.Shim.Events;
using System;
using System.Collections.Generic;

namespace Raido.Shim
{
  public class Engine
  {
    public readonly Settings _settings;
    public readonly EventManager _eventManager;
    public readonly ILogger _logger;

    public List<Player> _players;

    public Engine(Settings settings, ILogger<Engine> logger, EventManager eventManager)
    {
      _settings = settings;
      _players = new List<Player>();
      _logger = logger;
      _eventManager = eventManager;
    }

    public void AddPlayer(string name, Trait[] traits = null)
    {
      Player player = new Player(name);
      if (traits != null) 
      {
        foreach (Trait trait in traits)
        {
          player.AssignTrait(trait);
        }
      }
      _players.Add(player);
      _logger.LogInformation("Player {name} added", name);
    }

    public void Run()
    {
      try
      {
        // integrity checks
        if (_players.Count < _settings.MinPlayers || _players.Count > _settings.MaxPlayers)
        {
          throw new Exception($"Expected between {_settings.MinPlayers} and {_settings.MaxPlayers} players but got {_players.Count}");
        }

        // players initialization
        _players.ForEach(player => InitializePlayer(player));
      } 
      catch (Exception ex)
      {
        _logger.LogError(ex, ex.Message);
      }
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