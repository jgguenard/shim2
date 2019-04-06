using System.Collections.Generic;

namespace Raido.Shim.Entities
{
  public class Summary
  {
    public int Rounds { get; set; }
    public int PlayerCount { get; set; }
    public Dictionary<string, int> FavorByPlayer { get; set; }

    public Summary()
    {
      Rounds = 0;
      PlayerCount = 0;
      FavorByPlayer = new Dictionary<string, int>();
    }
  }
}