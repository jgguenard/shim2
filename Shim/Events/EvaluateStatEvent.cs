using Shim.Entities;
using System;

namespace Shim.Events
{
  public class EvaluateStatEvent : EventArgs
  {
    public StatType Stat { get; set; }
    public int Value { get; set; }
    public Target Source { get; set; }
    public Target Target { get; set; }
  }

  public delegate void EvaluateStatEventHandler(object sender, EvaluateStatEvent e);
}