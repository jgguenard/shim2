using Raido.Shim.Library;
using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public class Player : Character
  {
    public int MaxHitPoints { get; set; }
    public int HitPoints { get; set; }
    public int Favor { get; set; }
    public List<Equipment> Equipment { get; set; }
    public List<Potion> Potions { get; set; }
    public List<Quest> Quests { get; set; }

    public Player(string name) : base(name)
    {
      MaxHitPoints = 0;
      HitPoints = 0;
      Favor = 0;
      Equipment = new List<Equipment>();
      Potions = new List<Potion>();
      Quests = new List<Quest>();
    }
    public bool IsDead
    {
      get
      {
        return (HitPoints == 0);
      }
    }
    public override int GetStrengthAgainst(Character target)
    {
      int value = base.GetStrengthAgainst(target);
      Equipment.ForEach((Equipment equipment) =>
      {
        value += equipment.GetStrengthAgainst(target);
      });
      return value;
    }

    public override int GetDefenseAgainst(Character target)
    {
      int value = base.GetDefenseAgainst(target);
      Equipment.ForEach((Equipment equipment) =>
      {
        value += equipment.GetDefenseAgainst(target);
      });
      return value;
    }

    public int ModifyHitPoints(int amount)
    {
      int amountGained = MathHelper.NormalizeIncrement(HitPoints, amount, MaxHitPoints);
      if (amountGained != 0)
      {
        HitPoints += amountGained;
      }
      return amountGained;
    }
    public int ModifyFavor(int amount)
    {
      int amountGained = MathHelper.NormalizeIncrement(Favor, amount, int.MaxValue);
      if (amountGained != 0)
      {
        Favor += amountGained;
      }
      return amountGained;
    }
  }
}
