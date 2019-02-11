using Shim.Traits;

namespace Shim.Entities
{
  public class TraitManager
  {
    public static BasicAgentTrait BasicAgentTrait = new BasicAgentTrait();
    public static WolfAspectTrait WolfAspectTrait = new WolfAspectTrait();
    public static BearAspectTrait BearAspectTrait = new BearAspectTrait();
    public static PantherAspectTrait PantherAspectTrait = new PantherAspectTrait();
    public static DummyTrait DummyTrait = new DummyTrait();
    public static DummyBlessing DummyBlessing = new DummyBlessing();

    public static void Initialize(EventManager events, BoardManager board) {
      BasicAgentTrait.Initialize(events, board);
      WolfAspectTrait.Initialize(events);
      BearAspectTrait.Initialize(events);
      DummyTrait.Initialize(events);
      PantherAspectTrait.Initialize(events);
      DummyBlessing.Initialize(events);
    }
  }
}