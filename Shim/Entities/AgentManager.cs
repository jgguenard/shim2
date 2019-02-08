using Shim.Library;

namespace Shim.Entities
{
  public class AgentManager
  {
    public void AssignItem(Item item, Agent agent)
    {
      agent.Items.Add(item);
      Logger.Log($"Item {item.Name} was added to agent {agent.Name}'s inventory");
    }

    public void UnassignItem(Item item, Agent agent)
    {
      agent.Items.Remove(item);
      Logger.Log($"Item {item.Name} was removed from agent {agent.Name}'s inventory");
    }

    public void AssignTrait(Trait trait, Agent agent)
    {
      if (trait.Stackable || !agent.HasTrait(trait))
      {
        agent.Traits.Add(trait);
        Logger.Log($"Trait {trait.Name} was granted to agent {agent.Name}");
      }
    }

    public void UnassignTrait(Trait trait, Agent agent)
    {
      if (agent.HasTrait(trait))
      {
        agent.Traits.Remove(trait);
        Logger.Log($"Trait {trait.Name} was revoked from agent {agent.Name}");
      }
    }

    public void ResetActionPoints(Agent agent)
    {
      agent.AvailableActionPoints = agent.MaxActionPoints;
      agent.AvailableBonusActionPoints = agent.MaxBonusActionPoints;
      Logger.Log($"Actions points of {agent.Name} are reset (value: {agent.AvailableActionPoints}/{agent.AvailableBonusActionPoints})");
    }

    public void ResetHitPoints(Agent agent)
    {
      agent.AvailableHitPoints = agent.MaxHitPoints;
      Logger.Log($"Hit Points of {agent.Name} are reset (value: {agent.AvailableHitPoints})");
    }

    public void SetPosition(Agent agent, Tile tile)
    {
      agent.Position = tile;
      Logger.Log($"Agent {agent.Name} moves to {tile.Name}");
    }

    public int ModifyActionPoints(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.AvailableActionPoints, amount, agent.MaxActionPoints);
      if (amountGained != 0)
      {
        agent.AvailableActionPoints += amountGained;
        if (amountGained > 0)
        {
          Logger.Log($"Agent {agent.Name} gained {amountGained} AP (value: {agent.AvailableActionPoints})");
        }
        else
        {
          Logger.Log($"Agent {agent.Name} lost {amountGained * -1} AP (value: {agent.AvailableActionPoints})");
        }
      }
      return amountGained;
    }

    public int ModifyBonusActionPoints(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.AvailableBonusActionPoints, amount, agent.MaxBonusActionPoints);
      if (amountGained != 0)
      {
        agent.AvailableBonusActionPoints += amountGained;
        if (amountGained > 0)
        {
          Logger.Log($"Agent {agent.Name} gained {amountGained} bonus AP (value: {agent.AvailableBonusActionPoints})");
        }
        else
        {
          Logger.Log($"Agent {agent.Name} lost {amountGained * -1} bonus AP (value: {agent.AvailableBonusActionPoints})");
        }
      }
      return amountGained;
    }

    public int ModifyHitPoints(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.AvailableHitPoints, amount, agent.MaxHitPoints);
      if (amountGained != 0)
      {
        agent.AvailableHitPoints += amountGained;
        if (amount > 0)
        {
          Logger.Log($"Agent {agent.Name} gained {amountGained} HP (value: {agent.AvailableHitPoints})");
        }
        else
        {
          Logger.Log($"Agent {agent.Name} lost {amountGained} HP (value: {agent.AvailableHitPoints})");
        }
      }
      return amountGained;
    }

    public int ModifyFavor(Agent agent, int amount)
    {
      int amountGained = ValidateStat(agent.Favor, amount, 9999);
      if (amountGained != 0)
      {
        agent.Favor += amountGained;
        if (amount > 0)
        {
          Logger.Log($"Agent {agent.Name} gained {amountGained} favor (value: {agent.Favor})");
        }
        else
        {
          Logger.Log($"Agent {agent.Name} lost {amountGained} favor (value: {agent.Favor})");
        }
      }
      return amountGained;
    }

    private int ValidateStat(int value, int increment, int maxValue)
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