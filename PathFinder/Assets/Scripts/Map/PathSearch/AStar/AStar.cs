using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;


public struct MapNode: IEquatable<MapNode>, IComparable<MapNode>
{
    public Point pos, parent;
    public float dist { get; set;}
    public float cost { get; set; }
    public float totalCost => dist + cost;
    public static bool operator < (MapNode a, MapNode b) => a.totalCost < b.totalCost;
    public static bool operator >(MapNode a, MapNode b) => a.totalCost > b.totalCost;
    public static bool operator ==(Point pt, MapNode node) => node.pos == pt;
    public static bool operator !=(Point pt, MapNode node) => node.pos != pt;
    public static bool operator ==(MapNode node, Point pt) => node.pos == pt;
    public static bool operator !=(MapNode node, Point pt) => node.pos != pt;
    public static bool operator ==(MapNode node_a, MapNode node_b) => node_a.pos == node_b.pos;
    public static bool operator !=(MapNode node_a, MapNode node_b) => node_a.pos != node_b.pos;
    public override int GetHashCode()
    {
        return HashCode.Combine(pos, parent, dist, cost);
    }
    public override bool Equals(object obj)
    {
        if (!(obj is MapNode)) return false;
        return Equals((MapNode)obj);
    }
    public bool Equals(MapNode other)
    {
        if (!pos.Equals(other.pos)) return false;
        if (!parent.Equals(other.parent)) return false;
        if (dist != other.dist) return false;
        if (cost != other.cost) return false;
        return true;
    }
    public int CompareTo(MapNode other)
    { 
        if (this == other) return 0;
        if (this > other) return 1;
        return -1;
    }
}

public class AStar
{
    private static readonly Point[] _neighbours = new Point[8] { new Point(-1, -1), new Point( 1, -1), new Point(-1,  1),
                                                                 new Point( 1,  1), new Point( 0, -1), new Point(-1,  0),
                                                                 new Point( 0,  1), new Point( 1,  0) };

    private static readonly float[] _neighbours_cost = new float[8] { 1.414f, 1.414f, 1.414f, 1.414f, 1.0f, 1.0f, 1.0f, 1.0f };

    private Dictionary<PointsPair, List<Point>> _path_cache;
    private WeightsMap  _map;
    private Vector2     _physicalSize;
    private Vector2     _physicalOrig;
    private MapNode     _minimalNode;

    public Vector2 Size
    {
        get => _physicalSize;
        set
        {
            _physicalSize = new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
        }
    }
    public Vector2 Orig
    {
        get => _physicalOrig;
        set
        {
            _physicalOrig = value;
        }
    }

    private List<Vector2> BuildRealWorldPath(List<Point> path, bool reverce = false)
    {
        List<Vector2> worldPath = new List<Vector2>();
        float d_col = Size.y / Weights.Cols;
        float d_row = Size.x / Weights.Rows;
        float rowOffset = -Size.x * 0.5f + Orig.y;
        float colOffset = Size.y * 0.5f + Orig.x;
        if (reverce)
        {
            foreach (Point pt in path) worldPath.Add(new Vector2((pt.row + 0.5f) * d_row + rowOffset,  (-pt.col + 0.5f) * d_col + colOffset));
        }
        else 
        {
            foreach (Point pt in path) worldPath.Insert(0, new Vector2((pt.row + 0.5f) * d_row + rowOffset, (-pt.col + 0.5f) * d_col + colOffset));
        }
        return worldPath;
    }

    public bool CheckIfPositionValid(Vector2 position)
    {
        return Weights[GetCellPoint(position)] < WeightsMap.MAX_WEIGHT;
    }


    public bool RayCast(Vector2 start, Vector2 end, float threshold = 50)
    {
        return Weights.RayCast(GetCellPoint(start), GetCellPoint(end), threshold); //(start, end, Size, Orig, threshold);
    }


