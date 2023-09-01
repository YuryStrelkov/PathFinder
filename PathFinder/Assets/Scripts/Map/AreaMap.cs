using Dummiesman;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ObjMesh;

[System.Serializable]
public struct MapSettings
{
    public Vector3 lineColor;
    public Vector3 pointsColor;
    public float lineWidth;
    public int decimation;
    public int smoothing;
    public float weightMapThresshold;
    public int weightMapResolution;
    public int referenceMapResolution;
}

[System.Serializable]
public struct PathSegmentInfo
{
    public int segmentId;
    public int decimateBy;
    public int smoothBy;
    public Vector2[] points;
}

[System.Serializable]
public struct ReferenceViewInfo
{
    public int sgementId;
    public int segmentRelativePos;
    public string viewImageSrc;
    public float[] prjection;
    public float[] transform;
}

[System.Serializable]
public struct AreaMapInfo
{
    public string mapModel;
    public string mapTexture;
    public string mapWeights;
    public MapSettings settings;
    public float[] mapTransfrom;
    public Vector3 northDirection;
    public PathSegmentInfo[] pathSegments;
    public ReferenceViewInfo[] referenseViews;
}

public class AreaMap : MonoBehaviour, IProjectorXZ
{
    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
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
    [SerializeField]
    Camera _weightsRendererCamera;

    AStar                  _pathFinder;
    Texture2D              _wightsMap;
    private Vector2        _size;
    private Vector2        _origin;
    private static AreaMap _instance;
    private MapSettings    _settings;
    private float          _heightMax;
    private float          _heightMin;
    private Vector3        _northDirection;

    public Vector3 NorthDirection => _northDirection;
    public MapRenderer HeightsRenderer => _heightsRenderer;
    public MapRenderer HeatRenderer => _heatRenderer;
    public MapRenderer DefaultRenderer => _defaultRenderer;

    public static AreaMap Instance => _instance;
    private bool InitAStar()
    {
        if (_wightsMap != null) 
        {
            _pathFinder = new AStar(_wightsMap);
            _pathFinder.Size = _size;
            _pathFinder.Orig = _origin; 
            return true;
        }
        return false;
    }
    // Start is called before the first frame update
    public bool EvaluateWeight(Vector3 position, out float height)
    {
        height = 0.0f;
        if (_pathFinder == null) return false;
        height = _pathFinder.GetWeightNormalized(new Vector2(position.x, position.z));
        return height < _settings.weightMapThresshold;
    }
    public bool EvaluateNormal(Vector3 position, out Vector3 normal, float delta = 1e-3f)
    {
        normal = Vector3.up;
        if (_pathFinder == null) return false;
        Vector3 p_x_1 = position - Vector3.right * 0.5f * delta;
        p_x_1.y = _pathFinder.GetWeight(p_x_1);

        Vector3 p_x_2 = position + Vector3.right * 0.5f * delta;
        p_x_2.y = _pathFinder.GetWeight(p_x_2);

        Vector3 p_z_1 = position - Vector3.forward * 0.5f * delta;
        p_z_1.y = _pathFinder.GetWeight(p_z_1);

        Vector3 p_z_2 = position + Vector3.forward * 0.5f * delta;
        p_z_2.y = _pathFinder.GetWeight(p_z_2);

        normal = Vector3.Cross((p_z_2 - p_z_1).normalized, (p_x_2 - p_x_1).normalized).normalized;

        return true;
    }

    public float Project(float x, float z)
    {
        if (_pathFinder == null) return 0.0f;
        return _pathFinder.GetWeightNormalized(new Vector2(x, z)) * (_heightMax - _heightMin) * 0.5f + _heightMin + 0.001f;
    }

