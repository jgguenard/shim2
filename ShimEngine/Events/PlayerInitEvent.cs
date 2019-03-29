using Raido.Shim.Entities;
using System;

namespace Raido.Shim.Events
{
  public class PlayerInitEvent : EventArgs
  {
    public Player Player { get; set; }
    public int BaseStrength { get; set; }
    public int BaseDefense { get; set; }
    public int MaxHitPoints { get; set; }

    public PlayerInitEvent()
    {
      BaseStrength = 0;
      BaseDefense = 0;
      MaxHitPoints = 0;
    }
  }

  public delegate void PlayerInitEventHandler(object sender, PlayerInitEvent e);
}