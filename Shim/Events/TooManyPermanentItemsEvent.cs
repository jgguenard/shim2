using Shim.Entities;
using System;

namespace Shim.Events
{
  public class TooManyPermanentItemsEvent : EventArgs
  {
    public Agent Source { get; set; }
    public Item NewItem { get; set; }
    public Item ItemToDiscard { get; set; }
  }

  public delegate void TooManyPermanentItemsEventHandler(object sender, TooManyPermanentItemsEvent e);
}