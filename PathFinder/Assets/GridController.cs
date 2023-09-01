using UnityEngine;
using TMPro;

public class GridController : MonoBehaviour
{
    public static GridController Instance => _instance;

    private static GridController _instance;
    private void Awake() => _instance = this;

    Transform _xLabel;
    Transform _yLabel;

    [SerializeField]
    TMP_Text _stepLabel;
    [SerializeField]
    GameObject _grid;

    private Bounds _xLabelBoubnds;

    private Bounds _yLabelBoubnds;

    private static readonly float UnityPlaneHalfSize = 5.0f;
    void Start() 
    {
        _xLabel = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Markers/XMarker")).transform;
        _yLabel = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Markers/YMarker")).transform;
        _xLabel.parent = transform;
        _yLabel.parent = transform;
        MeshFilter mf;
        if ((mf = _xLabel.GetComponentInChildren<MeshFilter>()) == null)
        {
            _xLabelBoubnds = new Bounds(Vector3.zero, Vector3.one);
        }
        else 
        {
            Vector3 scale = _xLabel.lossyScale;
            Vector3 size  = mf.sharedMesh.bounds.size;
            Vector3 centr = mf.sharedMesh.bounds.center;
            scale.x *= size.x;
            scale.y *= size.y;
            scale.z *= size.z;
            _xLabelBoubnds = new Bounds(centr, scale);
        }

        if ((mf = _yLabel.GetComponentInChildren<MeshFilter>()) == null)
        {
            _yLabelBoubnds = new Bounds(Vector3.zero, Vector3.one);
        }
        else
        {
            Vector3 scale = _yLabel.lossyScale;
            Vector3 size  = mf.sharedMesh.bounds.size;
            Vector3 centr = mf.sharedMesh.bounds.center;
            scale.x *= size.x;
            scale.y *= size.y;
            scale.z *= size.z;
            _yLabelBoubnds = new Bounds(centr, scale);
        }

        SetupSize(new Vector2(10, 10));
    }
    public void SetupSize(Vector2 size)
    {
        if (!_grid) return;
        _grid.transform.localScale = new Vector3(size.x * 0.5f / UnityPlaneHalfSize + 1.0f / UnityPlaneHalfSize, 1.0f, size.y * 0.5f / UnityPlaneHalfSize + 1.0f / UnityPlaneHalfSize);

        if (_xLabel) _xLabel.position = new Vector3(size.x * 0.5f + _xLabelBoubnds.size.z * 1.1f, 0.0f, 0.0f);

        if (_yLabel) _yLabel.position = new Vector3(0.0f, 0.0f, size.y * 0.5f + _yLabelBoubnds.size.x * 1.5f);

        if (_stepLabel) _stepLabel.transform.position = new Vector3(0, 0, -UnityPlaneHalfSize * _grid.transform.lossyScale.z);
    }
}
