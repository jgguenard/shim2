using Shim.Entities;
using System;
using System.Collections.Generic;

namespace Shim.Events
{
  public class TargetDefeatedEvent : EventArgs
  {
    public Agent Source;
    public List<Agent> Helpers;
    public Target Target;
    public int FavorReward;

    public TargetDefeatedEvent() : base()
    {
      Helpers = new List<Agent>();
      FavorReward = 0;
    }
  }

  public delegate void TargetDefeatedEventHandler(object sender, TargetDefeatedEvent e);
}