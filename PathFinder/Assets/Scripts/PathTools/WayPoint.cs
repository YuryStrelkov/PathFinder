using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    /*
     1 - операция вставки в конец; 
     2 - при перемещении делать запрос на перестройку пути сегмента слева, сегмента справа;
     3 - при удалении соединение предыдущего со следующим новым сегментом пути;
     */
    private static Dictionary<int, WayPoint> _wayPoints;
    private static Stack<int> _wayPointsIds;
    [SerializeField]
    Transform _pointVisualTransform;

    [SerializeField]
    private float _pointSize; /// = 1.0f;

    [SerializeField]
    private BoxCollider _collider;
    private Vector3 _position;

    public Vector3 ProjectedPosition => new Vector3(transform.position.x, AreaMap.Instance.Project(transform.position.x, transform.position.z), transform.position.z);

    public static Stack<int> WayPointsIds
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
    WayPoint _prev;
    WayPoint _next;
    PathSegment _pathToNext;
    public int Index => _index;
    public bool HasNext => _next != null;
    public bool HasPrev => _prev != null;

    public PathSegment PathToNext => _pathToNext;
    public WayPoint Next => _next;
    public WayPoint Prev => _prev;

    public void OnPositionHasChanged()
    {
        transform.position = ProjectedPosition;
        if (HasPrev) _prev.LinkToNext();
        LinkToNext();
    }

    public void OnMoveBegin() => _position = transform.position;

    public void OnMoveEnd()
    {
        if ((_position - transform.position).magnitude < 1e-3f) return;
        OnPositionHasChanged();
    }
    public void LinkToPrev(List<Vector2> wayPoints = null) 
    {
        if (!HasPrev) return;
        Prev.LinkToNext(wayPoints);
    }
    private static List<Vector2> LerpList(Vector2 from, Vector2 to, int steps = 64) 
    {
        List<Vector2> points = new List<Vector2>();
        steps = Mathf.Max(3, steps);
        float dt = 1.0f / (steps - 1);
        for (int i = 0; i < steps; i++) points.Add(Vector2.Lerp(from, to, dt * i));
        return points;
    }

    public void LinkToNext(List<Vector2> wayPoints = null)
    {
        if (!HasNext) if (_pathToNext) { Destroy(_pathToNext.gameObject); return; }

        if (!_pathToNext) return;

        Vector2 startPt = new Vector2(transform.position.x, transform.position.z);
        Vector2 endPt   = new Vector2(_next.transform.position.x, _next.transform.position.z);
        var points      = wayPoints == null ? null /*AreaMap.Instance.BuildPath(startPt, endPt)*/ : wayPoints;
        if (points == null)
        {
            _pathToNext.BildPath(LerpList(startPt, endPt), AreaMap.Instance);
            return;
        }
        if (points.Count == 0)
        {
            _pathToNext.BildPath(LerpList(startPt, endPt), AreaMap.Instance);
            return;
        }
        _pathToNext.BildPath(points, AreaMap.Instance);
    }
    public void OnPointDelete()
    {
        if (_prev != null) _prev._next = _next;
        if (_next != null) _next._prev = _prev;
        if (LastInstance) if (_index == LastInstance._index) LastInstance = Prev;
        if (_pathToNext) Destroy(_pathToNext.gameObject);
        if (_prev) _prev.LinkToNext();
    }

    private void Link(WayPoint prev = null, WayPoint next = null,  List<Vector2> pathPoints = null)
    {
        _prev = prev;
        _next = next;
        if (prev) prev._next = this;
        if (next) next._prev = this;
        if (prev)
        {
            GameObject pathSeg = Resources.Load<GameObject>("Prefabs/LineSegment/PathSegment");
            if (!pathSeg) return;
            _prev._pathToNext = Instantiate(pathSeg).GetComponent<PathSegment>();
            if (!_prev._pathToNext) return;
            _prev._pathToNext.transform.parent = SegmentsContainer.Instance.transform;
        }
    }
    private void FixedUpdate()
    {
        float size = CamController.Instance.IsOrtho ? CamController.Instance.ControlledCamera.orthographicSize * _pointSize :
                    (CamController.Instance.ControlledCamera.transform.position - transform.position).magnitude * _pointSize;

        _pointVisualTransform.localScale = Mathf.Clamp(size, 0.25f, 100.0f) * Vector3.one;
        _collider.size = _pointVisualTransform.localScale;
    }
    public void Awake()
    {
        _index = WayPointsIds.Count != 0 ? WayPointsIds.Pop() : WayPoints.Count;

        WayPoints.Add(_index, this);

        transform.parent = PointsContainer.Instance.transform;

        _pointSize = 0.075f;

        transform.position = ProjectedPosition;
        
        _collider = GetComponent<BoxCollider>();
        
        Link(LastInstance, null, null);

        LastInstance = this;
    }
    public void OnDestroy()
    {
        OnPointDelete();

        WayPoints.Remove(_index);

        if (WayPoints.Count == 0)
        {
            _wayPointsIds.Clear();
            return;
        }

        WayPointsIds.Push(_index);
    }
    private void OnDrawGizmos()
    {
        if (_next) Gizmos.DrawLine(transform.position, _next.transform.position);
    }

}