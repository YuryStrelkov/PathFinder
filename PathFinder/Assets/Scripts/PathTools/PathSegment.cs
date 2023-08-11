using System.Collections.Generic;
using UnityEngine;
using static ObjMesh;

public struct CircularBuffer
{
    private float[] _buffer;

    private int _indent;

    private int _n_items;
    public int Capacity => _buffer.Length;
    public int NItems => _n_items;
    private int Index(int index) => (index + _indent) % Capacity;
    public float Peek
    {
        get
        {
            if (NItems == 0) throw new System.IndexOutOfRangeException("CircularBuffer :: IndexOutOfRangeException");
            return _buffer[Index(NItems - 1)];
        }
    }
    public float Pop
    {
        get
        {
            float value = Peek;
            _n_items -= 1;
            return value;
        }
    }
    public void Clear()
    {
        _indent = 0;
        _n_items = 0;
    }
    public void Append(float value)
    {
        _buffer[Index(NItems)] = value;
        if (NItems != Capacity)
        {
            _n_items += 1;
        }
        else
        {
            _indent += 1;
            _indent %= Capacity;
        }
    }
    public float this[int index]
    {
        get
        {
            if (index >= NItems || index < 0) throw new System.IndexOutOfRangeException("CircularBuffer :: IndexOutOfRangeException");
            return _buffer[Index(index)];
        }
        set
        {
            if (index >= NItems || index < 0) throw new System.IndexOutOfRangeException("CircularBuffer :: IndexOutOfRangeException");
            _buffer[Index(index)] = value;
        }
    }

    public CircularBuffer(int cap = 32)
    {
        _buffer = new float[cap];
        _indent = 0;
        _n_items = 0;
    }
}
public struct RunningAverage
{
    public int WindowSize
    {
        get => _values.Capacity;
        set
        {
            _values = new CircularBuffer(Mathf.Max(3, value));
            _sum = 0.0f;
        }
    }

    private float _sum;
    private CircularBuffer _values;
    public float Value => _sum / _values.NItems;
    public void Reset()
    {
        _sum = 0.0f;
        _values.Clear();
    }
    public RunningAverage(int windowSize = 32)
    {
        _values = new CircularBuffer(windowSize);
        _sum = 0.0f;
    }
    public float Call(float value)
    {
        if (_values.NItems == _values.Capacity) _sum -= _values[0];
        _values.Append(value);
        _sum += value;
        return _sum / _values.NItems;
    }
}
public struct RunningAverage3
{
    RunningAverage _x;
    RunningAverage _y;
    RunningAverage _z;
    public int WindowSize
    {
        get => _x.WindowSize;
        set
        {
            _x.WindowSize = value;
            _y.WindowSize = value;
            _z.WindowSize = value;
        }
    }
    public void Reset()
    {
        _x.Reset();
        _y.Reset();
        _z.Reset();
    }

    public RunningAverage3(int windowSize = 32)
    {
        _x = new RunningAverage(windowSize);
        _y = new RunningAverage(windowSize);
        _z = new RunningAverage(windowSize);
    }

    public Vector3 Call(Vector3 value)
    {
        return new Vector3(_x.Call(value.x),
                           _y.Call(value.y),
                           _z.Call(value.z));
    }
}

public struct RunningAverage2
{
    RunningAverage _x;
    RunningAverage _y;
    public int WindowSize
    {
        get => _x.WindowSize;
        set
        {
            _x.WindowSize = value;
            _y.WindowSize = value;
        }
    }
    public void Reset()
    {
        _x.Reset();
        _y.Reset();
    }

    public RunningAverage2(int windowSize = 32)
    {
        _x = new RunningAverage(windowSize);
        _y = new RunningAverage(windowSize);
    }

    public Vector2 Call(Vector2 value) => new Vector2(_x.Call(value.x), _y.Call(value.y));
}


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshFilter))]
public class PathSegment : MonoBehaviour
{
    private struct PathSegmentSettings
    {
        public int Decimate;
        public float LineWidth;
        public int AvgPointsCount;
    }
   
