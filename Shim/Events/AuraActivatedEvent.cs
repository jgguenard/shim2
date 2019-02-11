using Shim.Entities;
using System;

namespace Shim.Events
{
  public class AuraActivatedEvent : EventArgs
  {
    public Aura Aura { get; set; }
    public Agent Agent { get; set; }
    public int FavorModifier { get; set; }
    public int HitPointsModifier { get; set; }
    public int ActionPointsModifier { get; set; }
    public int BonusActionPointsModifier { get; set; }
    public AuraActivatedEvent() {
      FavorModifier = 0;
      HitPointsModifier = 0;
      ActionPointsModifier = 0;
      BonusActionPointsModifier = 0;
    }
  }

  public delegate void AuraActivatedEventHandler(object sender, AuraActivatedEvent e);
}