    using UnityEngine;
using System.IO;
using System.Text;


public class PointsContainer : MonoBehaviour
{
    private static PointsContainer _instance;
    public static PointsContainer Instance => _instance;
    void Awake() => _instance = this;
    
    public static readonly string SaveDataPath = "E:\\GitHub\\path.json";
    public bool ContainsAnyData => transform.childCount != 0;
    private WayPoint GetFirstAtPath() 
    {
        WayPoint start = null;
        foreach (Transform t in transform)
        {
            var wp = t.gameObject.GetComponent<WayPoint>();
            if (!wp) continue;
            if (wp.HasPrev) continue;
            start = wp;
            break;
        }
        return start;
    }
    public void ApplySettings(MapSettings settngs)
    {
        foreach (Transform t in transform)
        {
            var wp = t.gameObject.GetComponent<WayPoint>();
            if (!wp) continue;
            if (!wp.PathToNext) continue;
            wp.PathToNext.ApplySettings(settngs, AreaMap.Instance);
        }
        Material pointMat = Resources.Load<Material>("Prefabs/WayPoint/WayPointMaterial");
        if (pointMat) pointMat.color = new Color(settngs.pointsColor.x, settngs.pointsColor.y, settngs.pointsColor.z);
        Material lineMat  = Resources.Load<Material>("Prefabs/LineSegment/WayPointMaterial");
        if (lineMat) lineMat.color = new Color(settngs.lineColor.x, settngs.lineColor.y, settngs.lineColor.z);
    }
    public void ClearContainer() 
    {
        foreach (Transform t in transform) GameObject.Destroy(t.gameObject);
    }

    public string PathsAsJsonString(bool processed = true) 
    {
        WayPoint start = GetFirstAtPath();

        if (start == null) return "";
        
        StringBuilder builder = new StringBuilder();

        if (!start.PathToNext) return "";

        while (true)
        {
            builder.Append($"\t{{\n" +
                           $"\t\t\"segmentId\"  : {start.Index},\n");
            builder.Append($"\t\t\"decimateBy\" : {start.PathToNext.Decimate},\n");
            builder.Append($"\t\t\"smoothBy\"   : {start.PathToNext.AvgPointsCount},\n");
            builder.Append($"\t\t\"points\"     : [\n");
            int index = 0;
            int pointsCount = processed ? start.PathToNext.PathPointsProcessed.Count : start.PathToNext.PathPointsRaw.Count;

            foreach (var p in processed ? start.PathToNext.PathPointsProcessed : start.PathToNext.PathPointsRaw)
            {
                ++index;
                builder.Append($"\t\t{{\"x\" : {p.x.ToString().Replace(",", ".")}, \"y\": {p.y.ToString().Replace(",", ".")}}}");
                if (index != pointsCount) builder.Append(",\n");
            }
            builder.Append("\n\t\t]\n");
            builder.Append("\t}");
            if (!start.HasNext) break;
            start = start.Next;
            if (!start.PathToNext) break;
            builder.Append(",\n");
        }
        return builder.ToString();
    }
    private static Matrix4x4 BuildTransformRelativeView(WayPoint wp) 
    {
        Vector3 forward = wp.Next != null ? (wp.Next.transform.position - wp.transform.position).normalized:
            (wp.transform.position - wp.Prev.transform.position).normalized;
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        Vector3 up    = Vector3.Cross(right,   forward).normalized;
        Matrix4x4 transform = Matrix4x4.identity;
        transform.m00 = right.x;
        transform.m10 = right.y;
        transform.m20 = right.z;

        transform.m01 = up.x;
        transform.m11 = up.y;
        transform.m21 = up.z;

        transform.m02 = forward.x;
        transform.m12 = forward.y;
        transform.m22 = forward.z;

        transform.m03 = wp.transform.position.x;
        transform.m13 = wp.transform.position.y;
        transform.m23 = wp.transform.position.z;
        return transform;
    }

