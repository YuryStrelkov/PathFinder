using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Units.PathSearch
{
    public class PathFinder
    {
        private static PathFinder _instance;
        public static PathFinder Instance
        {
            get
            {
                if (_instance == null) 
                {
                    _instance = new PathFinder();
                }
                return _instance;
            }
        }
        Dictionary<int, AStar> _maps;
        Stack<int> _keys;
        private PathFinder()
        {
            _maps = new Dictionary<int, AStar>();
            _keys = new Stack<int>();
        }
        public bool CheckIfPositionValid(Vector3 pos, int map_id = 0)
        {
            if (!_maps.ContainsKey(map_id)) return false;
            return _maps[map_id].CheckIfPositionValid(pos);
        }

        public bool RayCast(Vector3 from, Vector3 to, int map_id = 0) 
        {
            if (!_maps.ContainsKey(map_id)) return false;
            return _maps[map_id].RayCast(from, to);
        }

        // public List<Vector3> RequestPath(Vector3 from, Vector3 to, bool pointsSeeEachOther = false, int map_id = 0)
        // {
        //     if (!_maps.ContainsKey(map_id))     return null;
        //     return _maps[map_id].Search(from, to, pointsSeeEachOther);
        // }


        public bool ContainsMap(int map_id) => _maps.ContainsKey(map_id);
        public AStar GetMap(int map_id)
        {
            if (!ContainsMap(map_id)) return null;
            return _maps[map_id];
        }
        public int RegisterMap(Vector2 orig, Vector2 size, Vector2Int sizeInt, float[] map) 
        {
            int key = _maps.Count;
            if (_keys.Count != 0) key = _keys.Pop();
            AStar _map = new AStar(sizeInt.x, sizeInt.y, map)
            {
                Size = size,
                Orig = orig
            };
            _maps.Add(key, _map);
            return key;
        }
        public int RegisterMap(Vector2 orig, Vector2 size, Texture2D texture, bool invertWeights = true)
        {
            int key = _maps.Count;
            if (_keys.Count != 0) key = _keys.Pop();
            AStar _map = new AStar(texture, invertWeights)
            {
                Size = size,
                Orig = orig
            };
            _maps.Add(key, _map);
            return key;
        }
        public void UnregisterMap(int map_id) 
        {
            if (!_maps.ContainsKey(map_id)) return;
            _keys.Push(map_id);
            _maps.Remove(map_id);
        }

    }
}
