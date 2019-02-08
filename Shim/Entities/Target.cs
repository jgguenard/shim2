using System.Collections.Generic;

namespace Shim.Entities
{
  public class Target
  {
    public string Name { get; set; }
    public int BaseStrength { get; set; }
    public int BaseDefense { get; set; }
    public List<Trait> Traits { get; set; }

    public Target(string name)
    {
      Name = name;
      Traits = new List<Trait>();
    }

    public virtual bool HasTrait(Trait trait)
    {
      return Traits.Contains(trait);
    }

    public virtual int GetDefenseAgainst(Target target)
    {
      return BaseDefense;
    }

    public virtual int GetStrengthAgainst(Target target)
    {
      return BaseStrength;
    }
  }
}