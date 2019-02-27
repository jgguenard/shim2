namespace Shim.Entities
{
  public class Aura
  {
    public AuraType Type { get; set; }
    public ScopeType Scope { get; set; }
    public ExpirationType Expiration { get; set; }
    public int ActionPointCost { get; set; }
    public Trait Trait { get; set; }
    public Aura()
    {
      ActionPointCost = 0;
      Type = AuraType.Other;
    }
  }
}