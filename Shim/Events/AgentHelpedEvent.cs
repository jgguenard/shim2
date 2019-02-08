using Shim.Entities;
using System;

namespace Shim.Events
{
  public class AgentHelpedEvent : EventArgs
  {
    public Agent Attacker;
    public Agent Helper;
    public Target Target;
    public int FavorReward;
  }

  public delegate void AgentHelpedEventHandler(object sender, AgentHelpedEvent e);
}