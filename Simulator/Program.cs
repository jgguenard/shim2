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
        MaxPlayers = 6,
        BlessingSlots = 1,
        EquipmentSlots = 3,
        DefaultMaxActionPoints = 3,
        MaxHitPoints = 10,
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

      engine.Run();
      Console.ReadKey();
    }
  }
}
