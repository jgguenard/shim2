using Shim.Entities;
using System;

namespace Shim.Events
{
  public class FavorGainedEvent : EventArgs
  {
    public Agent Agent { get; set; }
    public int Amount { get; set; }
  }

  public delegate void FavorGainedEventHandler(object sender, FavorGainedEvent e);
}