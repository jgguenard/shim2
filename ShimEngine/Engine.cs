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
    private int _availableFavor;
    private Guid _sid;
    private readonly Deck<Aura> _gameEvents;
    private readonly Deck<Entity> _cards;
    private readonly List<ActiveAura> _activeAuras;
    private readonly List<Player> _players;
    private readonly List<Summary> _summaries;

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
      _cards = new Deck<Entity>("Exploration");
      _activeAuras = new List<ActiveAura>();
      _summaries = new List<Summary>();
      _availableFavor = 0;
      _ended = false;
      _round = 0;
      _turn = 0;
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
    }

    public void AddGameEvent(Trait trait)
    {
      _gameEvents.Add(new Aura()
      {
        Source = AuraSource.GameEvent,
        Expiration = AuraExpiration.EndOfRound,
        Scope = AuraScope.All,
        Trait = trait
      });
    }

    public void AddCard(Entity entity, int copies = 1)
    {
      for (int i = 0; i < copies; i++)
      {
        _cards.Add(entity);
      }
    }

    public void Run(int simulations = 1)
    {
      // integrity checks
      if (_players.Count < _settings.MinPlayers || _players.Count > _settings.MaxPlayers)
      {
        throw new Exception($"Expected between {_settings.MinPlayers} and {_settings.MaxPlayers} players but got {_players.Count}");
      }

      _logger.LogInformation("============= SHIM ============");
      _logger.LogInformation($"Running {simulations} simulations with {_players.Count} players and {_settings.AvailableFavor(_players.Count)} favor");

      _players.ForEach(p => InitializePlayer(p));

      _summaries.Clear();

      for (int i = 0; i < simulations; i++)
      {
        ResetState();

        _logger.LogInformation("============= SIMULATION #{i} ({id}) - BEGIN ============", i + 1, _sid);
        try
        {
          RunSimulation();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, ex.Message);
        }
        GenerateSummary();
        _logger.LogInformation("============= SIMULATION #{i} ({id}) - END ============", i + 1, _sid);
      }
      _logger.LogInformation("============= END OF SIMULATIONS ============");
    }
    #endregion

    #region Private Methods
    private void GenerateSummary()
    {
      Summary summary = new Summary()
      {
        PlayerCount = _players.Count,
        Rounds = _round
      };
      _players.ForEach(p => summary.FavorByPlayer[p.Name] = p.Favor);
      _summaries.Add(summary);

      var favorSegments = new List<string>();
      foreach (var key in summary.FavorByPlayer.Keys)
      {
        favorSegments.Add($"{key}={summary.FavorByPlayer[key]}");
      }
      _logger.LogInformation($"SUMMARY: Players: {summary.PlayerCount} | Rounds played: {summary.Rounds} | Favor: {string.Join(",", favorSegments)}");
    }

    private void ResetState()
    {
      _players.ForEach(player =>
      {
        player.HitPoints = player.MaxHitPoints;
        player.Favor = 0;
        player.Equipment.ForEach(e => _cards.Discard(e));
        player.Potions.ForEach(e => _cards.Discard(e));
        player.Quests.ForEach(e => _cards.Discard(e));
        player.Equipment.Clear();
        player.Potions.Clear();
        player.Quests.Clear();
      });
      for (int i = 0; i < _activeAuras.Count; i++)
      {
        DeactivateAura(_activeAuras[i]);
      }
      _gameEvents.Shuffle();
      _cards.Shuffle();
      _availableFavor = _settings.AvailableFavor(_players.Count);
      _ended = false;
      _round = 0;
      _turn = 0;
      _sid = Guid.NewGuid();
    }

    private void RunSimulation()
    {
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

        // Execute turns
        for (_turn = 1; _turn <= _players.Count; _turn++)
        {
          ExecuteTurn();
          CheckActiveAurasExpiration();
          CheckEndCondition();
        }
      }
    }

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
        Player = TurnPlayer,
        DiscardedChoices = _cards.Discarded(TurnPlayer.Name)
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
          case TurnActionType.Visit:
            Visit(action.Entity);
            turnState.CanExplore = false;
            break;
          case TurnActionType.Duel:
            Duel(TurnPlayer, action.Target);
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
      ExplorationEvent exploration = new ExplorationEvent()
      {
        Player = TurnPlayer,
        Choices = new List<Entity>(),
        PlayerChoiceIndex = 0
      };
      for (int i = 0; i < _settings.ExplorationChoices; i++)
      {
        exploration.Choices.Add(_cards.Draw());
      }
      _eventManager.OnExploration(this, exploration);
      for (int i = 0; i < exploration.Choices.Count; i++)
      {
        if (i != exploration.PlayerChoiceIndex)
        {
          _cards.Discard(exploration.Choices[i], TurnPlayer.Name);
        }
      }
      var playerChoice = exploration.Choices[exploration.PlayerChoiceIndex];
      Visit(playerChoice);
    }

    private void Visit(Entity playerChoice)
    {
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
      else if (playerChoice is Quest quest)
      {
        AssignQuest(quest, TurnPlayer);
      }
    }

    private void DrawEvent()
    {
      Aura gameEvent = _gameEvents.Draw();
      _logger.LogInformation("Activating game event: {e}", gameEvent.Trait.Name);
      ActivateAura(gameEvent);
    }

    private void AssignQuest(Quest quest, Player player)
    {
      _logger.LogInformation("{player} is assigned to a new quest: {quest}", player.Name, quest.Name);
      player.Quests.Add(quest);
      ActivateAura(quest.Aura, player);
    }

    private void AssignEquipment(Equipment equipment, Player player)
    {
      if (player.Equipment.Count < _settings.EquipmentSlots)
      {
        player.Equipment.Add(equipment);
        _logger.LogInformation("{player} acquired {equipment} (equipment)", player.Name, equipment.Name);
      }
      else
      {
        _cards.Discard(equipment);
        _logger.LogInformation($"{player.Name} could not acquire {equipment.Name} (equipment) because his inventory is full");
      }
    }

    private void AssignPotion(Potion potion, Player player)
    {
      player.Potions.Add(potion);
      _logger.LogInformation("{player} acquired {potion} (potion)", player.Name, potion.Name);
    }

    private void UsePotion(Potion potion, Player target, Player owner)
    {
      _logger.LogInformation("{player} is using {potion} (potion) on {target}", owner.Name, potion.Name, target.Name);
      ActivateAura(potion.Aura, target);
      owner.Potions.Remove(potion);
      _cards.Discard(potion);
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
        _logger.LogInformation($"{creature.Name} was defeated by {player.Name}");
        var targetDefeat = new TargetDefeatedEvent()
        {
          Source = player,
          Target = creature,
          Helpers = ripost.Helpers,
          FavorReward = creature.FavorReward
        };
        _eventManager.OnTargetDefeated(this, targetDefeat);
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
      else
      {
        _logger.LogInformation($"{player.Name} was defeated by {creature.Name}");
      }

      _cards.Discard(creature);
    }

    private void Duel(Player attacker, Player defender)
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
          player.ModifyHitPoints(damageTaken * -1);
          _logger.LogInformation($"{attacker.Name} has inflicted {damageTaken} damage to {defender.Name} ({player.HitPoints} HP left)");
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
    }
    #endregion
  }
}