    private List<Vector2>       _points;
    private List<Vector2>       _pointsProcessed;
    private PathSegmentSettings _settings;
    private MeshFilter          _meshFilter;
    private MeshRenderer        _meshRenderer;
    [SerializeField]
    private int   _decimate;
    [SerializeField]
    private float _lineWidth = 0.05f;
    [SerializeField]
    private int   _avgPointsCount;
    public float Length { get; private set; }
    public float LengthProcessed { get; private set; }
    public int Decimate
    {
        get => _decimate;
        set
        {
            _decimate = Mathf.Max(1, value);
            UpdatePath();
            UpdateSettings();
        }
    }
    public float LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = Mathf.Max(0.01f, value);
            RebuildPathMesh();
            UpdateSettings();
        }
    }
    public int AvgPointsCount
    {
        get => _avgPointsCount;
        set 
        {
            _avgPointsCount = Mathf.Max(3, 64);
            UpdatePath();
            UpdateSettings();
        }
    }

    public List<Vector2> PathPointsRaw => _points;
    public List<Vector2> PathPointsProcessed => _pointsProcessed;

    // Start is called before the first frame update
    void Awake()
    {
        _meshFilter   = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _points = new List<Vector2>();
        _avgPointsCount = 16;
        _decimate = 0;
    }

    private void OnValidate()
    {
        if (_settings.LineWidth != LineWidth) 
        {
            RebuildPathMesh();
            UpdateSettings();
            return;
        }
        if (_settings.AvgPointsCount != AvgPointsCount)
        {
            UpdatePath();
            UpdateSettings();
            return;
        }
        if (_settings.Decimate != Decimate)
        {
            UpdatePath();
            UpdateSettings();
            return;
        }
    }
    private void UpdateSettings()
    {
        _settings = new PathSegmentSettings { AvgPointsCount = _avgPointsCount, Decimate = _decimate, LineWidth = _lineWidth };
    }
    private List<Vector2> DecimatePoints()
    {
        // List<Vector2>  processed = new List<Vector2>();
        // for (int i = 0; i < _points.Count; i += Decimate) processed.Add(_points[i]);
        // if (_points[_points.Count - 1] != processed[processed.Count - 1]) processed.Add(_points[_points.Count - 1]);
        return _points;
    }
    private List<Vector2> SmoothPoints(List<Vector2> processed)
    {
        RunningAverage2 smoothPoints = new RunningAverage2(_avgPointsCount);
        if (_pointsProcessed == null) _pointsProcessed = new List<Vector2>();
        _pointsProcessed.Clear();
        foreach (Vector2 p in processed) _pointsProcessed.Add(smoothPoints.Call(p));
        _pointsProcessed.Add(processed[processed.Count - 1]);
        for (int i = 0; i < _pointsProcessed.Count - 1; i++) LengthProcessed += (_pointsProcessed[i] - _pointsProcessed[i + 1]).magnitude;
        return _pointsProcessed;
    }

    public void UpdatePath(IProjectorXZ projector = null)
    {
        ProcessPath();
        RebuildPathMesh(projector);
    }
    private List<Vector2> ProcessPath()
    {
        return SmoothPoints(DecimatePoints());
    }
    public void BildPath(List<Vector2> points, IProjectorXZ projector = null)
    {
        _points = points;
        for (int i = 0; i < _points.Count - 1; i++) Length += (_points[i] - _points[i + 1]).magnitude;
        ProcessPath();
        RebuildPathMesh(projector);
    }
    private void RebuildPathMesh(IProjectorXZ projector=null) 
    {
        if (_pointsProcessed == null) return;
        if (_pointsProcessed.Count  == 0) return;
        Mesh polyStrip = ObjMesh.CreatePolyStrip(_pointsProcessed, _lineWidth, null, projector).ToUnityMesh();
        _meshFilter.sharedMesh = polyStrip;
        /// transform.position = Vector3.up * 0.001f;
    }
}
