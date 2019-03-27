﻿using Raido.Shim.Library;
using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public class Character : Agent
  {
    public int MaxHitPoints { get; set; }
    public int HitPoints { get; set; }
    public int MaxActionPoints { get; set; }
    public int ActionPoints { get; set; }
    public int Favor { get; set; }
    public List<Equipment> Equipment { get; set; }
    public List<Potion> Potions { get; set; }

    public Character(string name) : base(name)
    {
      MaxHitPoints = 0;
      HitPoints = 0;
      MaxActionPoints = 0;
      ActionPoints = 0;
      Favor = 0;
      Equipment = new List<Equipment>();
      Potions = new List<Potion>();
    }
    public bool IsDead
    {
      get
      {
        return (HitPoints == 0);
      }
    }
    public override int GetStrengthAgainst(Agent target)
    {
      int value = base.GetStrengthAgainst(target);
      Equipment.ForEach((Equipment equipment) =>
      {
        value += equipment.GetStrengthAgainst(target);
      });
      return value;
    }

    public override int GetDefenseAgainst(Agent target)
    {
      int value = base.GetDefenseAgainst(target);
      Equipment.ForEach((Equipment equipment) =>
      {
        value += equipment.GetStrengthAgainst(target);
      });
      return value;
    }

    public void ResetActionPoints(Character character)
    {
      ActionPoints = MaxActionPoints;
    }
    public void ResetHitPoints(Character character)
    {
      HitPoints = MaxHitPoints;
    }
    public int ModifyActionPoints(int amount)
    {
      int amountGained = MathHelper.NormalizeIncrement(ActionPoints, amount, MaxActionPoints);
      if (amountGained != 0)
      {
        ActionPoints += amountGained;
        if (amountGained > 0)
        {
          // log gain
        }
        else
        {
          // log loss
        }
      }
      return amountGained;
    }
    public int ModifyHitPoints(int amount)
    {
      int amountGained = MathHelper.NormalizeIncrement(HitPoints, amount, MaxHitPoints);
      if (amountGained != 0)
      {
        HitPoints += amountGained;
        if (amountGained > 0)
        {
          // log gain
        }
        else
        {
          // log loss
        }
      }
      return amountGained;
    }
    public int ModifyFavor(int amount)
    {
      int amountGained = MathHelper.NormalizeIncrement(Favor, amount, int.MaxValue);
      if (amountGained != 0)
      {
        Favor += amountGained;
        if (amountGained > 0)
        {
          // log gain
        }
        else
        {
          // log loss
        }
      }
      return amountGained;
    }
    public void SetBaseStrength(int amount)
    {
      BaseStrength = amount;
    }
    public void SetBaseDefense(int amount)
    {
      BaseDefense = amount;
    }
    public void AssignTrait(Trait trait)
    {
      if (!HasTrait(trait))
      {
        Traits.Add(trait);
      }
    }
    public void UnassignTrait(Trait trait)
    {
      Traits.Remove(trait);
    }
    public void AssignEquipment(Equipment equipment)
    {
      Equipment.Add(equipment);
    }
    public void UnassignEquipment(Equipment equipment)
    {
      Equipment.Remove(equipment);
    }
    public void AssignPotion(Potion potion)
    {
      Potions.Add(potion);
    }
    public void UnassignPotion(Potion potion)
    {
      Potions.Remove(potion);
    }
  }
}