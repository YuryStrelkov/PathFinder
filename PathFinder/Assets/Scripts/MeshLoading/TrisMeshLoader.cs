using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TrisMeshLoader : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _meshFilter   = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter.sharedMesh = ObjMesh.ReadObjMesh("E:\\GitHub\\PathFinder\\PathFinder\\Assets\\Resources\\3DMap\\big_map.obj")[0].ToUnityMesh();
    }
}