    public string ViewsAsJsonString(string viewsDir = "")
    {
        WayPoint start = GetFirstAtPath();

        if (start == null) return "";

        StringBuilder builder = new StringBuilder();

        if (!start.PathToNext) return "";

        Vector3 startPointPosition = start.transform.position;
        
        Vector3 startPointDirection = start.Next != null ? (start.Next.transform.position - start.transform.position).normalized : Vector3.forward;

        Matrix4x4 transfrom = BuildTransformRelativeView(start);

        EnvioRenderer.Instance.Render(startPointPosition, startPointDirection, startPointPosition, transfrom);

        if (!Directory.Exists(viewsDir)) Directory.CreateDirectory(viewsDir);

        if (!viewsDir.EndsWith("\\")) viewsDir += "\\";

        EnvioRenderer.Instance.SaveEnvioRenders($"{viewsDir}startPointEnvio\\");

        while (true)
        {
            Matrix4x4 p = start.PathToNext.ReferenceViewAtBegin.ProjectionMatrix;
            Matrix4x4 t = start.PathToNext.ReferenceViewAtBegin.TransformMatrix;
            builder.Append($"\t{{\n" +
                           $"\t\t\"segmentId\"          : {start.Index},\n");
            builder.Append($"\t\t\"segmentRelativePos\" : 0,\n");
            builder.Append($"\t\t\"viewImageSrc\"       : \"viewsImages\\startView{start.Index}.png\",\n".Replace('\\', '/'));
            builder.Append($"\t\t\"projection\"         : [{p.m00.ToString().Replace(",", ".")}, {p.m01.ToString().Replace(",", ".")}, {p.m02.ToString().Replace(",", ".")}, {p.m03.ToString().Replace(",", ".")}, " +
                                               $"{p.m10.ToString().Replace(",", ".")}, {p.m11.ToString().Replace(",", ".")}, {p.m12.ToString().Replace(",", ".")}, {p.m13.ToString().Replace(",", ".")}, " +
                                               $"{p.m20.ToString().Replace(",", ".")}, {p.m21.ToString().Replace(",", ".")}, {p.m22.ToString().Replace(",", ".")}, {p.m23.ToString().Replace(",", ".")}, " +
                                               $"{p.m30.ToString().Replace(",", ".")}, {p.m31.ToString().Replace(",", ".")}, {p.m32.ToString().Replace(",", ".")}, {p.m33.ToString().Replace(",", ".")}],\n");
            builder.Append($"\t\t\"transform\"          : [{t.m00.ToString().Replace(",", ".")}, {t.m01.ToString().Replace(",", ".")}, {t.m02.ToString().Replace(",", ".")}, {t.m03.ToString().Replace(",", ".")}, " +
                                               $"{t.m10.ToString().Replace(",", ".")}, {t.m11.ToString().Replace(",", ".")}, {t.m12.ToString().Replace(",", ".")}, {t.m13.ToString().Replace(",", ".")}, " +
                                               $"{t.m20.ToString().Replace(",", ".")}, {t.m21.ToString().Replace(",", ".")}, {t.m22.ToString().Replace(",", ".")}, {t.m23.ToString().Replace(",", ".")}, " +
                                               $"{t.m30.ToString().Replace(",", ".")}, {t.m31.ToString().Replace(",", ".")}, {t.m32.ToString().Replace(",", ".")}, {t.m33.ToString().Replace(",", ".")}]\n");
            builder.Append($"\t}},\n");

            p = start.PathToNext.ReferenceViewAtEnd.ProjectionMatrix;
            t = start.PathToNext.ReferenceViewAtEnd.TransformMatrix;
            builder.Append($"\t{{\n" +
                           $"\t\t\"segmentId\"          : {start.Index},\n");
            builder.Append($"\t\t\"segmentRelativePos\" : 1,\n");
            builder.Append($"\t\t\"viewImageSrc\"       : \"viewsImages\\endView{start.Index}.png\",\n".Replace('\\', '/'));
            builder.Append($"\t\t\"projection\"         : [{p.m00.ToString().Replace(",", ".")}, {p.m01.ToString().Replace(",", ".")}, {p.m02.ToString().Replace(",", ".")}, {p.m03.ToString().Replace(",", ".")}, " +
                                               $"{p.m10.ToString().Replace(",", ".")}, {p.m11.ToString().Replace(",", ".")}, {p.m12.ToString().Replace(",", ".")}, {p.m13.ToString().Replace(",", ".")}, " +
                                               $"{p.m20.ToString().Replace(",", ".")}, {p.m21.ToString().Replace(",", ".")}, {p.m22.ToString().Replace(",", ".")}, {p.m23.ToString().Replace(",", ".")}, " +
                                               $"{p.m30.ToString().Replace(",", ".")}, {p.m31.ToString().Replace(",", ".")}, {p.m32.ToString().Replace(",", ".")}, {p.m33.ToString().Replace(",", ".")}],\n");
            builder.Append($"\t\t\"transform\"          : [{t.m00.ToString().Replace(",", ".")}, {t.m01.ToString().Replace(",", ".")}, {t.m02.ToString().Replace(",", ".")}, {t.m03.ToString().Replace(",", ".")}, " +
                                               $"{t.m10.ToString().Replace(",", ".")}, {t.m11.ToString().Replace(",", ".")}, {t.m12.ToString().Replace(",", ".")}, {t.m13.ToString().Replace(",", ".")}, " +
                                               $"{t.m20.ToString().Replace(",", ".")}, {t.m21.ToString().Replace(",", ".")}, {t.m22.ToString().Replace(",", ".")}, {t.m23.ToString().Replace(",", ".")}, " +
                                               $"{t.m30.ToString().Replace(",", ".")}, {t.m31.ToString().Replace(",", ".")}, {t.m32.ToString().Replace(",", ".")}, {t.m33.ToString().Replace(",", ".")}]\n");
            builder.Append($"\t}}");

            start.PathToNext.ReferenceViewAtBegin.SaveViewTexture($"{viewsDir}viewsImages\\startView{start.Index}.png".Replace('\\', '/'));
            start.PathToNext.ReferenceViewAtEnd.  SaveViewTexture($"{viewsDir}viewsImages\\endView{start.Index}.png".Replace('\\', '/'));

            if (!start.HasNext) break;
            start = start.Next;
            if (!start.PathToNext) break;
            builder.Append(",\n");
        }
        startPointDirection = start.Prev != null ? (start.transform.position - start.Prev.transform.position).normalized: Vector3.forward;

        EnvioRenderer.Instance.Render(start.transform.position, startPointDirection, startPointPosition, transfrom);

        EnvioRenderer.Instance.SaveEnvioRenders($"{viewsDir}endPointEnvio\\");

        return builder.ToString();
    }

