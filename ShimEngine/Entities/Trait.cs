namespace Raido.Shim.Entities
{
  public class Trait
  {
    public string Name
    {
      get
      {
        return GetType().Name;
      }
    }
  }
}