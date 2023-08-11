using UnityEngine;
using System.IO;

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
                writer.Write($"\t{{\n\t\t\"segment\"  : {start.Index},\n");
                writer.Write(      $"\t\t\"decimate\" : {start.PathToNext.Decimate},\n");
                writer.Write(      $"\t\t\"smoothBy\" : {start.PathToNext.AvgPointsCount},\n");
                writer.Write($"\t\t\"points\" :[\n");
                int index = 0;
                int pointsCount = processed ? start.PathToNext.PathPointsProcessed.Count : start.PathToNext.PathPointsRaw.Count;
                foreach (var p in processed? start.PathToNext.PathPointsProcessed: start.PathToNext.PathPointsRaw)
                {
                    ++index;
                    writer.Write($"\t\t{{\"x\" :{p.x}, \"y\": {p.y}}}");
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
    private bool SavePath(string path2file, bool processed = true)
    {
        Debug.Log($"Saving path data as points list to: {path2file}");

        WayPoint start = GetFirstAtPath();

        if (start == null) return false;

        bool flag = false;
        using (StreamWriter writer = new StreamWriter(path2file))
        {
            if (!start.PathToNext) return false;
            while (true)
            {
                int pointsCount = processed ? start.PathToNext.PathPointsProcessed.Count : start.PathToNext.PathPointsRaw.Count;
                foreach (var p in processed ? start.PathToNext.PathPointsProcessed : start.PathToNext.PathPointsRaw)
                {
                    writer.Write($"{p.x} {p.y}\n");
                }
                flag |= true;
                if (!start.HasNext) break;
                start = start.Next;
                if (!start.PathToNext) break;
            }
        }
        return flag;
    }

    public  bool SaveRawPathJson(string path2file) => SavePathAsJson(path2file, false);
    public  bool SaveProcessedPathJson(string path2file) => SavePathAsJson(path2file, true);
    public bool SaveRawPath(string path2file) => SavePath(path2file, false);
    public bool SaveProcessedPath(string path2file) => SavePath(path2file, true);
}
