using Shim.Library;
using System.Runtime.CompilerServices;

namespace Shim.Entities
{
  public class Trait
  {
    public bool Stackable { get; set; }
    public string Name {
      get
      {
        return GetType().Name;
      }
    }

    protected void Log(object caller, string message, [CallerMemberName]string methodName = "")
    {
      HistoryManager.LogMessage($"{message} ({caller.GetType()}:{methodName}())");
    }
  }
}