    public void SwitchProjection()
    {
        _heightsRenderer.gameObject.SetActive(!_heightsRenderer.gameObject.activeInHierarchy);
        _defaultRenderer.gameObject.SetActive(!_defaultRenderer.gameObject.activeInHierarchy);
    }
    public List<Vector2> BuildPath(Vector2 from, Vector2 to)
    {
        if (_pathFinder == null) return new List<Vector2>(new Vector2[] { from, to });

        return _pathFinder.Search(from, to);
    }
    private bool LoadWeights(string mapDirectory, AreaMapInfo settings)
    { 
        string mapWeightsPath = mapDirectory.EndsWith("\\") ? mapDirectory + settings.mapWeights : mapDirectory + "\\" + settings.mapWeights;
        _weightsRendererCamera.orthographicSize = 0.5f * Mathf.Max(_size.x, _size.y) * _size.y / _size.x;
        WeightMapRenderer.Instance.Aspect = _size.x / _size.y;
        WeightMapRenderer.Instance.Resoultion = (ReferenceViewRenderer.ReferenceViewTextureResoultion)settings.settings.weightMapResolution;
        // if (File.Exists(mapWeightsPath))
        // {
            _wightsMap = LoadPNG(mapWeightsPath);
            if(InitAStar())return true;
        // }
        WeightMapRenderer.Instance.RenderReferenceView();
        _wightsMap = WeightMapRenderer.Instance.ToUnityTexture();
        return InitAStar();
    }
    private bool LoadMapTexture(string mapDirectory, AreaMapInfo settings)
    {
        string mapWeightsPath = mapDirectory.EndsWith("\\") ? mapDirectory + settings.mapTexture : mapDirectory + "\\" + settings.mapTexture;
        // if (File.Exists(mapWeightsPath))
        // {
            _defaultRenderer.MainTexture = LoadPNG(mapWeightsPath);
            return true;
        // }
        // return false;
    }
    private bool LoadGeometry(string mapDirectory, AreaMapInfo settings) 
    {
        string mapModelPath = mapDirectory.EndsWith("\\") ? mapDirectory + settings.mapModel : mapDirectory + "\\" + settings.mapModel;
        // try
        // {
            GameObject newMesh = new OBJLoader().Load(mapModelPath);
            MeshFilter mesh = newMesh.GetComponentInChildren<MeshFilter>();
            // if (mesh == null) return false;
            _heightsRenderer.SetMesh(mesh.mesh);
            _heatRenderer.   SetMesh(mesh.mesh);
            _defaultRenderer.SetMesh(mesh.mesh);
            _northDirection = settings.mapTransfrom !=null? new Vector3(settings.mapTransfrom[0], settings.mapTransfrom[1], settings.mapTransfrom[2]).normalized:Vector3.right;
            // Quaternion rotation;
            // Vector3 scale;
            // if (settings.mapTransfrom != null)
            // {
            //     Matrix4x4 mapTransform = settings.mapTransfrom.Length != 16 ? Matrix4x4.identity :
            //         new Matrix4x4(new Vector4(settings.mapTransfrom[0],  settings.mapTransfrom[1],  settings.mapTransfrom[2],  settings.mapTransfrom[3]),
            //                       new Vector4(settings.mapTransfrom[4],  settings.mapTransfrom[5],  settings.mapTransfrom[6],  settings.mapTransfrom[7]),
            //                       new Vector4(settings.mapTransfrom[8],  settings.mapTransfrom[9],  settings.mapTransfrom[10], settings.mapTransfrom[11]),
            //                       new Vector4(settings.mapTransfrom[12], settings.mapTransfrom[13], settings.mapTransfrom[14], settings.mapTransfrom[15])); //.transpose;
            //     // rotation = Quaternion.Inverse(mapTransform.rotation);
            //     rotation = Quaternion.identity;
            //     scale = new Vector3(mapTransform.m00 * mapTransform.m00 +
            //                         mapTransform.m10 * mapTransform.m10 +
            //                         mapTransform.m20 * mapTransform.m20,
            //                         
            //                         mapTransform.m01 * mapTransform.m01 +
            //                         mapTransform.m11 * mapTransform.m11 +
            //                         mapTransform.m21 * mapTransform.m21,
            // 
            //                         mapTransform.m02 * mapTransform.m02 +
            //                         mapTransform.m12 * mapTransform.m12 +
            //                         mapTransform.m22 * mapTransform.m22);
            // 
            //     // float qw = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f + mapTransform.m00 / scale.x + mapTransform.m11 / scale.y + mapTransform.m22 / scale.z)) * 0.5f;
            //     // float qx = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f + mapTransform.m00 / scale.x - mapTransform.m11 / scale.y - mapTransform.m22 / scale.z)) * 0.5f;
            //     // float qy = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f - mapTransform.m00 / scale.x + mapTransform.m11 / scale.y - mapTransform.m22 / scale.z)) * 0.5f;
            //     // float qz = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f - mapTransform.m00 / scale.x - mapTransform.m11 / scale.y + mapTransform.m22 / scale.z)) * 0.5f;
            //     // 
            //     // qx = Mathf.copysign(qx, mapTransform.m21 / scale.y - mapTransform.m12 / scale.z);
            //     // qy = Mathf.copysign(qy, mapTransform.m02 / scale.z - mapTransform.m20 / scale.x);
            //     // qz = Mathf.copysign(qz, mapTransform.m10 / scale.x - mapTransform.m01 / scale.y);
            // }
            // else 
            // {
            //     rotation = Quaternion.identity;
            //     scale = Vector3.one;
            // }
            // newMesh.SetActive(false);
            Destroy(newMesh);
            Vector3 meshSize   = mesh.sharedMesh.bounds.size;
            Vector3 meshCenter = mesh.sharedMesh.bounds.center;
            Vector3 shift = new Vector3(meshCenter.x, meshCenter.y - meshSize.y * 0.5f, meshCenter.z);
            _heightsRenderer.transform.position = -shift;
            _heatRenderer.   transform.position = -shift;
            _defaultRenderer.transform.position = -shift;

            // _heightsRenderer.transform.rotation = rotation;
            // _heatRenderer.   transform.rotation = rotation;
            // _defaultRenderer.transform.rotation = rotation;

            // _heightsRenderer.transform.localScale = new Vector3(scale.x * _heightsRenderer.transform.localScale.x,
            //                                                     scale.y * _heightsRenderer.transform.localScale.y,
            //                                                     scale.z * _heightsRenderer.transform.localScale.z);
            // 
            // _heatRenderer.   transform.localScale = new Vector3(scale.x * _heatRenderer.transform.localScale.x,
            //                                                     scale.y * _heatRenderer.transform.localScale.y,
            //                                                     scale.z * _heatRenderer.transform.localScale.z);
            // 
            // _defaultRenderer.transform.localScale = new Vector3(scale.x * _defaultRenderer.transform.localScale.x,
            //                                                     scale.y * _defaultRenderer.transform.localScale.y,
            //                                                     scale.z * _defaultRenderer.transform.localScale.z);
            // 
            // GridController.Instance.SetupSize(new Vector2(Mathf.Ceil(scale.x * meshSize.x), Mathf.Ceil(scale.z * meshSize.z)));
            // 
            // Vector3 cameraSize = new Vector3(scale.x * meshSize.x, Mathf.Max(scale.x * meshSize.x, scale.z * meshSize.z), scale.z * meshSize.z);
            // CamController.Instance.SetupMovmentLimits(-cameraSize * 0.5f + _gridController.transform.position,
            //                                            cameraSize * 0.5f + _gridController.transform.position);
            // CamController.Instance.SetOrthoLimits(0.1f, Mathf.Max(meshSize.x * scale.x , scale.z * meshSize.z));
            // 
            // _origin = new Vector2();
            // _size  = new Vector2(scale.x * meshSize.x, scale.z * meshSize.z);
            // _heightMin = 0.0f;       
            // _heightMax = scale.y * meshSize.y;

            GridController.Instance.SetupSize(new Vector2(Mathf.Ceil(meshSize.x), Mathf.Ceil(meshSize.z)));

            Vector3 cameraSize = new Vector3(meshSize.x, Mathf.Max(meshSize.x, meshSize.z), meshSize.z);
            CamController.Instance.SetupMovmentLimits(-cameraSize * 0.5f + _gridController.transform.position,
                                                       cameraSize * 0.5f + _gridController.transform.position);
            CamController.Instance.SetOrthoLimits(0.1f, Mathf.Max(meshSize.x, meshSize.z));

            _origin = new Vector2();
            _size = new Vector2(meshSize.x, meshSize.z);
            _heightMin = 0.0f;
            _heightMax = meshSize.y;

        // }
        // catch (System.Exception ex)
        // {
        //     return false;
        // }
        return true;
    }
    private bool LoadMapSettings(string mapDirectory, ref AreaMapInfo settings)
    {
        // if (!Directory.Exists(mapDirectory)) return false;
        string mapSettingsPath = mapDirectory.EndsWith("\\") ? mapDirectory + "map_settings.json" : mapDirectory + "\\map_settings.json";
        // if (!File.Exists(mapSettingsPath)) return false;
        // try
        // {
            settings = JsonUtility.FromJson<AreaMapInfo>(File.ReadAllText(mapSettingsPath));
            _settings = settings.settings;
        // }
        // catch (System.Exception ex)
        // {
        //     return false;
        // }
        // if (settings.mapModel == "") return false;
        return true;
    }
    private void LoadMapPathSegments(string mapDirectory, AreaMapInfo settings) 
    {
        if (settings.pathSegments.Length < 2) return;
        
        int currPointsCount;
        Vector2[] currPoints;

        GameObject point1 = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/WayPoint/WayPointPrefab"));
        for (int index = 0; index < settings.pathSegments.Length; index++) 
        {
            GameObject point2 = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/WayPoint/WayPointPrefab"));
            currPoints        = settings.pathSegments[index].points;
            currPointsCount   = currPoints.Length;
            point1.transform.position = new Vector3(currPoints[0].x, 0.0f, currPoints[0].y);
            point2.transform.position = new Vector3(currPoints[currPointsCount - 1].x, 0.0f, currPoints[currPointsCount - 1].y);
            WayPoint wp1 = point1.GetComponent<WayPoint>();
            wp1.transform.position = wp1.ProjectedPosition; 
            wp1.LinkToNext(new List<Vector2>(currPoints));
            point1 = point2;
        }
        WayPoint wp = point1.GetComponent<WayPoint>();
        wp.transform.position = wp.ProjectedPosition;
    }
    public bool LoadMap(string mapDirectory)
    {
        using (StreamWriter writer = new StreamWriter("C:\\Windows\\TEMP\\PAPTH_FINDER.log"))
        {
            writer.Write("================================\n");
            PointsContainer.Instance.ClearContainer();
            AreaMapInfo settings = new AreaMapInfo();
            try
            {
                LoadMapSettings(mapDirectory, ref settings);
                writer.Write("settings loads successfully\n");
            }
            catch (System.Exception ex)
            {
                writer.Write($"{ex}\n");
            }
            try
            {
                LoadGeometry(mapDirectory, settings);
                writer.Write("geometry loads successfully\n");
            }
            catch (System.Exception ex)
            {
                writer.Write($"{ex}\n");
            }
            try
            {
                LoadWeights(mapDirectory, settings);
                writer.Write("weights loads successfully\n");
            }
            catch (System.Exception ex)
            {
                writer.Write($"{ex}\n");
            }

            try
            {
                LoadMapTexture(mapDirectory, settings);
                writer.Write("textures loads successfully\n");
            }
            catch (System.Exception ex)
            {
                writer.Write($"{ex}\n");
            }
            
            try
            {
                LoadMapPathSegments(mapDirectory, settings);
                writer.Write("path segments loads successfully\n");
            }
            catch (System.Exception ex)
            {
                writer.Write($"{ex}\n");
            }

            try
            {
                PointsContainer.Instance.ApplySettings(settings.settings);
                writer.Write("settings applied segments loads successfully\n");
            }
            catch (System.Exception ex)
            {
                writer.Write($"{ex}\n");
            }

            // if (!LoadMapSettings(mapDirectory, ref settings)) return false;
            // if (!LoadGeometry(mapDirectory, settings)) return false;
            // if (!LoadWeights(mapDirectory, settings)) return false;
            // LoadMapTexture(mapDirectory, settings);
            // LoadMapPathSegments(mapDirectory, settings);
            // PointsContainer.Instance.ApplySettings(settings.settings);
        }
        return true;
    }

