using Raido.Shim.Entities;
using System;
using System.Collections.Generic;

namespace Raido.Shim.Events
{
  public class ExplorationEvent : EventArgs
  {
    public Player Player { get; set; }
    public List<Entity> Choices { get; set; }
    public int PlayerChoiceIndex { get; set; }

    public ExplorationEvent()
    {
      Choices = new List<Entity>();
    }
  }

  public delegate void ExplorationEventHandler(object sender, ExplorationEvent e);
}