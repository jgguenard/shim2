using Shim.Entities;
using System;

namespace Shim.Events
{
  public class TurnActionEvent : EventArgs
  {
    public GameState GameState { get; set; }
    public TurnActionType Type { get; set; }
    public Agent Source { get; set; }
    public Agent Target { get; set; }
    public Item Item { get; set; }
    public Tile Tile { get; set; }
    public SimulationParameters Parameters { get; set; }
  }

  public delegate void TurnActionEventHandler(object sender, TurnActionEvent e);
}