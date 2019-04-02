using Raido.Shim.Entities;
using System;

namespace Raido.Shim.Events
{
  public class PlayerAssistEvent : EventArgs
  {
    public Player Attacker;
    public Player Helper;
    public Character Target;
    public int FavorReward;
  }

  public delegate void PlayerAssistEventHandler(object sender, PlayerAssistEvent e);
}