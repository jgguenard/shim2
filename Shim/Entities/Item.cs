using System;
using System.Collections.Generic;
using System.Text;

namespace Shim.Entities
{
  public class Item
  {
    public string Name { get; set; }
    public int BaseStrength { get; set; }
    public int BaseDefense { get; set; }
    public bool IsPotion { get; set; }
    public bool IsPermanent { get
      {
        return !IsPotion;
      }
    }
    public bool IsRunic { get; set; }
    public Trait Trait { get; set; }
    public Item(string name)
    {
      Name = name;
      BaseDefense = 0;
      BaseStrength = 0;
      IsPotion = false;
      IsRunic = false;
    }
  }
}
