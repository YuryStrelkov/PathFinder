using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SurfaceSlider : MonoBehaviour
{
    [SerializeField] Texture2D _heightMap;
    [SerializeField, Range(0.1f, 1000.0f)] float _heightAmplitude;
    private static SurfaceSlider _instance;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    [SerializeField, Range(0.1f, 1000.0f)] float _heightThreshold;

    public static SurfaceSlider Instance => _instance;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public bool EvaluateHeight(Vector3 position, out float height)
    {
        height = 0.0f;
        if (_heightMap == null) return false;
        if (_meshFilter.mesh == null) return false; 
        position = transform.InverseTransformPoint(position);
        Bounds bounds = _meshFilter.mesh.bounds;
        if (!bounds.Contains(position)) return false;
        position -= bounds.center;
        height = _heightAmplitude * _heightMap.GetPixelBilinear(position.x / bounds.size.x + 0.5f, position.z / bounds.size.z + 0.5f).r;
        return height < _heightThreshold;
    }

    void Awake()
    {
        _heightAmplitude = 5.0f;
        _heightThreshold = 2.5f;
        _instance = this; 
    }
}
