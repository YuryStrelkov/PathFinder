using UnityEngine;

[ExecuteInEditMode]
public class WeightMapRenderer : MonoBehaviour
{
    private Camera   _camera;
    [SerializeField]
    private Material _material;
    [SerializeField]
    RenderResolutions _textureResolution = RenderResolutions.Middle;
    RenderTexture _texture;

    enum RenderResolutions
    {
        Minimal = 128,
        Middle  = 512,
        Big     = 1024,
        Huge    = 2048
    }

    void CreateRenderTexture(RenderResolutions resolution)
    {
        if (_camera == null)
            return;

        if (_texture == null) 
        {
            _texture = new RenderTexture((int)resolution, (int)resolution, 8);
            _camera.targetTexture = _texture;
            return;
        }
        if (_texture.width == (int)resolution) 
        {
            return;
        }
        _texture.Release();
        _texture = new RenderTexture((int)resolution, (int)resolution, 8);
        _camera.targetTexture = _texture;
    }

    private void OnValidate()
    {
        CreateRenderTexture(_textureResolution);
    }

    public Material WeightsMaterial
    {
        get 
        {
            return _material;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
        if (_material == null) _material = new Material(Shader.Find("MapUtilsShaders/weightsMapShader"));
        _camera.SetReplacementShader(_material.shader, "");
        CreateRenderTexture(_textureResolution);
    }

    private void RenderImage()
    {
        _camera.RenderWithShader(_material.shader, "");
    }
    // Update is called once per frame
    void Update()
    {
        RenderImage();
    }
}
