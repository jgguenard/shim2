using Microsoft.Extensions.Logging;
using Raido.Shim;
using Raido.Shim.Entities;
using Raido.Shim.Events;

namespace Simulator.Traits
{
  public class BasePlayerTrait : Trait
  {
    public const int VALUE_CREATURE_CAN_DEFEAT_ALONE = 600;
    public const int VALUE_INVENTORY_NOT_FULL = 500;
    public const int VALUE_FIRST_QUEST = 400;
    public const int VALUE_POTION = 350;
    public const int VALUE_CREATURE_NEED_HELP = 300;
    public const int VALUE_ADDITIONAL_QUEST = 250;
    public const int VALUE_INVENTORY_FULL = 100;
    public const int VALUE_CREATURE_CANNOT_SURVIVE = 0;

    private readonly ILogger _logger;
    private readonly Settings _settings;

    public BasePlayerTrait(ILogger<BasePlayerTrait> logger, EventManager eventManager, Settings settings) : base()
    {
      _logger = logger;
      _settings = settings;
      eventManager.PlayerInit += OnPlayerInit;
      eventManager.TurnAction += OnTurnAction;
      eventManager.Exploration += OnExploration;
    }

    public void OnPlayerInit(object sender, PlayerInitEvent e)
    {
      if (e.Player.HasTrait(this))
      {
        e.BaseDefense += 1;
        _logger.LogInformation("Player {name} gained +{amount} to his base DEF (now at {total})", e.Player.Name, 1, e.BaseDefense);
      }
    }

    public void OnTurnAction(object sender, TurnActionEvent e)
    {
      if (e.State.Player.HasTrait(this))
      {
        // todo: real decision making
        if (e.State.CanExplore)
        {
          e.Type = TurnActionType.Explore;
        }
        else
        {
          e.Type = TurnActionType.Stop;
        }
      }
    }

    public void OnExploration(object sender, ExplorationEvent e)
    {
      if (e.Player.HasTrait(this))
      {
        int bestValue = 0;
        for(var i = 0; i < e.Choices.Count; i++)
        {
          Entity choice = e.Choices[i];
          int value = 0;

          if (choice is Equipment equipment)
          {
            value = (e.Player.Equipment.Count < _settings.EquipmentSlots) ? VALUE_INVENTORY_NOT_FULL : VALUE_INVENTORY_FULL;
          }
          else if (choice is Potion potion)
          {
            value = VALUE_POTION;
          }
          else if (choice is Creature creature)
          {
            int damageDealt = e.Player.GetStrengthAgainst(creature) - creature.GetDefenseAgainst(e.Player);
            int damageTaken = creature.GetStrengthAgainst(e.Player) - e.Player.GetDefenseAgainst(creature);
            bool canDefeatAlone = (damageDealt > 0);
            bool canSurvive = (damageTaken <= 0 || damageTaken < e.Player.HitPoints);
            if (!canSurvive)
            {
              value = VALUE_CREATURE_CANNOT_SURVIVE;
            }
            else
            {
              value = canDefeatAlone ? VALUE_CREATURE_CAN_DEFEAT_ALONE : VALUE_CREATURE_NEED_HELP;
            }
          }
          else if (choice is Quest quest)
          {
            value = (e.Player.Quests.Count > 0) ? VALUE_ADDITIONAL_QUEST : VALUE_FIRST_QUEST;
          }

          if (value > bestValue)
          {
            bestValue = value;
            e.PlayerChoiceIndex = i;
          }
        }
      }
    }
  }
}
