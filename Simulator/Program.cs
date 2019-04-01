using Microsoft.Extensions.DependencyInjection;
using Raido.Shim;
using Raido.Shim.Entities;
using Serilog;
using Simulator.Traits;
using System;

namespace Simulator
{
  class Program
  {
    static void Main(string[] args)
    {
      Settings settings = new Settings
      {
        MinPlayers = 2,
        MaxPlayers = 6,
        BlessingSlots = 1,
        EquipmentSlots = 3,
        DefaultMaxActionPoints = 3,
        MaxHitPoints = 10,
        MaxRounds = 10,
        ExplorationChoices = 3,
        AvailableFavor = (playerCount) => playerCount * 10
      };

      Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.File("output.yaml", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}")
        .CreateLogger();

      ServiceProvider services = new ServiceCollection()
        .AddLogging(configure => configure.AddSerilog(dispose: true))
        .AddSingleton(settings)
        .AddSingleton<EventManager>()
        .AddSingleton<Engine>()
        .AddSingleton<BasePlayerTrait>()
        .AddSingleton<DummyGameEventTrait>()
        .BuildServiceProvider();

      var engine = services.GetService<Engine>();

      engine.AddPlayer("Wolf", new Trait[] {
        services.GetService<BasePlayerTrait>()
      });
      engine.AddPlayer("Bear", new Trait[] {
        services.GetService<BasePlayerTrait>()
      });
      engine.AddPlayer("Stag", new Trait[] {
        services.GetService<BasePlayerTrait>()
      });
      engine.AddPlayer("Eagle", new Trait[] {
        services.GetService<BasePlayerTrait>()
      });
      engine.AddPlayer("Monkey", new Trait[] {
        services.GetService<BasePlayerTrait>()
      });
      engine.AddPlayer("Panther", new Trait[] {
        services.GetService<BasePlayerTrait>()
      });

      engine.AddExplorationChoice(new Equipment("Armor") {
        BaseDefense = 1
      }, 5);

      engine.AddExplorationChoice(new Equipment("Weapon")
      {
        BaseStrength = 1
      }, 5);

      engine.AddExplorationChoice(new Creature("Bob")
      {
        BaseDefense = 2,
        BaseStrength = 2,
        FavorReward = 2
      }, 5);

      engine.AddGameEvent(services.GetService<DummyGameEventTrait>());

      engine.Run();
      Console.ReadKey();
    }
  }
}
