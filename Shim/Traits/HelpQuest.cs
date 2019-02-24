using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class HelpQuest : Trait
  {
    public const int FAVOR_REWARD = 1;

    public HelpQuest() : base()
    {
      EventManager.AgentHelped += OnAgentHelped;
    }

    public void OnAgentHelped(object sender, AgentHelpedEvent e)
    {
      if (e.Helper.HasTrait(this))
      {
        e.FavorReward += FAVOR_REWARD;
      }
    }
  }
}