    public void SaveMap(string mapDirectory) 
    {
        if (!Directory.Exists(mapDirectory)) Directory.CreateDirectory(mapDirectory);
        bool endsWith          = mapDirectory.EndsWith('\\');
        string dataFile        = endsWith ? $"{mapDirectory}map_settings.json" : $"{mapDirectory}\\map_settings.json";
        string pathViewsImages = endsWith ? $"{mapDirectory}viewsImages\\"     : $"{mapDirectory}\\viewsImages\\";
        if (!Directory.Exists(pathViewsImages)) Directory.CreateDirectory(pathViewsImages);
        using (StreamWriter writer = new StreamWriter(dataFile))
        {
            Vector3 s = _defaultRenderer.MeshSize;
            Vector3 c = _defaultRenderer.MeshCenter;
            System.IO.File.WriteAllBytes($"{mapDirectory}\\mapWeights.png", _wightsMap.EncodeToPNG());
            writer.Write("{\n");
            writer.Write($"\t\"mapModel\":                 \"{"Model/map.obj"}\",\n");
            writer.Write($"\t\"mapTexture\":               \"{"Model/mapTexture.png"}\",\n");
            writer.Write($"\t\"mapWeights\":               \"{"mapWeights.png"}\",\n");
            writer.Write($"\t\"settings\":\n");            
            writer.Write("\t{\n");                         
            writer.Write($"\t\t\"lineColor\":              {{ \"x\": {_settings.lineColor.x.ToString().Replace(',', '.')}," +
                                                           $" \"y\": {_settings.lineColor.y.ToString().Replace(',', '.')}," +
                                                           $" \"z\": {_settings.lineColor.z.ToString().Replace(',', '.')} }},\n");
            writer.Write($"\t\t\"pointsColor\":            {{ \"x\": {_settings.pointsColor.x.ToString().Replace(',', '.')}, " +
                                                           $"\"y\": {_settings.pointsColor.y.ToString().Replace(',', '.')}, " +
                                                           $"\"z\": {_settings.pointsColor.z.ToString().Replace(',', '.')} }},\n");
            writer.Write($"\t\t\"lineWidth\":              {_settings.lineWidth.ToString().Replace(',', '.')},\n");
            writer.Write($"\t\t\"smoothing\":              {_settings.smoothing},\n");
            writer.Write($"\t\t\"decimation\":             {_settings.decimation},\n");
            writer.Write($"\t\t\"weightMapThresshold\":    {_settings.weightMapThresshold.ToString().Replace(',', '.')},\n");
            writer.Write($"\t\t\"weightMapResolution\":    {_settings.weightMapResolution},\n");
            writer.Write($"\t\t\"referenceMapResolution\": {_settings.referenceMapResolution}\n");

            writer.Write("\t},\n");

            if (!PointsContainer.Instance.ContainsAnyData)
            {
                writer.Write($"\t\"pathSegments\"  :[],\n");
                writer.Write($"\t\"referenseViews\":[]\n");
                writer.Write("}\n");
            }
            else
            {
                writer.Write($"\t\"pathSegments\"  :[\n{PointsContainer.Instance.PathsAsJsonString(false)}],\n");
                writer.Write($"\t\"referenseViews\":[\n{PointsContainer.Instance.ViewsAsJsonString(mapDirectory)}]\n");
                writer.Write("}\n");
            }
        }
    }

