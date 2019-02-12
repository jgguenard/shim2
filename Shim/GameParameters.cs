using System.Collections.Generic;

namespace Shim
{
  public class GameParameters
  {
    public int MaxRounds { get; set; }
    public int MaxPermanentItemsPerAgent { get; set; }
    public int MaxActiveBlessingPerAgent { get; set; }
    public int MinAgents { get; set; }
    public int MaxAgents { get; set; }
    public bool StartingItemEnabled { get; set; }
    public int HitPointsRestoredByHealer { get; set; }
    public int BaseMovementCost { get; set; }
    public int HitPointsAfterResurrection { get; set; }
    public int MaxActionsPerTurn { get; set; }
    public int MaxHitPoints { get; set; }
    public int DefaultMaxActionPoints { get; set; }
    public int DefaultMaxBonusActionPoints { get; set; }
    public int DefaultBaseDefense { get; set; }
    public int DefaultBaseStrength { get; set; }
    public string[] StartingTiles { get; set; }
  }
}