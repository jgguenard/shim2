using Raido.Shim;
using System;

namespace Simulator
{
  class Program
  {
    static void Main(string[] args)
    {
      Settings settings = new Settings
      {
        MaxAgents = 2,
        BlessingSlots = 1,
        EquipmentSlots = 3,
        DefaultMaxActionPoints = 3,
        MaxHitPoints = 10,
      };

      Engine engine = new Engine(settings);

      engine.AddCharacter("Wolf");
      engine.AddCharacter("Bear");
      engine.AddCharacter("Stag");
      engine.AddCharacter("Eagle");
      engine.AddCharacter("Monkey");
      engine.AddCharacter("Panther");

      engine.Run();
      Console.ReadKey();
    }
  }
}
