using Raido.Shim.Entities;
using System;

namespace Raido.Shim.Events
{
  public class TurnActionEvent : EventArgs
  {
    public TurnActionType Type { get; set; }
    public TurnState State { get; set; }
    public Player Target { get; set; }
    public Entity Entity { get; set; }
  }

  public delegate void TurnActionEventHandler(object sender, TurnActionEvent e);
}