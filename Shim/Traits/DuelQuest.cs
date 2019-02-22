using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class DuelQuest : Trait
  {
    public const int FAVOR_REWARD_DIFFERENT_TARGET = 4;
    public DuelQuest() : base()
    {
      EventManager.TargetDefeated += OnTargetDefeated;
    }

    public void OnTargetDefeated(object sender, TargetDefeatedEvent e)
    {
      if (e.Source is Agent && e.Target is Agent && !e.Source.DefeatedAgents.Contains((Agent) e.Target))
      {
        Log(this, $"{e.Source.Name} will be rewarded {FAVOR_REWARD_DIFFERENT_TARGET} favor for defeating {e.Target.Name}");
        e.FavorReward += FAVOR_REWARD_DIFFERENT_TARGET;
      }
    }
  }
}