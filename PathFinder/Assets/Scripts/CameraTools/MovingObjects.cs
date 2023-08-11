using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MovingObjects : MonoBehaviour
{
    private Camera _camera;
    private Plane _plane = new Plane(Vector3.up, Vector3.zero);
    private Vector3 _clickDistance;
    private WayPoint _object = null;

    private void ScanSceneAlongCameraRay()
    {
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, 10000)) return;
        _object = hit.transform.gameObject.GetComponent<WayPoint>();
        if (!_object) return;
        float t;
        _plane.Raycast(ray, out t);
        _clickDistance = ray.origin + ray.direction * t - hit.transform.position;
        _object.OnMoveBegin();
    }

    private void MoveObject()
    {
        if (_object == null) return;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        float t;
        _plane.Raycast(ray, out t);
        Vector3 rayEnd = ray.origin + ray.direction * t;
        if (!AreaMap.Instance.EvaluateWeight(rayEnd, out t)) return;
        _object.transform.position = rayEnd - _clickDistance;// + t * Vector3.up;
    }

#if UNITY_EDITOR
    private Ray ray_1;
    private Ray ray_2;
    private Ray ray_3;
    private Ray ray_4;
    private void OnDrawGizmos()
    {
        ray_1 = _camera.ScreenPointToRay(new Vector3(0, 0, 0));
        ray_2 = _camera.ScreenPointToRay(new Vector3(0, _camera.pixelHeight, 0));
        ray_3 = _camera.ScreenPointToRay(new Vector3(_camera.pixelWidth, 0, 0));
        ray_4 = _camera.ScreenPointToRay(new Vector3(_camera.pixelWidth, _camera.pixelHeight, 0));

        float t;
        Gizmos.color = Color.red;
        _plane.Raycast(ray_1, out t);
        Gizmos.DrawLine(ray_1.origin, ray_1.origin + ray_1.direction * t);

        _plane.Raycast(ray_2, out t);
        Gizmos.DrawLine(ray_2.origin, ray_2.origin + ray_2.direction * t);

        _plane.Raycast(ray_3, out t);
        Gizmos.DrawLine(ray_3.origin, ray_3.origin + ray_3.direction * t);

        _plane.Raycast(ray_4, out t);
        Gizmos.DrawLine(ray_4.origin, ray_4.origin + ray_4.direction * t);
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (!CamController.Instance.IsOrtho) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ScanSceneAlongCameraRay();
            return;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            MoveObject();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _clickDistance = Vector3.zero;
            if(_object)_object.OnMoveEnd();
            _object = null;
        }
    }
}
