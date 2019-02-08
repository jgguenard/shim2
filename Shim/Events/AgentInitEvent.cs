using Shim.Entities;
using System;

namespace Shim.Events
{
  public class AgentInitEvent : EventArgs
  {
    public Agent NewAgent { get; set; }
  }

  public delegate void AgentInitEventHandler(object sender, AgentInitEvent e);
}