using Dummiesman;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ObjMesh;

// [RequireComponent(typeof(MeshFilter))]
// [RequireComponent(typeof(MeshRenderer))]
public class AreaMap : MonoBehaviour, IProjectorXZ
{
    [System.Serializable]
    private struct AreaMapSettings
    {
        public string mapModel;
        public string mapWeights;          
        public float mapWeightsThresshold;
        public int mapWeightsResolution;
        public Vector3 mapSize;
        public Vector3 mapOrig;             
    }
    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public static readonly float MAX_WEIGHT = WeightsMap.MAX_WEIGHT;
    public static readonly float MIN_WEIGHT = WeightsMap.MIN_WEIGHT;
    [SerializeField]
    MapRenderer _heightsRenderer;
    [SerializeField]
    MapRenderer _heatRenderer;
    [SerializeField]
    MapRenderer _defaultRenderer;
    [SerializeField]
    GridController _gridController;

    private static AreaMap _instance;
    // private MeshFilter     _meshFilter;
    // private MeshRenderer   _meshRenderer;
    [SerializeField]
    AStar _pathFinder;

    [SerializeField]
    Texture2D _wightsMap;
    [SerializeField, Range(0.1f, 100.0f)] 
    float _heightAmplitude;
    [SerializeField, Range(0.1f, 100.0f)]
    float _heightThreshold;
    private Vector2 _size;
    private float _heightMax;
    private float _heightMin;

    public Vector2 Origin { get; set; }
    public Vector2 Size
    {
        get => _size;
        set => _size = Vector2.Max(value, Vector2.zero);
    }

    public float HeightAmplitude
    {
        get => _heightAmplitude;
        set 
        {
            _heightAmplitude = Mathf.Clamp(value, MIN_WEIGHT, MAX_WEIGHT);
        }
    }

    public float HeightThreshold
    {
        get => _heightThreshold;
        set
        {
            _heightThreshold = Mathf.Clamp(value, MIN_WEIGHT, MAX_WEIGHT);
        }
    }
    public static AreaMap Instance => _instance;
    private void InitAStar()
    {
        if (_wightsMap != null) // ; && _meshFilter != null)
        {
            _pathFinder = new AStar(_wightsMap);
            // var boubds = _meshFilter.mesh.bounds;
            _pathFinder.Size = Size  ; // new Vector2(boubds.size.x, boubds.size.z);
            _pathFinder.Orig = Origin; // new Vector2(boubds.center.x, boubds.center.z);
        }
    }
    private void Start()
    {
        _heightsRenderer.gameObject.SetActive(false);
        _heatRenderer.gameObject.SetActive(false);
        _defaultRenderer.gameObject.SetActive(true);
        InitAStar();
    }
    // Start is called before the first frame update
    public bool EvaluateWeight(Vector3 position, out float height)
    {
        height = 0.0f;
        if (_pathFinder == null) return false;
        height = _pathFinder.GetWeight(position);
        return height < _heightThreshold;
    }

    public float Project(float x, float z)
    {
        if (_pathFinder == null) return 0.0f;
        return _pathFinder.GetWeightNormalized(new Vector2(x, z)) * (_heightMax - _heightMin) + _heightMin + 0.1f;
    }

    private void OnValidate()
    {
        InitAStar();
    }

    public void SwitchProjection() 
    {
        _heightsRenderer.gameObject.SetActive(!_heightsRenderer.gameObject.activeInHierarchy);
        _defaultRenderer.gameObject.SetActive(!_defaultRenderer.gameObject.activeInHierarchy);
    }

    public List<Vector2> BuildPath(Vector2 from, Vector2 to) 
    {
        if (_pathFinder == null) 
        {
            return new List<Vector2>(new Vector2[] { from, to });
        }

        return _pathFinder.Search(from, to);
    }
    public bool LoadMap(string mapDirectory)
    {
        //  using (StreamWriter writer = new StreamWriter("D:\\UNITY_projects\\PathFinder\\PathFinder\\PathFinderBuild\\run.txt"))
        {
            if (!Directory.Exists(mapDirectory)) return false;

            string mapSettingsPath = mapDirectory.EndsWith("\\") ? mapDirectory + "map_settings.json" : mapDirectory + "\\map_settings.json";
            // writer.WriteLine($"Map settings {mapSettingsPath}");

            if (!File.Exists(mapSettingsPath)) return false;
            AreaMapSettings settings = JsonUtility.FromJson<AreaMapSettings>(File.ReadAllText(mapSettingsPath));
            if (settings.mapModel == "") return false;

            string mapModelPath = mapDirectory.EndsWith("\\") ? mapDirectory + settings.mapModel : mapDirectory + "\\" + settings.mapModel;
            
           // writer.WriteLine($"Map model {mapModelPath}");

            // List<ObjMesh> meshes = ObjMesh.ReadObjMesh(mapModelPath);
            try
            {
                GameObject newMesh = new OBJLoader().Load(mapModelPath);
                // OBJObjectBuilder:
                // material = OBJLoaderHelper.CreateNullMaterial();
                // material.name = kvp.Key;
                MeshFilter mesh = newMesh.GetComponentInChildren<MeshFilter>();
                if (mesh == null) return false;
                _heightsRenderer.SetMesh(mesh.mesh);
                _heatRenderer.SetMesh(mesh.mesh);
                _defaultRenderer.SetMesh(mesh.mesh);
                Destroy(newMesh);
                newMesh.SetActive(false);

                Size = new Vector2(settings.mapSize.x, settings.mapSize.z);
                _gridController.SetupSize(new Vector2(Mathf.Ceil(Size.x), Mathf.Ceil(Size.y)));
                Origin = new Vector2(settings.mapOrig.x, settings.mapOrig.z);
                _heightMin = -0.5f * settings.mapSize.y + settings.mapOrig.y;
                _heightMax = 0.5f * settings.mapSize.y + settings.mapOrig.y;
                string mapWeightsPath = mapDirectory.EndsWith("\\") ? mapDirectory + settings.mapWeights : mapDirectory + "\\" + settings.mapWeights;
                // writer.WriteLine($"Map weights {mapWeightsPath}");
                _wightsMap = LoadPNG(mapWeightsPath);
                InitAStar();
            } catch (System.Exception ex) 
            {
                // writer.WriteLine($"Exception {ex.StackTrace}");
            }
            return true;
        }


    }

    void Awake()
    {
        _heightAmplitude = MAX_WEIGHT;
        _heightThreshold = MAX_WEIGHT * 0.5f;
        _instance = this;
        Size = new Vector3(10.0f, 10.0f);
        _heightMin = 0.0f;
        _heightMax = 1;
    }


}