    public bool CheckCashedPath(ref List<Vector2> path, PointsPair key)
    {
        if (_path_cache.ContainsKey(key)) 
        {
            path = BuildRealWorldPath(_path_cache[key]);
            return true;
        }
        key = key.Swap();
        if (_path_cache.ContainsKey(key))
        {
            path = BuildRealWorldPath(_path_cache[key], true);
            return true;
        }
        return false;
    }
    private bool IsValidPt(ref Point p)
    {
        if (p.row < 0) return false;
        if (p.col < 0) return false;
        if (p.row >= Weights.Rows) return false;
        if (p.col >= Weights.Cols) return false;
        return true;
    }
    private bool FillOpen(Point start, Point target, MapNode current, ref Dictionary<int, MapNode> _open, ref Dictionary<int, MapNode> _closed)
    {
        if (current.pos == target) return true;

        float newCost   ; 
        float stepWeight;
        Point neighbour ;
        int   hash      ;

        for (int index = 0; index < _neighbours.Length; index++)
        {
            neighbour = current.pos + _neighbours[index];

            if (!IsValidPt(ref neighbour)) continue;

            hash = neighbour.Hash();

            if (_closed.ContainsKey(hash)) continue;

            if ((stepWeight = Weights[neighbour]) >= WeightsMap.MAX_WEIGHT) continue; ///???

            newCost = current.cost + _neighbours_cost[index] * stepWeight;

            MapNode node = new MapNode
            {
                cost   = newCost,
                dist   = Heuristics.DiagonalDistance2D(neighbour, target),
                pos    = neighbour,
                parent = current.pos
            };

            if (!_open.ContainsKey(hash))
            {
                _open.Add(hash, node);
                // if (node < _minimalNode) _minimalNode = node;
                continue;
            }

            if(_open[hash].cost < newCost) continue;
            _open[hash] = node;
            // if (node < _minimalNode) _minimalNode = node;

        }
        return false;
    }
    private void BuildPath(PointsPair key, ref Dictionary<int, MapNode> _closed)
    {
        if (_path_cache.ContainsKey(key)) return;

        MapNode last = _closed.Values.Last();

        List<Point> path = new List<Point>();

        while (last.parent != Point.MiusOnes)
        {
            path.Add(last.pos);
            last = _closed[last.parent.Hash()];
        }
        path.Add(key.Start);

        if (_path_cache.ContainsKey(key)) return;
        _path_cache.Add(key, path);
    }
    private Point GetCellPoint(Vector2 t)
    {
        // x - cols
        // y - rows
        float aspect = Size.x / Size.y;
        return new Point((short)((/*1.0f - */(t.x / aspect + Size.y * 0.5f - Orig.y) / Size.y) * (Weights.Rows - 1)), 
                         (short)((1.0f     - (t.y * aspect + Size.x * 0.5f - Orig.x) / Size.x) * (Weights.Cols - 1)));
    }
    private Vector2 GetCellUv(Vector2 t)
    {
        // x - cols
        // y - rows
        float aspect = Size.x / Size.y;
        return new Vector2(((/*1.0f - */(t.x / aspect + Size.y * 0.5f - Orig.y) / Size.y)),
                           ((1.0f - (t.y * aspect + Size.x * 0.5f - Orig.x) / Size.x)));
    }
    public float GetWeightBilinear(Vector2 pos) => Weights.GetWeightsBilinear(GetCellUv(pos));
    public float GetWeight(Vector3 pos)
    {
        return Weights[GetCellPoint(new Vector2(pos.x, pos.z))];
    }
    public float GetWeight(Vector2 pos)
    {
        return Weights[GetCellPoint(pos)];
    }
    public float GetWeightBilinearNormalized(Vector2 pos) => Weights.GetWeightsBilinearNormalized(GetCellUv(pos));

