namespace Raido.Shim.Entities
{
  public class CombatEntity : Entity
  {
    public int BaseStrength { get; set; }
    public int BaseDefense { get; set; }

    public CombatEntity(string name) : base(name)
    {
      BaseStrength = 0;
      BaseDefense = 0;
    }

    public virtual int GetDefenseAgainst(Character target)
    {
      return BaseDefense;
    }

    public virtual int GetStrengthAgainst(Character target)
    {
      return BaseStrength;
    }
  }
}
