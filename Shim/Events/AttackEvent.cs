using Shim.Entities;
using System;
using System.Collections.Generic;

namespace Shim.Events
{
  public class AttackEvent : EventArgs
  {
    public List<Agent> Helpers;
    public Target Attacker;
    public Target Defender;
    public int Strength;
    public int Defense;

    public AttackEvent() : base()
    {
      Helpers = new List<Agent>();
    }
  }

  public delegate void AttackEventHandler(object sender, AttackEvent e);
}