using Shim.Library;
using System.Runtime.CompilerServices;

namespace Shim.Entities
{
  public class Trait
  {
    public bool Stackable { get; set; }
    public int ActionPointCost { get; set; }
    public string Name {
      get
      {
        return GetType().Name;
      }
    }

    public Trait()
    {
      ActionPointCost = 0;
    }

    protected void Log(object caller, string message, [CallerMemberName]string methodName = "")
    {
      Logger.Log($"{message} ({caller.GetType()}:{methodName}())");
    }
  }
}
