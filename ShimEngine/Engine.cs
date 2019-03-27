using Microsoft.Extensions.DependencyInjection;
using Raido.Shim.Entities;
using System.Collections.Generic;

namespace Raido.Shim
{
  public class Engine
  {
    public static ServiceProvider Services;
    public List<Character> _characters;
    public Settings _settings { get; }

    public Engine(Settings settings)
    {
      _settings = settings;
      _characters = new List<Character>();
    }

    public void AddCharacter(string name, Trait[] traits = null)
    {
      Character character = new Character(name);
      if (traits != null)
      {
        foreach (Trait trait in traits)
        {
          character.AssignTrait(trait);
        }
      }
      _characters.Add(character);
    }

    public void Run()
    {
      Services = new ServiceCollection()
        .BuildServiceProvider();
    }
  }
}