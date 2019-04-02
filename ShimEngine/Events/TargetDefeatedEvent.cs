using Raido.Shim.Entities;
using System;
using System.Collections.Generic;

namespace Raido.Shim.Events
{
  public class TargetDefeatedEvent : EventArgs
  {
    public Player Source;
    public List<Player> Helpers;
    public Character Target;
    public int FavorReward;

    public TargetDefeatedEvent() : base()
    {
      Helpers = new List<Player>();
      FavorReward = 0;
    }
  }

  public delegate void TargetDefeatedEventHandler(object sender, TargetDefeatedEvent e);
}