    private bool SavePathAsJson(string path2file, bool processed = true)
    {
        Debug.Log($"Saving path data as JSON to: {path2file}");

        WayPoint start = GetFirstAtPath();

        if (start == null) return false;

        bool flag = false;
        using (StreamWriter writer = new StreamWriter(path2file))
        {
            writer.Write("{\n\t\"pathSegments\":[\n");
            if (!start.PathToNext) return false;
            while (true)
            {
                writer.Write($"\t{{\n\t\t\"segmentId\"  : {start.Index},\n");
                writer.Write(      $"\t\t\"decimateBy\" : {start.PathToNext.Decimate},\n");
                writer.Write(      $"\t\t\"smoothBy\"   : {start.PathToNext.AvgPointsCount},\n");
                writer.Write($"\t\t\"points\" :[\n");
                int index = 0;
                int pointsCount = processed ? start.PathToNext.PathPointsProcessed.Count : start.PathToNext.PathPointsRaw.Count;

                foreach (var p in processed? start.PathToNext.PathPointsProcessed: start.PathToNext.PathPointsRaw)
                {
                    ++index;
                    writer.Write($"\t\t{{\"x\" : {p.x.ToString().Replace(",", ".")}, \"y\": {p.y.ToString().Replace(",", ".")}}}");
                    if (index != pointsCount) writer.Write(",\n");
                }
                writer.Write("\n\t\t]\n");
                writer.Write("\t}");
                flag |= true;
                if (!start.HasNext) break;
                start = start.Next;
                if (!start.PathToNext) break;
                writer.Write(",\n");
            }
            writer.Write("\n\t]\n}");
        }
        return flag;
    }
    private bool SavePath(string path2file, bool processed = true, bool localSpace=true)
    {
        Debug.Log($"Saving path data as points list to: {path2file}");

        WayPoint start = GetFirstAtPath();

        Matrix4x4 transfrom = localSpace ? BuildTransformRelativeView(start).inverse : Matrix4x4.identity;

        if (start == null) return false;

        bool flag = false;
        using (StreamWriter writer = new StreamWriter(path2file))
        {
            if (!start.PathToNext) return false;

            Vector3 startPos = start.transform.position, pos;

            while (true)
            {
                pos = transfrom.MultiplyPoint(start.transform.position);
                writer.Write($"{(int)(pos.x)} {(int)(pos.z)}\n".Replace(',', '.'));
                // int pointsCount = processed ? start.PathToNext.PathPointsProcessed.Count : start.PathToNext.PathPointsRaw.Count;
                // foreach (var p in processed ? start.PathToNext.PathPointsProcessed : start.PathToNext.PathPointsRaw)
                // {
                //     writer.Write($"{p.x} {p.y}\n".Replace(',', '.'));
                // }
                flag |= true;
                if (!start.HasNext) break;
                start = start.Next;
                // if (!start.PathToNext) break;
            }
        }
        return flag;
    }

    public float[] PointsDataRaw
    {
        get
        {
            WayPoint start = GetFirstAtPath();
            WayPoint current;
            current = start;
            // int arraySize = 0;
            if (!current.PathToNext) return null;
            // while (true)
            // {
            //     arraySize += /*current.PathToNext.PathPointsRaw.Count * */ 2;
            //     if (!current.HasNext) break;
            //     current = current.Next;
            //     // if (!current.PathToNext) break;
            // }
            if(transform.childCount == 0) return null;
            int cntr = 0;
            float[] array = new float[transform.childCount*2];

            Vector3 p0 = start.transform.position;
            current = start;
            while (true)
            {
                // foreach (Vector2 pt in current.PathToNext.PathPointsRaw)
                // {
                //     array[cntr] = pt.x;
                //     array[cntr + 1] = pt.y;
                //     cntr += 2;
                // }
                array[cntr] = current.transform.position.x - p0.x;
                array[cntr + 1] = current.transform.position.z - p0.z;
                cntr += 2;
                if (!current.HasNext) break;
                current = current.Next;
                // if (!current.PathToNext) break;
            }
            return array;
        }
    }

