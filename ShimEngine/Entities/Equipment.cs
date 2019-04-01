namespace Raido.Shim.Entities
{
  public class Equipment : CombatEntity
  {
    public Aura Aura { get; set; }

    public Equipment(string name) : base(name)
    {
    }
  }
}
