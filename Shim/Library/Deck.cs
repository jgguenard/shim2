using System;
using System.Collections.Generic;

namespace Shim.Library
{
  public class Deck<T>
  {
    private List<T> _discarded;
    private List<T> _available;
    private string _name;

    public Deck(string name)
    {
      _available = new List<T>();
      _discarded = new List<T>();
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
          Shuffle();
        }
      }
      var item = _available[0];
      _available.RemoveAt(0);
      return item;
    }

    public void Add (T item)
    {
      _available.Add(item);
    }

    public void Discard(T item)
    {
      _discarded.Add(item);
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
