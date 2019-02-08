using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shim.Entities
{
  public class Aura
  {
    public ScopeType Scope { get; set; }
    public ExpirationType Expiration { get; set; }
    public Trait Trait { get; set; }
  }
}