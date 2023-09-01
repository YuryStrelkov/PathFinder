using UnityEngine;

[ExecuteInEditMode]
public class WeightMapRenderer : ReferenceViewRenderer
{
    private static WeightMapRenderer _instance;

    public static WeightMapRenderer Instance => _instance;

    private void Awake()
    {
        _instance = this;
        _instance._aspect = 1.0f;
        _instance.InitRenderTexture();
    }

    public new void RenderReferenceView()
    {
        if (_camera == null) return;
        if (AreaMap.Instance.DefaultRenderer == null) return;
        if (AreaMap.Instance.HeightsRenderer == null) return;
        if (AreaMap.Instance.HeatRenderer    == null) return;

        bool defaultRendererState = AreaMap.Instance.DefaultRenderer.gameObject.activeInHierarchy;
        bool heightRendererState = AreaMap.Instance.HeightsRenderer.gameObject.activeInHierarchy;
        bool heatRendererState = AreaMap.Instance.HeatRenderer.gameObject.activeInHierarchy;

        AreaMap.Instance.HeightsRenderer.gameObject.SetActive(true);
        AreaMap.Instance.HeatRenderer.gameObject.SetActive(false);
        AreaMap.Instance.DefaultRenderer.gameObject.SetActive(false);
        _camera.gameObject.SetActive(true);
        _camera.Render();// DontRestore();
        _camera.gameObject.SetActive(false);
        AreaMap.Instance.HeightsRenderer.gameObject.SetActive(heightRendererState);
        AreaMap.Instance.HeatRenderer.gameObject.SetActive(heatRendererState);
        AreaMap.Instance.DefaultRenderer.gameObject.SetActive(defaultRendererState);
    }
}
