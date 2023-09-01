using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour
{
    private static CamController _instance;
    public static CamController Instance => _instance;
    public bool IsOrtho => _camera.orthographic;
    struct CameraSettings
    {
        public Vector3 position;
        public Vector3 rotation;
        public float orthoSize;
        public float fov;
    }
    CameraSettings _orthoState;
    CameraSettings _perspectiveState;
    private void ApplySettings(CameraSettings settings)
    {
        _camera.transform.position = settings.position;
        _camera.transform.eulerAngles = settings.rotation;
        _camera.fieldOfView = settings.fov;
        _camera.orthographicSize = settings.orthoSize;
    }
    private CameraSettings CollectSettings()
    {
        return new CameraSettings()
        {
            position = _camera.transform.position,
            rotation = _camera.transform.eulerAngles,
            fov = _camera.fieldOfView,
            orthoSize = _camera.orthographicSize
        };
    }
    public void SwitchProjection()
    {
        AreaMap.Instance.SwitchProjection();
        if (_camera.orthographic)
        {
            _orthoState = CollectSettings();
            _orthoState.position = new Vector3(_camera.transform.position.x, Mathf.Max(_camera.transform.position.y, 100.0f), _camera.transform.position.z);
            _camera.orthographic = false;
            ApplySettings(_perspectiveState);
            return;
        }
        _camera.orthographic = true;
        _perspectiveState = CollectSettings();
        ApplySettings(_orthoState);
        _camera.transform.forward = Vector3.down;
    }

    Vector3 _maxPosition;
    Vector3 _minPosition;
    [SerializeField]
    float _minOrtho;
    [SerializeField]
    float _maxOrtho;
    private Camera _camera;
    public Camera ControlledCamera => _camera;

    public void SetOrthoLimits(float min, float max)
    {
        _minOrtho = Mathf.Min(min, max);
        _maxOrtho = Mathf.Max(min, max);
    }

    public void SetupMovmentLimits(Vector3 min, Vector3 max)
    {
        _minPosition = Vector3.Min(min, max);
        _maxPosition = Vector3.Max(min, max);
        ClampCameraPosition();
    }

    public float MovementSpeed
    {
        get => _movementSpeed;
        set => _movementSpeed = Mathf.Clamp(value, 0.01f, 10f);
    }

    public float HorRotSpeed
    {
        get => _horRotSpeed;
        set => _horRotSpeed = Mathf.Clamp(value, 0.01f, 10f);
    }

    public float VertRotSpeed
    {
        get => _vertRotSpeed;
        set => _vertRotSpeed = Mathf.Clamp(value, 0.01f, 10f);
    }

    public bool VertInversion
    {
        get => _vertInversion;
        set
        {
            _vertInversion = value;
            _vertDirection = _vertInversion ? -1.0f : 1.0f;
        }
    }

    public bool HorInversion
    {
        get => _horInversion;
        set
        {
            _horInversion = value;
            _horDirection = _horInversion ? -1.0f : 1.0f;
        }
    }

    [SerializeField, Range(0.01f, 10f)]
    private float _movementSpeed;

    [SerializeField, Range(0.01f, 10f)]
    private float _horRotSpeed;

    [SerializeField, Range(0.01f, 10f)]
    private float _vertRotSpeed;

    [SerializeField]
    private bool _vertInversion = false;

    [SerializeField]
    private bool _horInversion = false;

    private float _vertDirection = 1.0f;

    private float _horDirection = 1.0f;

    private Vector3 _prevMousePos;
    private Vector3 _deltaMousePos = Vector3.zero;
    private Vector2 _currentRotation = Vector2.zero;

    private void CameraKeyboardMovement(float actualMovementSpeed)
    {
        if (_camera.orthographic)
        {
            if (Input.GetKey(KeyCode.W))
            {
                _camera.transform.position += _camera.transform.up * actualMovementSpeed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _camera.transform.position -= _camera.transform.up * actualMovementSpeed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                _camera.transform.position -= _camera.transform.right * actualMovementSpeed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                _camera.transform.position += _camera.transform.right * actualMovementSpeed;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                _camera.orthographicSize += actualMovementSpeed;
            }

            if (Input.GetKey(KeyCode.E))
            {
                _camera.orthographicSize -= actualMovementSpeed;
                _camera.orthographicSize = Mathf.Max(0.1f, _camera.orthographicSize);
            }
            return;
        }

        if (Input.GetKey(KeyCode.W))
        {
            _camera.transform.position += _camera.transform.forward * actualMovementSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _camera.transform.position -= _camera.transform.forward * actualMovementSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _camera.transform.position -= _camera.transform.right * actualMovementSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            _camera.transform.position += _camera.transform.right * actualMovementSpeed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            _camera.transform.position += _camera.transform.up * actualMovementSpeed;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _camera.transform.position -= _camera.transform.up * actualMovementSpeed;
        }
    }

    private void ClampCameraPosition()
    {
        _camera.transform.position = Vector3.Max(_camera.transform.position, _minPosition);
        _camera.transform.position = Vector3.Min(_camera.transform.position, _maxPosition);
    }
    private void CameraMouseMovement(float actualMovementSpeed)
    {
        float wheelDelta;

        if (Input.GetKey(KeyCode.Mouse2))
        {
            _camera.transform.position -= _deltaMousePos.y * actualMovementSpeed * _vertDirection * _camera.transform.up
                                          + _deltaMousePos.x * actualMovementSpeed * _horDirection * _camera.transform.right;
        }

        wheelDelta = Input.GetAxis("Mouse ScrollWheel");

        if (wheelDelta == 0.0f) return;

        if (_camera.orthographic)
        {
            _camera.orthographicSize += _camera.orthographicSize * wheelDelta;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minOrtho, _maxOrtho);
        }
        else
        {
            _camera.transform.position += _camera.transform.position.magnitude * _camera.transform.forward * wheelDelta;
        }
        ClampCameraPosition();
    }

    private void CameraRotation()
    {
        if (_camera.orthographic) return;
        if (!Input.GetKey(KeyCode.Mouse1)) return;
        _currentRotation.x += Input.GetAxis("Mouse X") * _horRotSpeed * _horDirection;
        _currentRotation.y -= Input.GetAxis("Mouse Y") * _vertRotSpeed * _vertDirection;
        _currentRotation.x = Mathf.Repeat(_currentRotation.x, 360);
        _currentRotation.y = Mathf.Clamp(_currentRotation.y, -80.0f, 80.0f);
        //_camera.transform.rotation = Quaternion.Euler(_currentRotation.y, _currentRotation.x + 180, 0);
        _camera.transform.rotation = Quaternion.Euler(_currentRotation.y, _currentRotation.x, 0);
    }

    void Start()
    {
        _camera = GetComponent<Camera>();
        _camera.enabled = true;
        _movementSpeed = 5.0f;
        _horRotSpeed = 5.0f;
        _vertRotSpeed = 5.0f;
        _prevMousePos = Input.mousePosition;
        _maxPosition = new Vector3(100, 100, 100);
        _minPosition = new Vector3(-100, -100, -100);
        _camera.transform.position = new Vector3(-10, 10, -10);
        _camera.transform.forward = -_camera.transform.position.normalized;
        _currentRotation = new Vector2(_camera.transform.eulerAngles.y, _camera.transform.eulerAngles.x);
        _orthoState = CollectSettings();
        _orthoState.position = new Vector3(0, 100, 0);
        _orthoState.rotation = new Vector3(0, 0, 0);
        _minOrtho = 0.1f;
        _maxOrtho = _camera.orthographicSize;
        _perspectiveState = CollectSettings();
    }

    private void OnEnable()
    {
        _currentRotation = _camera != null ? new Vector2(_camera.transform.eulerAngles.y, _camera.transform.eulerAngles.x) : new Vector2(0.0f, 0.0f);
        _prevMousePos = Input.mousePosition;
        _deltaMousePos = Vector3.zero;
        _instance = this;
    }

    private void Update()
    {
        _deltaMousePos = Input.mousePosition - _prevMousePos;
        _prevMousePos = Input.mousePosition;

        if (!_camera.enabled) return;

        float actualMovementSpeed = _movementSpeed * Time.deltaTime;

        CameraKeyboardMovement(actualMovementSpeed);
        CameraMouseMovement(actualMovementSpeed);
        CameraRotation();
    }

    private void OnValidate()
    {
        _vertDirection = _vertInversion ? -1.0f : 1.0f;
        _horDirection = _horInversion ? -1.0f : 1.0f;
    }
}