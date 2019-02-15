using Shim.Entities;
using Shim.Events;

namespace Shim.Traits
{
  public class DummyItemTrait : Trait
  {
    public DummyItemTrait() : base()
    {
      EventManager.EvaluateStat += OnEvaluateStat;
    }

    public void OnEvaluateStat(object sender, EvaluateStatEvent e)
    {
    }
  }
}