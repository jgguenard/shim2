using Raido.Shim.Entities;
using System;

namespace Raido.Shim.Events
{
  public class AuraActivatedEvent : EventArgs
  {
    public Aura Aura { get; set; }
    public Player Player { get; set; }
    public int FavorModifier { get; set; }
    public int HitPointsModifier { get; set; }

    public AuraActivatedEvent()
    {
      FavorModifier = 0;
      HitPointsModifier = 0;
    }
  }

  public delegate void AuraActivatedEventHandler(object sender, AuraActivatedEvent e);
}