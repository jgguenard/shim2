using Shim.Events;
using Shim.Library;

namespace Shim.Entities
{
  public static class AgentManager
  {
    public static void AssignItem(Item item, Agent agent)
    {
      agent.Items.Add(item);
      HistoryManager.LogItem(item, agent);
    }

    public static void UnassignItem(Item item, Agent agent)
    {
      agent.Items.Remove(item);
      HistoryManager.LogItem(item, agent, true);
    }

    public static void AssignTrait(Trait trait, Agent agent)
    {
      if (trait.Stackable || !agent.HasTrait(trait))
      {
        agent.Traits.Add(trait);
        HistoryManager.LogTrait(trait, agent);
      }
    }

    public static void UnassignTrait(Trait trait, Agent agent)
    {
      if (agent.HasTrait(trait))
      {
        agent.Traits.Remove(trait);
        HistoryManager.LogTrait(trait, agent, true);
      }
    }

    public static void ResetActionPoints(Agent agent)
    {
      agent.AvailableActionPoints = agent.MaxActionPoints;
      agent.AvailableBonusActionPoints = agent.MaxBonusActionPoints;
      HistoryManager.LogMessage($"Actions points are reset (value: {agent.AvailableActionPoints} AP / {agent.AvailableBonusActionPoints}) bonus AP", agent);
    }

    public static void ResetHitPoints(Agent agent)
    {
      agent.AvailableHitPoints = agent.MaxHitPoints;
      HistoryManager.LogMessage($"Hit Points are reset (value: {agent.AvailableHitPoints})", agent);
    }

    public static void SetPosition(Agent agent, Tile tile)
    {
      if (agent.Position != null)
      {
        agent.PreviousPosition = agent.Position;
      }
      agent.Position = tile;
      HistoryManager.LogMessage($"Move to {tile.Name} ({tile.Type.ToString()})", agent);
    }

    public static int ModifyActionPoints(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.AvailableActionPoints, amount, agent.MaxActionPoints);
      if (amountGained != 0)
      {
        agent.AvailableActionPoints += amountGained;
        if (amountGained > 0)
        {
          HistoryManager.LogMessage($"{amountGained} AP gained (value: {agent.AvailableActionPoints})", agent);
        }
        else
        {
          HistoryManager.LogMessage($"{amountGained * -1} AP lost (value: {agent.AvailableActionPoints})", agent);
        }
      }
      return amountGained;
    }

    public static int ModifyBonusActionPoints(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.AvailableBonusActionPoints, amount, agent.MaxBonusActionPoints);
      if (amountGained != 0)
      {
        agent.AvailableBonusActionPoints += amountGained;
        if (amountGained > 0)
        {
          HistoryManager.LogMessage($"{amountGained} bonus AP gained (value: {agent.AvailableBonusActionPoints})", agent);
        }
        else
        {
          HistoryManager.LogMessage($"{amountGained * -1} bonus AP lost (value: {agent.AvailableBonusActionPoints})", agent);
        }
      }
      return amountGained;
    }

    public static int ModifyHitPoints(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.AvailableHitPoints, amount, agent.MaxHitPoints);
      if (amountGained != 0)
      {
        agent.AvailableHitPoints += amountGained;
        if (amount > 0)
        {
          HistoryManager.LogMessage($"{amountGained} HP gained (value: {agent.AvailableHitPoints})", agent);
        }
        else
        {
          HistoryManager.LogMessage($"{amountGained * -1} HP lost (value: {agent.AvailableHitPoints})", agent);
        }
      }
      return amountGained;
    }

    public static int ModifyFavor(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.Favor, amount, 9999);
      if (amountGained != 0)
      {
        if (amount > 0)
        {
          FavorGainedEvent favorGained = new FavorGainedEvent()
          {
            Agent = agent,
            Amount = amount
          };
          EventManager.OnFavorGained(agent, favorGained);
          agent.Favor += favorGained.Amount;
          HistoryManager.LogMessage($"{amountGained} favor gained (value: {agent.Favor})", agent);
        }
        else
        {
          agent.Favor += amountGained;
          HistoryManager.LogMessage($"{amountGained * -1} favor lost (value: {agent.Favor})", agent);
        }
      }
      return amountGained;
    }

    public static void ModifyBaseStrength(Agent agent, int amount)
    {
      agent.BaseStrength += amount;
    }

    public static void ModifyBaseDefense(Agent agent, int amount)
    {
      agent.BaseStrength += amount;
    }

    public static void ModifyMaxBonusActionPoints(Agent agent, int amount)
    {
      agent.MaxBonusActionPoints += amount;
    }

    public static void ModifyMaxActionPoints(Agent agent, int amount)
    {
      agent.MaxActionPoints += amount;
    }

    public static void RegisterDuelVictory(Agent attacker, Agent defender)
    {
      attacker.DefeatedAgents.Add(defender);
      HistoryManager.LogMessage($"Duel between {attacker.Name} and {defender.Name} was registered");
    }

    private static int ValidateStat(int value, int increment, int maxValue)
    {
      int nextValue = value + increment;
      if (nextValue < 0)
      {
        return 0;
      }
      if (nextValue > maxValue)
      {
        return nextValue - maxValue;
      }
      return increment;
    }
  }
}