namespace Raido.Shim.Entities
{
  public class Potion : Entity
  {
    public Aura Aura { get; set; }
    public Potion(string name) : base(name)
    {
    }
  }
}
