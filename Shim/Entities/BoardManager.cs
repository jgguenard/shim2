using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using Shim.Library;
using System.Collections.Generic;
using System.Linq;

namespace Shim.Entities
{
  public class BoardManager
  {
    private readonly UndirectedGraph<Tile, UndirectedEdge<Tile>> _board;
    private readonly Dictionary<string, Tile> _tiles;
      
    public BoardManager()
    {
      _board = new UndirectedGraph<Tile, UndirectedEdge<Tile>>(true);
      _tiles = new Dictionary<string, Tile>();
    }

    public void Initialize()
    {
      int cols = 13;
      int rows = 13;

      // generate tiles
      for (int col = 0; col < cols; col++)
      {
        char colName = (char)(65 + col);
        for (int row = 0; row < rows; row++)
        {
          string tileName = $"{colName}{row + 1}";
          var tile = new Tile() { Type = TitleType.Empty, Name = tileName };
          _tiles[tileName] = tile;
          _board.AddVertex(tile);
        }
      }

      // set types
      _tiles["E1"].Type = TitleType.Item;
      _tiles["I1"].Type = TitleType.Trap;
      _tiles["A5"].Type = TitleType.Healer;
      _tiles["E5"].Type = TitleType.Creature;
      _tiles["I5"].Type = TitleType.Gate;
      _tiles["I5"].StringValue = "E9";
      _tiles["M5"].Type = TitleType.Discovery;
      _tiles["M5"].IntValue = 1;
      _tiles["A9"].Type = TitleType.Discovery;
      _tiles["A9"].IntValue = 1;
      _tiles["E9"].Type = TitleType.Gate;
      _tiles["E9"].StringValue = "I5";
      _tiles["I9"].Type = TitleType.Blessing;
      _tiles["M9"].Type = TitleType.Item;
      _tiles["E13"].Type = TitleType.Healer;
      _tiles["I13"].Type = TitleType.Creature;

      // connect nodes
      for (int i = 1; i < rows; i++)
      {
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles["A" + i], _tiles["A" + (i + 1)]));
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles["E" + i], _tiles["E" + (i + 1)]));
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles["I" + i], _tiles["I" + (i + 1)]));
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles["M" + i], _tiles["M" + (i + 1)]));
      }
      for (int i = 1; i < cols; i++)
      {
        char col1Name = (char)(65 + i - 1);
        char col2Name = (char)(65 + i);
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles[$"{col1Name}1"], _tiles[$"{col2Name}1"]));
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles[$"{col1Name}5"], _tiles[$"{col2Name}5"]));
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles[$"{col1Name}9"], _tiles[$"{col2Name}9"]));
        _board.AddEdge(new UndirectedEdge<Tile>(_tiles[$"{col1Name}13"], _tiles[$"{col2Name}13"]));
      }
    }

    public IEnumerable<Tile> GetPath(Tile from, Tile to)
    {
      List<Tile> path = new List<Tile>();
      var shortestPath = GetShortestPath(from, to);
      if (shortestPath != null)
      {
        Tile previous = from;
        foreach (var edge in shortestPath)
        {
          if (edge.Source == previous)
          {
            path.Add(edge.Target);
            previous = edge.Target;
          }
          else
          {
            path.Add(edge.Source);
            previous = edge.Source;
          }
        }
      }
      return path;
    }

    public IEnumerable<Tile> GetReachableValuableTiles(Tile from, int distance, Tile previousTile = null)
    {
      List<Tile> tiles = new List<Tile>();
      var otherNonEmptyTiles = _tiles.Values.Where((Tile t) => t.Type != TitleType.Empty && t != from).ToList();
      otherNonEmptyTiles.ForEach((Tile to) =>
      {
        var path = GetShortestPath(from, to, previousTile);
        if (path.Count() <= distance)
        {
          tiles.Add(to);
        }
      });
      return tiles;
    }

    public Tile GetTile(string name)
    {
      return _tiles[name];
    }
    
    private IEnumerable<UndirectedEdge<Tile>> GetShortestPath(Tile from, Tile to, Tile exclude = null)
    {
      double getDistance(UndirectedEdge<Tile> x) => (exclude != null && (exclude == x.Source || exclude == x.Target)) ? 9999 : 1;
      var tryGetShortestPath = _board.ShortestPathsDijkstra(getDistance, from);
      return tryGetShortestPath(to, out IEnumerable<UndirectedEdge<Tile>> path) ? path : null;
    }
  }
}