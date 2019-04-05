namespace Raido.Shim.Entities
{
  public class Quest : Entity
  {
    public Aura Aura { get; set; }

    public Quest(string name, Trait trait) : base(name)
    {
      Aura = new Aura()
      {
        Expiration = AuraExpiration.Never,
        Scope = AuraScope.Self,
        Source = AuraSource.Quest,
        Trait = trait
      };
    }
  }
}
