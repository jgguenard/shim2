using Raido.Shim.Entities;
using System;
using System.Collections.Generic;

namespace Raido.Shim.Events
{
  public class AttackEvent : EventArgs
  {
    public List<Player> Helpers;
    public Character Attacker;
    public Character Defender;
    public int Strength;
    public int Defense;

    public AttackEvent() : base()
    {
      Helpers = new List<Player>();
    }
  }

  public delegate void AttackEventHandler(object sender, AttackEvent e);
}