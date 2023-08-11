using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    /*
     1 - операция вставки в конец; 
     2 - при перемещении делать запрос на перестройку пути сегмента слева, сегмента справа;
     3 - при удалении соединение предыдущего со следующим новым сегментом пути;
     */
    // PathOfPoints _parent;
    Vector3 position;

    private static Stack<int>                _wayPointsIds;
    private static Dictionary<int, WayPoint> _wayPoints;
    public static Stack<int>                WayPointsIds
    {
        get
        {
            if (_wayPointsIds == null) _wayPointsIds = new Stack<int>();
            return _wayPointsIds;
        }
    }
    public static Dictionary<int, WayPoint> WayPoints
    {
        get
        {
            if (_wayPoints == null) _wayPoints = new Dictionary<int, WayPoint>();
            return _wayPoints;
        }
    }

    public static WayPoint LastInstance { get; protected set; }

    private int _index;
    WayPoint    _prev;
    WayPoint    _next;
    PathSegment _pathToNext;
    public int Index => _index;
    public bool HasNext => _next != null;
    public bool HasPrev => _prev != null;

    public PathSegment PathToNext => _pathToNext;
    public WayPoint Next => _next;
    public WayPoint Prev => _prev;

    public void OnPositionHasChanged()
    {
        if (_prev) _prev.LinkToNext();
        LinkToNext();
    }

    public void OnMoveBegin()
    {
        position = transform.position;
    }

    public void OnMoveEnd()
    {
        if((position - transform.position).magnitude < 1e-3f) return;
        OnPositionHasChanged();
    }

    private void LinkToNext()
    {
        if (!HasNext)
        {
            if (_pathToNext) { Destroy(_pathToNext.gameObject); return; }
        }
      
        if (_pathToNext)
        {
            Vector2 startPt = new Vector2(transform.position.x, transform.position.z);
            Vector2 endPt = new Vector2(_next.transform.position.x, _next.transform.position.z);
            var points = AreaMap.Instance.BuildPath(startPt, endPt);
            if (points == null)
            {
                _pathToNext.BildPath(new List<Vector2>(new Vector2[] { startPt, endPt }), AreaMap.Instance);
                return;
            }
            if (points.Count == 0)
            {
                _pathToNext.BildPath(new List<Vector2>(new Vector2[] { startPt, endPt }), AreaMap.Instance);
                return;
            }
            _pathToNext.BildPath(points, AreaMap.Instance);
        }
    }
    public void OnPointDelete()
    {
        if (_prev != null) _prev._next = _next;
        if (_next != null) _next._prev = _prev;
        if (LastInstance) if (_index == LastInstance._index) LastInstance = Prev;
        if (_pathToNext) Destroy(_pathToNext.gameObject);
        if (_prev) _prev.LinkToNext();
    }

    private void Init(WayPoint prev = null, WayPoint next = null)
    {
        _prev = prev;
        _next = next;
        if (prev) prev._next = this;
        if (next) next._prev = this;
        if (prev) 
        {
            GameObject pathSeg = Resources.Load<GameObject>("Prefabs/PathSegment");
            if (!pathSeg) return;
            _prev._pathToNext = Instantiate(pathSeg).GetComponent<PathSegment>();
            _prev._pathToNext.transform.parent = SegmentsContainer.Instance.transform;
            Vector2 startPt = new Vector2(_prev.transform.position.x, _prev.transform.position.z);
            Vector2 endPt    = new Vector2(transform.position.x, transform.position.z);
            if (_prev._pathToNext)
            {
                var points = AreaMap.Instance.BuildPath(startPt, endPt);
                if (points == null)
                {
                    _prev._pathToNext.BildPath(new List<Vector2>(new Vector2[] { startPt, endPt }), AreaMap.Instance);
                    return;
                }
                if (points.Count == 0)
                {
                    _prev._pathToNext.BildPath(new List<Vector2>(new Vector2[] { startPt, endPt }), AreaMap.Instance);
                    return;
                }
                _prev._pathToNext.BildPath(points, AreaMap.Instance);
            }
        }

    }

    public void Start()
    {
        _index = WayPointsIds.Count != 0? WayPointsIds.Pop() : WayPoints.Count; 

        WayPoints.Add(_index, this);

        transform.parent = PointsContainer.Instance.transform;

        Init(LastInstance, null);

        LastInstance = this;
    }
    public void OnDestroy()
    {
        WayPoints.Remove(_index);
        
        if (WayPoints.Count == 0) 
        {
            _wayPointsIds.Clear();
            return;
        }
        
        WayPointsIds.Push(_index);
        
        OnPointDelete();
    }
    private void OnDrawGizmos()
    {
        if (_next) Gizmos.DrawLine(transform.position, _next.transform.position);
    }

}