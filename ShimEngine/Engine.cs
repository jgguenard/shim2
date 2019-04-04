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
    private bool _ended;
    private bool _started;
    private int _availableFavor;
    private readonly Deck<Aura> _gameEvents;
    private readonly Deck<Entity> _explorationChoices;
    private readonly List<ActiveAura> _activeAuras;
    private readonly List<Player> _players;

    public Player TurnPlayer
    {
      get
      {
        return _players[_turn - 1];
      }
    }

    #region Public Methods
    public Engine(Settings settings, ILogger<Engine> logger, EventManager eventManager)
    {
      _settings = settings;
      _players = new List<Player>();
      _logger = logger;
      _eventManager = eventManager;
      _gameEvents = new Deck<Aura>("Game Events");
      _explorationChoices = new Deck<Entity>("Exploration");
      _activeAuras = new List<ActiveAura>();
      _availableFavor = 0;
      _started = false;
      _ended = false;
      _round = 0;
      _turn = 0;
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

    public void AddExplorationChoice(Entity entity, int copies = 1)
    {
      if (_started)
      {
        throw new Exception($"Cannot add entity after the simulation has started");
      }
      for (int i = 0; i < copies; i++)
      {
        _explorationChoices.Add(entity);
      }
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

        _availableFavor = _settings.AvailableFavor(_players.Count);
        _logger.LogInformation($"Setting amount of available favor to {_availableFavor}");

        // players initialization
        _players.ForEach(player => InitializePlayer(player));

        while (!_ended)
        {
          // Prepare new round
          if (_round < _settings.MaxRounds)
          {
            _round++;
          }
          else
          {
            _ended = true;
            throw new Exception($"Max number of rounds ({_settings.MaxRounds}) reached");
          }

          _logger.LogInformation("Round {i}", _round);
        
          if (_settings.GameEventEnabled)
          {
            DrawEvent();
          }

          // Play turns
          for (_turn = 1; _turn <= _players.Count; _turn++)
          {
            ExecuteTurn();
            CheckActiveAurasExpiration();
            CheckEndCondition();
          }
        }
      } 
      catch (Exception ex)
      {
        _logger.LogError(ex, ex.Message);
      }
      _logger.LogInformation("============= SIMULATION {id} END ============", sid);
    }
    #endregion

    #region Private Methods
    private void CheckEndCondition()
    {
      if (_availableFavor == 0 && _turn == _players.Count)
      {
        _logger.LogInformation("END: There is no favor left and the last turn has been played");
        _ended = true;
      }
    }

    private void ExecuteTurn()
    {
      _logger.LogInformation("Turn {i} of round {r}", _turn, _round);
      bool endOfTurn = false;
      int maxActions = _settings.MaxActionsPerTurn;
      int actionsDone = 0;
      TurnState turnState = new TurnState() {
        CanExplore = true,
        Round = _round,
        Turn = _turn,
        Player = TurnPlayer
      };
      while (!endOfTurn)
      {
        var action = new TurnActionEvent()
        {
          Type = TurnActionType.Undecided,
          State = turnState
        };
        _eventManager.OnTurnAction(this, action);

        switch (action.Type)
        {
          case TurnActionType.Explore:
            Explore();
            turnState.CanExplore = false;
            break;
          case TurnActionType.Duel:
            FightPlayer(TurnPlayer, action.Target);
            break;
          case TurnActionType.UseSkill:
            UseSkill((Equipment)action.Entity, action.Target, TurnPlayer);
            break;
          case TurnActionType.UsePotion:
            UsePotion((Potion)action.Entity, action.Target, TurnPlayer);
            break;
          case TurnActionType.Stop:
            endOfTurn = true;
            _logger.LogInformation($"{TurnPlayer.Name} has decided to end his turn");
            break;
          case TurnActionType.Undecided:
            endOfTurn = true;
            _logger.LogWarning($"{TurnPlayer.Name} couldn't decide what to do next");
            break;
        }

        actionsDone++;
        if (actionsDone > maxActions)
        {
          endOfTurn = true;
          _logger.LogWarning($"Turn was ended because too many actions were done ({maxActions})");
        }
      }
    }

    private void Explore()
    {
      _logger.LogInformation("{player} is exploring", TurnPlayer.Name);
      List<Entity> choices = new List<Entity>();
      for (int i = 0; i < _settings.ExplorationChoices; i++)
      {
        choices.Add(_explorationChoices.Draw());
      }
      var playerChoiceIndex = 0;
      var playerChoice = choices[playerChoiceIndex]; // todo: real decision making
      if (playerChoice is Equipment equipment)
      {
        AssignEquipment(equipment, TurnPlayer);
      }
      else if (playerChoice is Potion potion)
      {
        AssignPotion(potion, TurnPlayer);
      }
      else if (playerChoice is Creature creature)
      {
        FightCreature(creature, TurnPlayer);
      }
      choices.RemoveAt(playerChoiceIndex);
      choices.ForEach(unusedChoice => _explorationChoices.Discard(unusedChoice));
    }

    private void DrawEvent()
    {
      Aura gameEvent = _gameEvents.Draw();
      _logger.LogInformation("Activating game event: {e}", gameEvent.Trait.Name);
      ActivateAura(gameEvent);
    }

    private void AssignEquipment(Equipment equipment, Player player)
    {
      if (player.Equipment.Count < _settings.EquipmentSlots)
      {
        player.AssignEquipment(equipment);
        _logger.LogInformation("{player} acquired {equipment} (equipment)", player.Name, equipment.Name);
      }
      else
      {
        _explorationChoices.Discard(equipment);
        _logger.LogInformation($"{player.Name} could not acquire {equipment.Name} (equipment) because his inventory is full");
      }
    }

    private void AssignPotion(Potion potion, Player player)
    {
      player.AssignPotion(potion);
      _logger.LogInformation("{player} acquired {potion} (potion)", player.Name, potion.Name);
    }

    private void UsePotion(Potion potion, Player target, Player owner)
    {
      _logger.LogInformation("{player} is using {potion} (potion) on {target}", owner.Name, potion.Name, target.Name);
      ActivateAura(potion.Aura, target);
      owner.UnassignPotion(potion);
      _explorationChoices.Discard(potion);
    }

    private void UseSkill(Equipment equipment, Player target, Player owner)
    {

    }

    private void FightCreature(Creature creature, Player player)
    {
      _logger.LogInformation("{player} is fighting {creature} (creature)", player.Name, creature.Name);

      PerformAttack(creature, player, out AttackEvent attack);

      if (!player.IsDead && PerformAttack(player, creature, out AttackEvent ripost))
      {
        var targetDefeat = new TargetDefeatedEvent()
        {
          Source = player,
          Target = creature,
          Helpers = ripost.Helpers,
          FavorReward = creature.FavorReward
        };
        _eventManager.OnTargetDefeated(this, targetDefeat);
        _logger.LogInformation($"{creature.Name} was defeated by {player.Name}");
        RewardFavor(targetDefeat.FavorReward, player, $"defeating {creature.Name}");
        targetDefeat.Helpers.ForEach((Player helper) =>
        {
          PlayerAssistEvent assist = new PlayerAssistEvent()
          {
            Target = creature,
            Helper = helper
          };
          _eventManager.OnPlayerAssist(this, assist);          
          RewardFavor(assist.FavorReward, helper, $"assisting {player.Name}");
        });
      }

      _explorationChoices.Discard(creature);
    }

    private void FightPlayer(Player attacker, Player defender)
    {
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
        _eventManager.OnTargetDefeated(this, targetDefeated);
        _logger.LogInformation($"{targetDefeated.Target.Name} was defeated by {targetDefeated.Source.Name}");
        RewardFavor(targetDefeated.FavorReward, attacker, $"defeating {targetDefeated.Target.Name}");
      }
    }

    private bool PerformAttack(Character attacker, Character defender, out AttackEvent attack)
    {
      attack = new AttackEvent()
      {
        Attacker = attacker,
        Defender = defender,
        Strength = attacker.GetStrengthAgainst(defender),
        Defense = defender.GetDefenseAgainst(attacker)
      };
      _eventManager.OnAttack(this, attack);
      _logger.LogInformation($"{attacker.Name} is striking with {attack.Strength} STR against {defender.Name} with {attack.Defense} DEF");
      if (attack.Strength > attack.Defense)
      {
        if (defender is Player player)
        {
          int damageTaken = (attack.Strength - attack.Defense);
          _logger.LogInformation($"{attacker.Name} is inflicting {damageTaken} damage to {defender.Name}");
          player.ModifyHitPoints(damageTaken * -1);
        }
        return true;
      }
      else
      {
        _logger.LogInformation($"{attacker.Name}'s attack was ineffective against {defender.Name}");
      }
      return false;
    }

    private int RewardFavor(int amount, Player player, string reason)
    {
      if (amount > 0)
      {
        int availableReward = Math.Abs(MathHelper.NormalizeIncrement(_availableFavor, amount * -1, _availableFavor));
        if (availableReward > 0)
        {
          _availableFavor -= availableReward;
          player.ModifyFavor(availableReward);
          if (availableReward < amount)
          {
            _logger.LogInformation($"{player.Name} should have gained {amount} favor for {reason}, but only {availableReward} was left (now at {player.Favor})");
          }
          else
          {
            _logger.LogInformation($"{player.Name} gained {availableReward} favor for {reason} (now at {player.Favor})");
          }
          return availableReward;
        }
        else
        {
          _logger.LogInformation($"{player.Name} should have gained {amount} favor for {reason}, but there was nothing left");
        }        
      }
      return 0;
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

    private void ActivateAura(Aura aura, Character activator = null)
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

      activeAura.Targets.ForEach((Character character) =>
      {
        character.AssignTrait(aura.Trait);
        if (character is Player player)
        {
          var auraActivated = new AuraActivatedEvent()
          {
            Player = player,
            Aura = aura
          };
          _eventManager.OnAuraActivated(this, auraActivated);
          if (auraActivated.FavorModifier != 0)
          {
            RewardFavor(auraActivated.FavorModifier, player, "gaining aura");
          }
          if (auraActivated.HitPointsModifier != 0)
          {
            player.ModifyHitPoints(auraActivated.HitPointsModifier);
          }
          _logger.LogInformation("Aura {auraName} activated on {character}", activeAura.Aura.Trait.Name, character.Name);
        }
      });
      _activeAuras.Add(activeAura);
      if (activeAura.Aura.Expiration == AuraExpiration.Now)
      {
        DeactivateAura(activeAura);
      }
    }

    public void DeactivateAura(ActiveAura activeAura)
    {
      activeAura.Targets.ForEach((Character character) =>
      {
        character.UnassignTrait(activeAura.Aura.Trait);
        _logger.LogInformation("Aura {auraName} expired on {character}", activeAura.Aura.Trait.Name, character.Name);
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
    #endregion
  }
}