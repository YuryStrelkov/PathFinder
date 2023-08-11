using UnityEngine;

public class ObjectCreater : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] GameObject _prefab;
    private float _lastClickTime = 0.0f;
    private float _doubleClickTime = 0.2f;
    private Plane plane = new Plane(Vector3.up, Vector3.zero);
    private bool IsDoubleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return false;
        float timeSinceLastClick = Time.time - _lastClickTime;
        if (timeSinceLastClick <= _doubleClickTime)
            return true;
        _lastClickTime = Time.time;
        return false;
    }
    private WayPoint IsClickedOnPoint()
    {
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, 10000)) return null;
        return hit.transform.gameObject.GetComponent<WayPoint>();
    }
    private void NewObject()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        float t;
        plane.Raycast(ray, out t);
        Vector3 end = ray.origin + ray.direction * t;

        GameObject gameObject = _prefab == null ? GameObject.CreatePrimitive(PrimitiveType.Cube) : GameObject.Instantiate(_prefab);
        gameObject.transform.position = end;
    }

    void Start() =>_camera = GetComponent<Camera>();

    // Update is called once per frame
    void Update()
    {
        if(!CamController.Instance.IsOrtho) return;
        if (!IsDoubleClick()) return;
        _lastClickTime = 0.0f;
        WayPoint clicked = IsClickedOnPoint();
        if (clicked)
        {
            GameObject.Destroy(clicked.gameObject);
            return;
        }
        NewObject();
    }
}
