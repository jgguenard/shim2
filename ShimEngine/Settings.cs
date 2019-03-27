﻿namespace Raido.Shim
{
  public class Settings
  {
    public int MaxRounds { get; set; }
    public int EquipmentSlots { get; set; }
    public int BlessingSlots { get; set; }
    public int MinAgents { get; set; }
    public int MaxAgents { get; set; }   
    public int MaxActionsPerTurn { get; set; }
    public int MaxHitPoints { get; set; }
    public int DefaultMaxActionPoints { get; set; }
    public int DefaultBaseDefense { get; set; }
    public int DefaultBaseStrength { get; set; }

    public Settings()
    {
      MaxRounds = int.MaxValue;
      EquipmentSlots = 0;
      BlessingSlots = 0;
      MinAgents = 2;
      MaxAgents = int.MaxValue;
      MaxActionsPerTurn = int.MaxValue;
      MaxHitPoints = int.MaxValue;
      DefaultMaxActionPoints = int.MaxValue;
      DefaultBaseDefense = 0;
      DefaultBaseStrength = 0;
    }
  }
}