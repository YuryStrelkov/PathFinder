using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MapRenderer : MonoBehaviour
{
    private MeshFilter   _meshFilter;
    private MeshRenderer _meshRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        _meshFilter   = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetMesh(Mesh mesh)
    {
        _meshFilter.mesh = mesh;
        SetBounds(mesh.bounds.min, mesh.bounds.max);
    }

    public void SetBounds(Vector3 min, Vector3 max) 
    {
        _meshRenderer.material.SetVector("_MinBound", new Vector4(min.x, min.y, min.z, 1.0f));
        _meshRenderer.material.SetVector("_MaxBound", new Vector4(max.x, max.y, max.z, 1.0f));
    }
}
