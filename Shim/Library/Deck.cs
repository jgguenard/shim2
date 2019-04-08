using System;
using System.Collections.Generic;

namespace Shim.Library
{
  public class Deck<T>
  {
    private List<T> _discarded;
    private List<T> _available;
    private List<string> _discardedTag;
    private string _name;

    public Deck(string name)
    {
      _available = new List<T>();
      _discarded = new List<T>();
      _discardedTag = new List<string>();
      _name = name;
    }

    public List<T> GetAll()
    {
      return _available;
    }

    public T Get(int index)
    {
      return _available[index];
    }

    public T Draw()
    {
      if (_available.Count == 0)
      {
        if (_discarded.Count == 0)
        {
          throw new Exception($"{_name} Deck is empty");
        }
        else
        {
          _available = _discarded;
          _discarded = new List<T>();
          _discardedTag = new List<string>();
          Shuffle();
        }
      }
      var item = _available[0];
      _available.RemoveAt(0);
      return item;
    }

    public List<T> DiscardedItems(string tag = "")
    {
      List<T> items = new List<T>();
      for (int t = 0; t < _discardedTag.Count; t++)
      {
        if (_discardedTag[t] == tag)
        {
          items.Add(_discarded[t]);
        }
      }
      return items;
    }

    public List<T> AvailableItems()
    {
      List<T> items = new List<T>();
      items.ForEach(i => items.Add(i));
      return items;
    }

    public T Undiscard(string tag = "")
    {
      int index = _discardedTag.FindLastIndex(t => t == tag);
      if (index < 0)
      {
        return default(T);
      }
      var item = _discarded[index];
      _discarded.RemoveAt(index);
      _discardedTag.RemoveAt(index);
      return item;
    }

    public void Add (T item)
    {
      _available.Add(item);
    }

    public void Discard(T item, string tag = "")
    {
      _discarded.Add(item);
      _discardedTag.Add(tag);
    }

    public void Shuffle()
    {
      Random rng = new Random();
      int n = _available.Count;
      while (n > 1)
      {
        n--;
        int k = rng.Next(n + 1);
        T value = _available[k];
        _available[k] = _available[n];
        _available[n] = value;
      }
    }
  }
}
