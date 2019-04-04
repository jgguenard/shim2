using Microsoft.Extensions.Logging;
using Raido.Shim;
using Raido.Shim.Entities;
using Raido.Shim.Events;

namespace Simulator.Traits
{
  public class DummyPotionTrait : Trait
  {
    public const int HP_GAIN = 2;

    private readonly ILogger _logger;

    public DummyPotionTrait(ILogger<DummyPotionTrait> logger, EventManager eventManager) : base()
    {
      _logger = logger;
      eventManager.AuraActivated += OnAuraActivated;
    }

    public void OnAuraActivated(object sender, AuraActivatedEvent e)
    {
      if (e.Aura.Trait == this)
      {
        e.HitPointsModifier = HP_GAIN;
        _logger.LogInformation($"{e.Player.Name} is recovering {HP_GAIN} hit points");
      }
    }
  }
}