using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathOfPoints : MonoBehaviour
{
    private WayPoint _start;
    private int _nPoints;
    private WayPoint _current;
    private WayPoint _last;

    public void RequestPathSegment(WayPoint from, WayPoint to)
    {

    }

    public void AddPoint(Vector3 point)
    {
        GameObject gameObject = new GameObject($"PathPoint{_nPoints}");
        WayPoint _pathPoint = gameObject.AddComponent<WayPoint>();
        if (_nPoints == 0) _start = _pathPoint;
        // _pathPoint.Init(this, _current, null);
        _nPoints++;
        _current = _pathPoint;
        _last = _pathPoint;
        _pathPoint.transform.position = point;
        _pathPoint.OnPositionHasChanged();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        WayPoint point = _start;
        if (point == null) return;
        while (point.HasNext)
        {
            Gizmos.DrawLine(point.transform.position, point.Next.transform.position);
            point = point.Next;
        }
    }

    void Start()
    {
        _start = null;
        _current = null;
        _last = null;
    }
}