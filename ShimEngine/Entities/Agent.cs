using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public class Agent : Entity
  {
    public int BaseStrength { get; set; }
    public int BaseDefense { get; set; }
    public List<Trait> Traits { get; set; }

    public Agent(string name) : base(name)
    {
      BaseStrength = 0;
      BaseDefense = 0;
      Traits = new List<Trait>();
    }

    public virtual bool HasTrait(Trait trait)
    {
      return Traits.Contains(trait);
    }
    public virtual int GetDefenseAgainst(Agent target)
    {
      return BaseDefense;
    }

    public virtual int GetStrengthAgainst(Agent target)
    {
      return BaseStrength;
    }
  }
}
