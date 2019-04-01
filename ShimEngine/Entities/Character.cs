using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public class Character : CombatEntity
  {
    public List<Trait> Traits { get; set; }

    public Character(string name) : base(name)
    {
      Traits = new List<Trait>();
    }

    public virtual bool HasTrait(Trait trait)
    {
      return Traits.Contains(trait);
    }
    public virtual void AssignTrait(Trait trait)
    {
      if (!HasTrait(trait))
      {
        Traits.Add(trait);
      }
    }
    public virtual void UnassignTrait(Trait trait)
    {
      if (HasTrait(trait))
      {
        Traits.Remove(trait);
      }
    }
  }
}