    public Vector2[] PointsRaw
    {
        get
        {
            WayPoint start = GetFirstAtPath();
            WayPoint current;
            current = start;
            // int arraySize = 0;
            if (!current.PathToNext) return null;
            // while (true)
            // {
            //     arraySize += /*current.PathToNext.PathPointsRaw.Count * */ 2;
            //     if (!current.HasNext) break;
            //     current = current.Next;
            //     // if (!current.PathToNext) break;
            // }
            if (transform.childCount == 0) return null;
            int cntr = 0;
            Vector2[] array = new Vector2[transform.childCount];

            Vector3 p0 = start.transform.position;
            current = start;
            
            Matrix4x4 transfrom = BuildTransformRelativeView(start).inverse;

            while (true)
            {
                // foreach (Vector2 pt in current.PathToNext.PathPointsRaw)
                // {
                //     array[cntr] = pt.x;
                //     array[cntr + 1] = pt.y;
                //     cntr += 2;
                // }
                // p0 = transfrom.MultiplyPoint(current.transform.position);
                // writer.Write($"{(int)(pos.x)} {(int)(pos.z)}\n".Replace(',', '.'));
                // array[cntr] = new Vector2(current.transform.position.x - p0.x, current.transform.position.z - p0.z);
                array[cntr] = transfrom.MultiplyPoint(current.transform.position); ///new Vector2(current.transform.position.x - p0.x, current.transform.position.z - p0.z);
                cntr += 1;
                if (!current.HasNext) break;
                current = current.Next;
                // if (!current.PathToNext) break;
            }
            return array;
        }
    }

    public float[] PointsDataProcessed
    {
        get
        {
            WayPoint start = GetFirstAtPath();
            WayPoint current;
            current = start;
            int arraySize = 0;
            if (!current.PathToNext) return null;
            while (true)
            {
                arraySize += current.PathToNext.PathPointsProcessed.Count * 2;
                if (!current.HasNext) break;
                current = current.Next;
                if (!current.PathToNext) break;
            }
            if (arraySize == 0) return null;
            int cntr = 0;
            float[] array = new float[arraySize];
            current = start;
            while (true)
            {
                foreach (Vector2 pt in current.PathToNext.PathPointsProcessed)
                {
                    array[cntr]     = pt.x;
                    array[cntr + 1] = pt.y;
                    cntr += 2;
                }
                if (!current.HasNext) break;
                current = current.Next;
                if (!current.PathToNext) break;
            }
            return array;
        }
    }
    public  bool SaveRawPathJson      (string path2file) => SavePathAsJson(path2file, false);
    public  bool SaveProcessedPathJson(string path2file) => SavePathAsJson(path2file, true);
    public bool SaveRawPath           (string path2file) => SavePath(path2file, false);
    public bool SaveProcessedPath     (string path2file) => SavePath(path2file, true);
    public bool SaveRawPathJson()
    {
        string paths;
        paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveRawPathJson", "", "", "json");
        if (string.IsNullOrEmpty(paths)) return false;
        return SaveRawPathJson(paths);
    }
    public bool SaveProcessedPathJson()
    {
        string paths;
        paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveProcessedPathJson", "", "", "json");
        if (string.IsNullOrEmpty(paths)) return false;
        return SaveProcessedPathJson(paths);
    }
    public bool SaveRawPath()
    {
        string paths;
        paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveRawPath", "", "", "txt");
        if (string.IsNullOrEmpty(paths)) return false;
        return SaveRawPath(paths);
    }
    public bool SaveProcessedPath()
    {
        string paths;
        paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveProcessedPath", "", "", "txt");
        if (string.IsNullOrEmpty(paths)) return false;
        return SaveProcessedPath(paths);
    }
    public bool SaveAllPaths()
    {
        if (!ContainsAnyData) return false;
        string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Save session as...", "", false);
        if (paths.Length == 0) return false;
        if (string.IsNullOrEmpty(paths[0])) return false;
        try
        {
            SaveProcessedPathJson(paths[0] + "\\raw_path.json");
            SaveRawPathJson(paths[0] + "\\processed_path.json");
            SaveRawPath(paths[0] + "\\raw_path.txt");
            SaveProcessedPath(paths[0] + "\\processed_path.txt");
            // _saveButtonsContainer.style.display = DisplayStyle.None;
        }
        catch (System.Exception ex)
        {
            return false;
            // _saveButtonsContainer.style.display = DisplayStyle.None;
        }
        return true;
    }
}
