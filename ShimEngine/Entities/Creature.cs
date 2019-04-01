namespace Raido.Shim.Entities
{
  public class Creature : Character
  {
    public int FavorReward { get; set; }
    public int PackSize { get; set; }
    public Creature(string name) : base(name)
    {
      FavorReward = 0;
      PackSize = 1;
    }
  }
}
