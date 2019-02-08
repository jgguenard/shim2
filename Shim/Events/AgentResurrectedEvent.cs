using Shim.Entities;
using System;

namespace Shim.Events
{
  public class AgentResurrectedEvent : EventArgs
  {
    public Agent Agent { get; set; }
    public int HitPoints { get; set; }
  }

  public delegate void AgentResurrectedEventHandler(object sender, AgentResurrectedEvent e);
}