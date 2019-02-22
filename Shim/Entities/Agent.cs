using Shim.Events;
using System.Collections.Generic;
using System.Linq;

namespace Shim.Entities
{
  public class Agent : Target
  {
    public int MaxHitPoints { get; set; }
    public int AvailableHitPoints { get; set; }
    public int MaxActionPoints { get; set; }
    public int AvailableActionPoints { get; set; }
    public int MaxBonusActionPoints { get; set; }
    public int AvailableBonusActionPoints { get; set; }
    public int Favor { get; set; }
    public List<Item> Items { get; set; }
    public Tile Position { get; set; }
    public Tile PreviousPosition { get; set; }
    public List<Agent> DefeatedAgents { get; set; }
    public int PermanentItemCount
    {
      get
      {
        return Items.Where(item => item.IsPermanent).Count();
      }
    }

    public bool IsDead
    {
      get
      {
        return (AvailableHitPoints == 0);
      }
    }

    public Agent(string name) : base(name)
    {
      MaxHitPoints = 0;
      AvailableHitPoints = 0;
      MaxActionPoints = 0;
      AvailableActionPoints = 0;
      MaxBonusActionPoints = 0;
      AvailableBonusActionPoints = 0;
      Favor = 0;
      Items = new List<Item>();
      DefeatedAgents = new List<Agent>();
    }

    public override int GetStrengthAgainst(Target target)
    {
      int value = base.GetStrengthAgainst(target);
      Items.ForEach((Item item) =>
      {
        value += item.BaseStrength;
      });
      EvaluateStatEvent evaluation = new EvaluateStatEvent()
      {
        Source = this,
        Target = target,
        Value = value,
        Stat = StatType.Strength
      };
      EventManager.OnEvaluateStat(this, evaluation);
      return evaluation.Value;
    }

    public override int GetDefenseAgainst(Target target)
    {
      int value = base.GetDefenseAgainst(target);
      Items.ForEach((Item item) =>
      {
        value += item.BaseDefense;
      });
      EvaluateStatEvent evaluation = new EvaluateStatEvent()
      {
        Source = this,
        Target = target,
        Value = value,
        Stat = StatType.Defense
      };
      EventManager.OnEvaluateStat(this, evaluation);
      return value;
    }
  }
}
