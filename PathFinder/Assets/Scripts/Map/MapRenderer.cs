using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MapRenderer : MonoBehaviour
{
    private MeshFilter   _meshFilter;
    private MeshRenderer _meshRenderer;
    public Vector3 MeshSize => _meshFilter.sharedMesh == null ? Vector3.zero : _meshFilter.sharedMesh.bounds.size;
    public Vector3 MeshCenter => _meshFilter.sharedMesh == null ? Vector3.zero : _meshFilter.sharedMesh.bounds.center;
    public string MeshName => _meshFilter.sharedMesh == null ? "no mesh!": _meshFilter.sharedMesh.name;

    public Texture MainTexture
    {
        get => _meshRenderer.sharedMaterial == null ? null : _meshRenderer.sharedMaterial.mainTexture;
        set 
        {
            if (_meshRenderer.sharedMaterial == null) return;
            _meshRenderer.sharedMaterial.mainTexture = value;
        }
    }
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
