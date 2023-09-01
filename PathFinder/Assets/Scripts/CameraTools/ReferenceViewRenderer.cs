using UnityEngine;

public class ReferenceViewRenderer : MonoBehaviour
{
    public enum SaveTextureFileFormat
    {
        EXR, JPG, PNG, TGA
    };
    public enum ReferenceViewTextureResoultion
    {
        low = 256,
        middle = 512,
        height = 1024,
        veryHeight = 2048
    }
    private ReferenceViewTextureResoultion _RT_TextureResoution = ReferenceViewTextureResoultion.middle;

    private RenderTexture _RT_Target;
    [SerializeField]
    protected Camera _camera;

    protected float _aspect = 1.0f;
    public float Aspect 
    {
        get => _aspect;
        set
        {
            _aspect = Mathf.Clamp(value, 0.01f, 100.0f);
            InitRenderTexture();
        }
    }

    public Matrix4x4 TransformMatrix => transform.localToWorldMatrix;
    public Matrix4x4 ProjectionMatrix => _camera.projectionMatrix;
    protected void InitRenderTexture()
    {
        if (_RT_Target != null) DisposeRenderTexture();
        int resolution = (int)_RT_TextureResoution;
        if (_aspect > 1.0f)
        {
            _RT_Target = new RenderTexture(resolution, (int)(resolution / _aspect), 16, RenderTextureFormat.ARGB32);
        }
        else 
        {
            _RT_Target = new RenderTexture((int)(resolution / _aspect), resolution, 16, RenderTextureFormat.ARGB32);
        }
        _RT_Target.Create();
        _camera.targetTexture = _RT_Target;
    }
    private void DisposeRenderTexture() => _RT_Target.Release();
    public ReferenceViewTextureResoultion Resoultion
    {
        get => _RT_TextureResoution;
        set
        {
            if (_RT_TextureResoution == value) return;
            _RT_TextureResoution = value;
            InitRenderTexture();
        }
    }
    public Texture2D ToUnityTexture() 
    {
        var currentRT = RenderTexture.active;
        Texture2D tex = new Texture2D(_RT_Target.width, _RT_Target.height, TextureFormat.ARGB32, false);
        RenderTexture.active = _RT_Target;
        tex.ReadPixels(new Rect(0, 0, _RT_Target.width, _RT_Target.height), 0, 0);
        RenderTexture.active = currentRT;
        tex.Apply();
        return tex;
    }
    public void SaveViewTexture(string filePath) //, SaveTextureFileFormat fileFormat) 
    {
        var rawPath = filePath.Split('.');

        string fileFormat = rawPath[rawPath.Length - 1];

        switch (fileFormat)
        {
            case "exr":
                System.IO.File.WriteAllBytes(filePath + ".exr", ToUnityTexture().EncodeToEXR());
                break;
            case "jpg":
                System.IO.File.WriteAllBytes(filePath + ".jpg", ToUnityTexture().EncodeToJPG(/*jpgQuality*/ 95));
                break;
            case "png":
                System.IO.File.WriteAllBytes(filePath + ".png", ToUnityTexture().EncodeToPNG());
                break;
            case "tga":
                System.IO.File.WriteAllBytes(filePath + ".tga", ToUnityTexture().EncodeToTGA());
                break;
            default:
                System.IO.File.WriteAllBytes(filePath + ".png", ToUnityTexture().EncodeToPNG());
                break;
        }
    }
    public void RenderReferenceView() 
    {   
        if (AreaMap.Instance.DefaultRenderer == null) return;
        if (AreaMap.Instance.HeightsRenderer == null) return;
        if (AreaMap.Instance.HeatRenderer    == null) return;
        bool defaultRendererState = AreaMap.Instance.DefaultRenderer.gameObject.activeInHierarchy;
        bool heightRendererState  = AreaMap.Instance.HeightsRenderer.gameObject.activeInHierarchy;
        bool heatRendererState    = AreaMap.Instance.HeatRenderer   .gameObject.activeInHierarchy;

        AreaMap.Instance.HeightsRenderer.gameObject.SetActive(false);
        AreaMap.Instance.HeatRenderer.   gameObject.SetActive(false);
        AreaMap.Instance.DefaultRenderer.gameObject.SetActive(true);
        _camera.gameObject.SetActive(true);
        _camera.Render();// DontRestore();
        _camera.gameObject.SetActive(false);
        AreaMap.Instance.HeightsRenderer.gameObject.SetActive(heightRendererState);
        AreaMap.Instance.HeatRenderer.   gameObject.SetActive(heatRendererState);
        AreaMap.Instance.DefaultRenderer.gameObject.SetActive(defaultRendererState);
    }
    void Start()
    {
        // _camera = GetComponentInChildren<Camera>();
        if (_camera == null) return;
        _aspect = 1.0f;
        InitRenderTexture();
        RenderReferenceView();
    }

    public void OnDestroy() => DisposeRenderTexture();
}
