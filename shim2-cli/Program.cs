using System;
using Shim;
using Shim.Entities;

namespace ShimCLI
{
  class Program
  {
    static void Main(string[] args)
    {
      SimulationParameters parameters = new SimulationParameters()
      {
        MaxRounds = 10,
        MaxActiveBlessingPerAgent = 1,
        MaxPermanentItemsPerAgent = 3,
        MinAgents = 2,
        MaxAgents = 6,
        StartingItemEnabled = false,
        HitPointsRestoredByHealer = 2,
        BaseMovementCost = 1,
        HitPointsAfterResurrection = 8,
        MaxActionsPerTurn = 20,
        MaxHitPoints = 10,
        DefaultBaseDefense = 0,
        DefaultBaseStrength = 0,
        DefaultMaxActionPoints = 4,
        DefaultMaxBonusActionPoints = 1
      };
      Simulation simulation = new Simulation(parameters);
      simulation.AddEvent(TraitManager.DummyTrait);
      simulation.AddAgent("Bear", new Trait[] {
        TraitManager.BasicAgentTrait,
        TraitManager.BearAspectTrait
      });
      simulation.AddAgent("Wolf", new Trait[] {
        TraitManager.BasicAgentTrait,
        TraitManager.WolfAspectTrait
      });
      simulation.AddAgent("Panther", new Trait[] {
        TraitManager.BasicAgentTrait,
        TraitManager.PantherAspectTrait
      });
      simulation.AddCreature(new Creature("DummyCreature")
      {
        BaseDefense = 0,
        BaseStrength = 1,
        FavorReward = 1
      });
      simulation.Run();
      System.IO.File.WriteAllLines(@".\Output.yaml", simulation.GetLog());
      Console.ReadKey();
    }
  }
}
