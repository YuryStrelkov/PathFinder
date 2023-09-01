using UnityEngine;

public class ReferenceViewRenderer : MonoBehaviour
{
    public enum SaveTextureFileFormat
    {
        EXR, JPG, PNG, TGA
    };
    public enum ReferenceViewTextureResoultion
    {
        low,
        middle,
        height,
        veryHeight
    }
    private ReferenceViewTextureResoultion _RT_TextureResoution;
   
    private RenderTexture _RT_Target;

    private Camera _camera;
    private void InitRenderTexture()    
    {
        if (_RT_Target != null) DisposeRenderTexture();
        int resolution = 512;
        switch (_RT_TextureResoution) 
        {
            case ReferenceViewTextureResoultion.low:        resolution =  256; break;
            case ReferenceViewTextureResoultion.middle:     resolution =  512; break;
            case ReferenceViewTextureResoultion.height:     resolution = 1024; break;
            case ReferenceViewTextureResoultion.veryHeight: resolution = 2048; break;
        }
        _RT_Target = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32);
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
            InitRenderTexture();
        }
    }

    public void SaveViewTexture(string filePath) //, SaveTextureFileFormat fileFormat) 
    {
        var currentRT = RenderTexture.active;
        Texture2D tex = new Texture2D(_RT_Target.width, _RT_Target.height, TextureFormat.ARGB32, false);
        RenderTexture.active = _RT_Target;
        tex.ReadPixels(new Rect(0, 0, _RT_Target.width, _RT_Target.height), 0, 0);
        tex.Apply();
        System.IO.File.WriteAllBytes(filePath, tex.EncodeToPNG());

        var rawPath = filePath.Split('.');

        string fileFormat = rawPath[rawPath.Length - 1];

        switch (fileFormat)
        {
            case "exr":
                System.IO.File.WriteAllBytes(filePath + ".exr", tex.EncodeToEXR());
                break;
            case "jpg":
                System.IO.File.WriteAllBytes(filePath + ".jpg", tex.EncodeToJPG(/*jpgQuality*/ 95));
                break;
            case "png":
                System.IO.File.WriteAllBytes(filePath + ".png", tex.EncodeToPNG());
                break;
            case "tga":
                System.IO.File.WriteAllBytes(filePath + ".tga", tex.EncodeToTGA());
                break;
            default: System.IO.File.WriteAllBytes(filePath + ".png", tex.EncodeToPNG());
                break;
        }
        RenderTexture.active = currentRT; 
    }
    public void RenderReferenceView() => _camera.Render();

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        InitRenderTexture();
        RenderReferenceView();
    }

    public void OnDestroy() => DisposeRenderTexture();

    // // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
