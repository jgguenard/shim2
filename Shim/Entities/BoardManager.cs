using QuickGraph;
using QuickGraph.Algorithms;
using System.Collections.Generic;
using System.Linq;

namespace Shim.Entities
{
  public class BoardManager
  {
    private readonly UndirectedGraph<Tile, UndirectedEdge<Tile>> _board;
    private readonly Dictionary<string, Tile> _tiles;

    private IEnumerable<UndirectedEdge<Tile>> ShortestPath(Tile from, Tile to, Tile exclude = null)
    {
      double getDistance(UndirectedEdge<Tile> x) => (exclude != null && (exclude == x.Source || exclude == x.Target)) ? 9999 : 1;
      var tryGetShortestPath = _board.ShortestPathsDijkstra(getDistance, from);
      return tryGetShortestPath(to, out IEnumerable<UndirectedEdge<Tile>> path) ? path : null;
    }

    private List<Tile> GetPathFromEdges(IEnumerable<UndirectedEdge<Tile>> edges, Tile from)
    {
      List<Tile> path = new List<Tile>();
      if (edges != null)
      {
        Tile previous = from;
        foreach (var edge in edges)
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
          var tile = new Tile() { Type = TileType.Empty, Name = tileName };
          _tiles[tileName] = tile;
          _board.AddVertex(tile);
        }
      }

      // set types
      _tiles["E1"].Type = TileType.Item;
      _tiles["I1"].Type = TileType.Trap;
      _tiles["A5"].Type = TileType.Healer;
      _tiles["E5"].Type = TileType.Creature;
      _tiles["I5"].Type = TileType.Gate;
      _tiles["I5"].StringValue = "E9";
      _tiles["M5"].Type = TileType.Discovery;
      _tiles["M5"].IntValue = 1;
      _tiles["A9"].Type = TileType.Discovery;
      _tiles["A9"].IntValue = 1;
      _tiles["E9"].Type = TileType.Gate;
      _tiles["E9"].StringValue = "I5";
      _tiles["I9"].Type = TileType.Blessing;
      _tiles["M9"].Type = TileType.Item;
      _tiles["E13"].Type = TileType.Healer;
      _tiles["I13"].Type = TileType.Creature;

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

    public List<Tile> GetPath(Tile from, Tile to)
    {
      List<Tile> path = new List<Tile>();
      var shortestPath = ShortestPath(from, to);
      return GetPathFromEdges(shortestPath, from);
    }

    public List<List<Tile>> ReachablePointsOfInterest(Tile from, int distance, Tile previousTile = null)
    {
      List<List<Tile>> paths = new List<List<Tile>>();
      var otherNonEmptyTiles = _tiles.Values.Where((Tile t) => t.Type != TileType.Empty && t != from).ToList();
      otherNonEmptyTiles.ForEach((Tile to) =>
      {
        var path = ShortestPath(from, to, previousTile);
        if (path.Count() <= distance)
        {
          paths.Add(GetPathFromEdges(path, from));
        }
      });
      return paths;
    }

    public Tile GetTile(string name)
    {
      return _tiles[name];
    }

    public List<Tile> ShortestPathToTileType(Tile from, TileType type)
    {
      var tiles = _tiles.Values.Where((Tile t) => t.Type == type).ToList();
      IEnumerable<Tile> nearestPath = null;
      int minDistance = 9999;
      tiles.ForEach(t =>
      {
        var path = GetPath(from, t);
        if (nearestPath == null || nearestPath.Count() < minDistance)
        {
          nearestPath = path;
          minDistance = nearestPath.Count();
        }
      });
      return nearestPath.ToList();
    }
  }
}