using Shim.Entities;
using System;

namespace Shim.Events
{
  public class EvaluateItemUseEvent : EventArgs
  {
    public Agent Source { get; set; }
    public Agent Target { get; set; }
    public Item Item { get; set; }
    public ItemUseContext Context { get; set; }
    public int Score { get; set; }

    public EvaluateItemUseEvent()
    {
      Score = 0;
    }
  }

  public delegate void EvaluateItemUseEventHandler(object sender, EvaluateItemUseEvent e);
}