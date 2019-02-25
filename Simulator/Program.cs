using System;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shim;
using Shim.Entities;
using Simulator.Entities;

namespace Simulator
{
  class Program
  {
    static void Main(string[] args)
    {
      GameParameters parameters = new GameParameters()
      {
        MaxRounds = 10,
        MaxActiveBlessingPerAgent = 1,
        MaxPermanentItemsPerAgent = 3,
        MinAgents = 2,
        MaxAgents = 6,
        StartingItemEnabled = false,
        HitPointsRestoredByHealer = 2,
        BaseMovementCost = 1,
        BaseActionCost = 1,
        HitPointsAfterResurrection = 8,
        MaxActionsPerTurn = 20,
        MaxHitPoints = 10,
        DefaultBaseDefense = 0,
        DefaultBaseStrength = 0,
        DefaultMaxActionPoints = 4,
        DefaultMaxBonusActionPoints = 1,
        StartingTiles = new string[] { "A1", "M13", "M1", "A13" },
        AgentAttackBaseRange = 4
      };
      Game game = new Game(parameters);
      game.AddEvent(TraitManager.DummyTrait);
      game.AddAgent("Bear", new Trait[] {
        TraitManager.BasicAgentTrait,
        TraitManager.DuelQuest,
        TraitManager.HelpQuest,
        TraitManager.BearAspectTrait
      });
      game.AddAgent("Wolf", new Trait[] {
        TraitManager.BasicAgentTrait,
        TraitManager.DuelQuest,
        TraitManager.HelpQuest,
        TraitManager.WolfAspectTrait
      });
      game.AddAgent("Panther", new Trait[] {
        TraitManager.BasicAgentTrait,
        TraitManager.DuelQuest,
        TraitManager.HelpQuest,
        TraitManager.PantherAspectTrait
      });
      game.AddCreature(new Creature("DummyCreature1")
      {
        BaseDefense = 0,
        BaseStrength = 1,
        FavorReward = 1
      });
      game.AddCreature(new Creature("DummyCreature2")
      {
        BaseDefense = 0,
        BaseStrength = 3,
        FavorReward = 3
      });
      game.AddCreature(new Creature("DummyCreature3")
      {
        BaseDefense = 2,
        BaseStrength = 2,
        FavorReward = 2
      });
      game.AddBlessing(TraitManager.DummyBlessing, ExpirationType.Now);
      game.AddBlessing(TraitManager.DummyBlessing, ExpirationType.Now);
      game.AddBlessing(TraitManager.DummyBlessing, ExpirationType.Now);
      game.AddBlessing(TraitManager.DummyBlessing, ExpirationType.Now);
      game.AddBlessing(TraitManager.DummyBlessing, ExpirationType.Now);
      game.AddTrap(TraitManager.DummyTrap, ExpirationType.Now);
      game.AddTrap(TraitManager.DummyTrap, ExpirationType.Now);
      game.AddTrap(TraitManager.DummyTrap, ExpirationType.Now);
      game.AddTrap(TraitManager.DummyTrap, ExpirationType.Now);
      game.AddTrap(TraitManager.DummyTrap, ExpirationType.Now);
      for( var i = 0; i < 6; i++)
      {
        game.AddItem(new Item("Spear")
        {
          BaseDefense = 0,
          BaseStrength = 2,
          Trait = TraitManager.DummyItemTrait
        });
        game.AddItem(new Item("Shield")
        {
          BaseDefense = 2,
          BaseStrength = 0
        });
        game.AddItem(new Item("Helm")
        {
          BaseDefense = 1,
          BaseStrength = 1
        });
        game.AddItem(new Item("Cape")
        {
          BaseDefense = 1,
          BaseStrength = 0
        });
        game.AddItem(new Item("Hammer")
        {
          BaseDefense = 0,
          BaseStrength = 1
        });
      }
      game.Run();
      System.IO.File.WriteAllLines(@".\Output.yaml", game.GetLog());

      var state = game.GetState();

      using (var db = new SimulationContext())
      {
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        db.Database.Migrate();
        var simulation = new Simulation()
        {
          SimulationId = Guid.NewGuid().ToString(),
          TotalAgents = state.Agents.Count,
          TotalRounds = state.Round,
          Parameters = JsonConvert.SerializeObject(parameters)
        };
        state.Agents.ForEach(a =>
        {
          simulation.Results.Add(new Result()
          {
            Name = a.Name,
            Favor = a.Favor
          });
        });
        db.Simulations.Add(simulation);
        db.SaveChanges();
      }
    }
  }
}
