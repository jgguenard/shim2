using Microsoft.Extensions.Logging;
using Raido.Shim;
using Raido.Shim.Entities;
using Raido.Shim.Events;

namespace Simulator.Traits
{
  public class DummyQuestTrait : Trait
  {
    public const int FAVOR_GAIN = 1;

    private readonly ILogger _logger;

    public DummyQuestTrait(ILogger<DummyQuestTrait> logger, EventManager eventManager)
    {
      _logger = logger;
      eventManager.TargetDefeated += OnTargetDefeated;
    }

    public void OnTargetDefeated(object sender, TargetDefeatedEvent e)
    {
      if (e.Source.HasTrait(this) && e.Target is Creature && e.Helpers.Count == 0)
      {
        e.FavorReward += FAVOR_GAIN;
        _logger.LogInformation($"{e.Source.Name} will earn an extra {FAVOR_GAIN} favor for defeating a creature alone");
      }
    }
  }
}