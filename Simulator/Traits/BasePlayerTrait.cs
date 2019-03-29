using Microsoft.Extensions.Logging;
using Raido.Shim;
using Raido.Shim.Entities;
using Raido.Shim.Events;

namespace Simulator.Traits
{
  public class BasePlayerTrait : Trait
  {
    private readonly ILogger _logger;

    public BasePlayerTrait(ILogger<BasePlayerTrait> logger, EventManager eventManager) : base()
    {
      _logger = logger;
      eventManager.PlayerInit += OnPlayerInit;
    }

    public void OnPlayerInit(object sender, PlayerInitEvent e)
    {
      if (e.Player.HasTrait(this))
      {
        e.BaseDefense += 1;
        _logger.LogInformation("Player {name} got his BaseStrenght increased by", e.Player.Name, 1);
      }
    }
  }
}
