using Shim.Entities;
using System;

namespace Shim.Events
{
  public class MoveEvent : EventArgs
  {
    public Agent Agent { get; set; }
    public int ActionPointCost { get; set; }
    public Tile Tile { get; set; }
  }

  public delegate void MoveEventHandler(object sender, MoveEvent e);
}