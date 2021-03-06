﻿using Microsoft.Extensions.DependencyInjection;
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
        MaxRounds = 20,
        ExplorationChoices = 3,
        AvailableFavor = (playerCount) => (playerCount * 2) + 1
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
        .AddSingleton<DummyPotionTrait>()
        .AddSingleton<DummyQuestTrait>()
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

      engine.AddCard(new Equipment("Armor") {
        BaseDefense = 1
      }, 3);

      engine.AddCard(new Equipment("Weapon")
      {
        BaseStrength = 1
      }, 7);

      engine.AddCard(new Creature("Berserker")
      {
        BaseDefense = 1,
        BaseStrength = 2,
        FavorReward = 2
      }, 5);

      engine.AddCard(new Potion("Potion")
      {
        Aura = new Aura()
        {
          Source = AuraSource.Potion,
          Expiration = AuraExpiration.Now,
          Scope = AuraScope.Self,
          Trait = services.GetService<DummyPotionTrait>()
        }
      }, 2);

      engine.AddCard(new Quest("Quest", services.GetService<DummyQuestTrait>()), 3);

      engine.AddGameEvent(services.GetService<DummyGameEventTrait>());

      engine.Run();
      Console.ReadKey();
    }
  }
}
