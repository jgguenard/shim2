using Shim.Entities;
using System;

namespace Shim.Events
{
  public class AgentInitEvent : EventArgs
  {
    public Agent Agent { get; set; }
    public int MaxActionPoints { get; set; }
    public int MaxBonusActionPoints { get; set; }
    public int BaseStrength { get; set; }
    public int BaseDefense { get; set; }

    public AgentInitEvent()
    {
      MaxActionPoints = 0;
      MaxBonusActionPoints = 0;
      BaseStrength = 0;
      BaseDefense = 0;
    }
  }

  public delegate void AgentInitEventHandler(object sender, AgentInitEvent e);
}