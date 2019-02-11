namespace Shim.Entities
{
  public class Aura
  {
    public AuraType Type { get; set; }
    public ScopeType Scope { get; set; }
    public ExpirationType Expiration { get; set; }
    public Trait Trait { get; set; }
    public Aura()
    {
      Type = AuraType.Other;
    }
  }
}