    void Awake()
    {
        _instance = this;
        _size = new Vector3(10.0f, 10.0f);
        _heightMin = 0.0f;
        _heightMax = 1.0f;
        _settings = new MapSettings()
        {
            weightMapThresshold = 0.0f,
            decimation = 0,
            referenceMapResolution = 512,
            weightMapResolution = 512,
            lineColor = Vector3.forward,
            pointsColor = Vector3.right,
            lineWidth = 0.1f,
            smoothing = 16
        };
    }
    private void Start()
    {
        _heightsRenderer.gameObject.SetActive(false);
        _heatRenderer.   gameObject.SetActive(false);
        _defaultRenderer.gameObject.SetActive(true);
        InitAStar();
    }
    public void OnDrawGizmos()
    {
        if (_pathFinder == null) return;
        int row, col;
        int sizePoints = 128;
        float du = _size.x / (sizePoints);
        float dv = _size.y / (sizePoints);
        for (int index = 0; index < sizePoints * sizePoints; index++)
        {
            row = index / sizePoints;
            col = index % sizePoints;
            Vector2 uv = new Vector2(du * (col + 0.5f) - _size.x * 0.5f, dv * (row + 0.5f) - _size.y * 0.5f);
            float w = _pathFinder.GetWeightNormalized(uv);
            Gizmos.color = new Color(w, w, w);
            Vector3 size   = new Vector3(du, w * 10.0f, dv);
            Vector3 center = new Vector3(uv.x, w * 0.5f * 10.0f, uv.y);
            Gizmos.DrawCube(center, size);
        }
    }
}
