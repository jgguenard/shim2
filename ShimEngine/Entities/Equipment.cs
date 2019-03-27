namespace Raido.Shim.Entities
{
  public class Equipment : Agent
  {
    public bool IsRunic { get; set; }
    public Aura Aura { get; set; }
    public Equipment(string name) : base(name)
    {
      IsRunic = false;
    }
  }
}