    public float GetWeightNormalized(Vector2 pos)
    {
        return (GetWeight(pos) - WeightsMap.MIN_WEIGHT) / (WeightsMap.MAX_WEIGHT - WeightsMap.MIN_WEIGHT);
    }
    public float GetWeightNormalized(Vector3 pos)
    {
        return (GetWeight(pos) - WeightsMap.MIN_WEIGHT) / (WeightsMap.MAX_WEIGHT - WeightsMap.MIN_WEIGHT);
    }
    private List<Vector2> SearchPath(Point start, Point end)
    {
        if (Weights[start] >= WeightsMap.MAX_WEIGHT) 
        {
            Debug.LogWarning("Start point in forbidden zone...");
            return null;
        }
        if (Weights[end]   >= WeightsMap.MAX_WEIGHT) 
        {
            Debug.LogWarning("End point in forbidden zone...");
            return null;
        }
        // if (pointsSeeEachOther) 
        // {
        //     if (!Weights.RayCast(start, end)) return null; 
        // }
        PointsPair _pair_key = new PointsPair(start, end);

        List<Vector2> path = null;
        
        if (CheckCashedPath(ref path, _pair_key)) return path;
        
        Dictionary<int, MapNode> _open = new Dictionary<int, MapNode>();
        Dictionary<int, MapNode> _clsd = new Dictionary<int, MapNode>();
        
        bool    _success = false;
        int     _hash    = start.Hash();
        int     _cntr    = 0;
        MapNode _node    = new MapNode
        {
            cost   = 0.0f,
            pos    = start,
            parent = Point.MiusOnes,
            dist   = Heuristics.DiagonalDistance2D(end, start)
        };
        _open.Add(_hash, _node);
        
        _minimalNode = _node;

        // SetupMinimalNode();

        while (true)
        {
            // _node = _minimalNode; //  _open.Values.Min();
            _node = _open.Values.Min();
            _hash = _node.pos.Hash();
            _clsd.Add(_hash, _node);
            _open.Remove(_hash);
            _success = FillOpen(start, end, _node, ref _open, ref _clsd);
            if (_success) break;
            if (_open.Count == 0) break;
            if (_cntr == Weights.NCells) 
            {
                Debug.LogWarning("A* cheked each cell in map, but optimal path was not found...");
                break;
            }
            _cntr++;
        }
        if (!_success) return null;

        BuildPath(_pair_key, ref _clsd);

        if(!CheckCashedPath(ref path, _pair_key)) return null;
        
        return path;
    }

    private IEnumerator SearchPathEnum(List<Vector2> newPath, Point start, Point end) //, bool pointsSeeEachOther = false)
    {
        if (Weights[start] >= WeightsMap.MAX_WEIGHT)
        {
            Debug.LogWarning("Start point in forbidden zone...");
            yield break;
        }
        if (Weights[end] >= WeightsMap.MAX_WEIGHT)
        {
            Debug.LogWarning("End point in forbidden zone...");
            yield break;
        }

        // if (pointsSeeEachOther)
        // {
        //     if (!Weights.RayCast(start, end)) yield break;
        // }

        PointsPair key = new PointsPair(start, end);

        List<Vector2> path = null;

        if (CheckCashedPath(ref path, key)) 
        {
            newPath = path;
            yield break;
        }
        
        Dictionary<int, MapNode> _open  = new Dictionary<int, MapNode>();
        Dictionary<int, MapNode> _clsd = new Dictionary<int, MapNode>();
        float _t_start = Time.time;
        bool  _success = false;
        int   _hash    = start.Hash();
        int   _cntr    = 0;
        MapNode _node  = new MapNode
        {
            cost   = 0.0f,
            pos    = start,
            parent = Point.MiusOnes,
            dist   = Heuristics.DiagonalDistance2D(start, end)
        };
        _open.Add(_hash, _node);
        
        while (true)
        {
            if (Time.time - _t_start > 0.1f)
            {
                _t_start = Time.time;
                yield return null;
            }

            _node = _open.Values.Min();
            _hash = _node.pos.Hash();
            _clsd.Add(_hash, _node);
            _open.Remove(_hash);
            _success = FillOpen(start, end, _node, ref _open, ref _clsd);
            if (_success) break;
            if (_open.Count == 0) break;
            if (_cntr == Weights.NCells)
            {
                Debug.LogWarning("A* cheked each cell in map, but optimal path was not found...");
                break;
            }
            _cntr++;
        }

        yield return null;

        if (!_success) yield break;

        BuildPath(key, ref _clsd);

        yield return null;

        if (!CheckCashedPath(ref path, key)) yield break;

        newPath = path;
// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.isPaused = true;
// #endif
    }
    public List<Vector2> Search(Point start, Point end) => SearchPath(start, end);
    public List<Vector2> Search() => SearchPath(new Point(0, 0), new Point((short)(Weights.Rows - 1), (short)(Weights.Cols - 1)));
    public List<Vector2> Search(Vector2 start, Vector2 end) => Search(GetCellPoint(start), GetCellPoint(end));

    public WeightsMap Weights => _map;
    public override string ToString()
    {
        return "";
    }
    public AStar(int rows, int cols, float[]weigths) 
    {
        _map        = new WeightsMap(rows, cols, weigths);
        _path_cache = new Dictionary<PointsPair, List<Point>>();

    }
    public AStar(Texture2D texture, bool invertWeights = false)
    {
        _map        = new WeightsMap(texture, invertWeights);
        _path_cache = new Dictionary<PointsPair, List<Point>>();
    }
}

