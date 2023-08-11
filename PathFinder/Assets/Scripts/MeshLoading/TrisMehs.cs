using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System.Globalization;
using System.Collections;

public struct Face 
{
    private int _p1 ;
    private int _uv1;
    private int _n1 ;
    private int _p2 ;
    private int _uv2;
    private int _n2 ;
    private int _p3 ;
    private int _uv3;
    private int _n3 ;
    public int p_1  => _p1;
    public int uv1 => _uv1;
    public int n_1 => _n1 ;
    public int p_2 => _p2 ;
    public int uv2 => _uv2;
    public int n_2 => _n2 ;
    public int p_3 => _p3 ;
    public int uv3 => _uv3;
    public int n_3 => _n3 ;
    public override string ToString() 
    {
        return $"\t{{\n \t\"p_1\": {p_1}, \"uv1\": {uv1}, \"n_1\": {n_1},\n" +
                    $"\t\t\"p_2\": {p_2}, \"uv2\": {uv2}, \"n_2\": {n_2},\n" +
                    $"\t\t\"p_3\": {p_3}, \"uv3\": {uv3}, \"n_3\": {n_3}\n\t}}";
    }
    public Vector3Int pt_1 => new Vector3Int(p_1, n_1, uv1);
    public Vector3Int pt_2 => new Vector3Int(p_2, n_2, uv2);
    public Vector3Int pt_3 => new Vector3Int(p_3, n_3, uv3);
    public IEnumerable<Vector3Int> points 
    {
        get 
        {
            yield return pt_1;
            yield return pt_2;
            yield return pt_3;
        }
    }
    public Face(int p1, int uv1, int n1, int p2, int uv2, int n2, int p3, int uv3, int n3)
    {
        _p1  = Mathf.Max(p1, -1);
        _uv1 = Mathf.Max(uv1, -1);
        _n1  = Mathf.Max(n1,  -1);
        _p2  = Mathf.Max(p2,  -1);
        _uv2 = Mathf.Max(uv2, -1);
        _n2  = Mathf.Max(n2,  -1);
        _p3  = Mathf.Max(p3,  -1);
        _uv3 = Mathf.Max(uv3, -1);
        _n3  = Mathf.Max(n3, - 1);
    }
}

public class ObjMesh
{
    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    private static bool IntersectLines(ref Vector2 intersection, Vector2 pt1, Vector2 pt2, Vector2 pt3, Vector2 pt4)
    {
        Vector2 da = pt2 - pt1;
        Vector2 db = pt4 - pt3;
        float det = Cross(da, db);
        if (Mathf.Abs(det) < 1e-6) return false;
        det = 1.0f / det;
        float x = Cross(pt1, da);
        float y = Cross(pt3, db);
        intersection = new Vector2((y * da.x - x * db.x) * det, (y * da.y - x * db.y) * det);
        return true;
    }
    public static List<ObjMesh> ReadObjMesh(string path)
    {
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";
        List<ObjMesh> meshes = new List<ObjMesh>();
        ObjMesh currentMesh = null;
        int uv_shift = 0;
        int v__shift = 0;
        int n__shift = 0;
        string[] tmp ;
        int id_;

        using (StreamReader file = new StreamReader(path))
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0) continue;
                tmp = line.Split(" ");
                if ((id_ = tmp.Length - 1) == -1) continue;
                if (tmp[0] == "#")
                {
                    if (id_ == 0) continue;
                    if (tmp[1] != "object") continue;
                    currentMesh = new ObjMesh();
                    currentMesh._source = path;
                    currentMesh._name = tmp[2];
                    meshes.Add(currentMesh);
                    if (meshes.Count == 1) continue;
                    uv_shift += meshes[meshes.Count - 2].UvsCount;
                    v__shift += meshes[meshes.Count - 2].VerticesCount;
                    n__shift += meshes[meshes.Count - 2].NormalsCount;
                    continue;
                }
                if (tmp[0] == "o") 
                {
                    currentMesh = new ObjMesh();
                    currentMesh._source = path;
                    currentMesh._name = tmp[1];
                    meshes.Add(currentMesh);
                    if (meshes.Count == 1) continue;
                    uv_shift += meshes[meshes.Count - 2].UvsCount;
                    v__shift += meshes[meshes.Count - 2].VerticesCount;
                    n__shift += meshes[meshes.Count - 2].NormalsCount;
                    continue;
                }
                if (currentMesh == null) continue;
                if (tmp[0] == "v") 
                {
                    currentMesh.AddVertex(new Vector3(float.Parse(tmp[id_ - 2], culture), float.Parse(tmp[id_ - 1], culture), float.Parse(tmp[id_], culture)));
                    continue;
                }
                if (tmp[0] == "vn") 
                {
                    currentMesh.AddNormal(new Vector3(float.Parse(tmp[id_ - 2], culture), float.Parse(tmp[id_ - 1], culture), float.Parse(tmp[id_], culture)));
                    continue;
                }
                if (tmp[0] == "vt") 
                {
                    currentMesh.AddUv(new Vector2(float.Parse(tmp[id_ - 2], culture), float.Parse(tmp[id_ - 1], culture)));
                    continue;
                }
                if (tmp[0] == "f") 
                {
                    string[] vertex1 = tmp[1].Split("/");
                    string[] vertex2 = tmp[2].Split("/");
                    string[] vertex3 = tmp[3].Split("/");
                    currentMesh.AddFace(new Face(int.Parse(vertex1[0]) - 1 - v__shift,
                                                 int.Parse(vertex1[1]) - 1 - uv_shift,
                                                 int.Parse(vertex1[2]) - 1 - n__shift,
                                                 int.Parse(vertex2[0]) - 1 - v__shift,
                                                 int.Parse(vertex2[1]) - 1 - uv_shift,
                                                 int.Parse(vertex2[2]) - 1 - n__shift,
                                                 int.Parse(vertex3[0]) - 1 - v__shift,
                                                 int.Parse(vertex3[1]) - 1 - uv_shift,
                                                 int.Parse(vertex3[2]) - 1 - n__shift));
                }
            }
            file.Close();
        }
        return meshes;
    }

    public static ObjMesh CreatePlane(float height = 1.0f, float width = 1.0f, int rows = 10, int cols = 10, Transform transform = null)
    {
        rows = Mathf.Max(rows, 2);
        cols = Mathf.Max(cols, 2);
        int nPoints = cols * rows;
        ObjMesh mesh = new ObjMesh();
        Vector3 normal = Vector3.up;
        int row, col, p1, p2, p3, p4;
        float x, z;
        for (int index = 0; index < nPoints; index++) 
        {
            col = index % cols;
            row = index / cols;
            x = width * ((cols - 1) * 0.5f - col) / (cols - 1.0f);
            z = height * ((cols - 1) * 0.5f - row) / (cols - 1.0f);
            mesh.AddVertex(new Vector3(x, 0, z));
            mesh.AddUv(new Vector2(1.0f - col * 1.0f / (cols - 1), row * 1.0f / (cols - 1)));
            mesh.AddNormal(normal);
            if ((index + 1) % cols == 0) continue;
            if (rows - 1 == row) continue;
            p1 = index;
            p2 = index + 1;
            p3 = index + cols;
            p4 = index + cols + 1;
            mesh.AddFace(new Face(p1, p1, p1, p2, p2, p2, p3, p3, p3));
            mesh.AddFace(new Face(p3, p3, p3, p2, p2, p2, p4, p4, p4));
        }
        if (transform != null) mesh.TransformMesh(transform);
        mesh._name = "procedural_plane";
        return mesh;
    }

    public static ObjMesh CreateBox(Vector3 max, Vector3 min, Transform transform = null)
    {
        Vector3 max_b = Vector3.Max(max, min);
        Vector3 min_b = Vector3.Min(max, min);
        ObjMesh mesh = new ObjMesh();

        mesh.AddVertex(new Vector3(min_b.x, max_b.y, min_b.z));
        mesh.AddVertex(new Vector3(min_b.x, max_b.y, max_b.z));
        mesh.AddVertex(new Vector3(max_b.x, max_b.y, max_b.z));
        mesh.AddVertex(new Vector3(max_b.x, max_b.y, min_b.z));
        mesh.AddVertex(new Vector3(min_b.x, min_b.y, min_b.z));
        mesh.AddVertex(new Vector3(max_b.x, min_b.y, min_b.z));
        mesh.AddVertex(new Vector3(max_b.x, min_b.y, max_b.z));
        mesh.AddVertex(new Vector3(min_b.x, min_b.y, max_b.z));

        mesh.AddNormal (new Vector3( 0.0f,  1.0f, 0.0f));
        mesh.AddNormal(new Vector3( 0.0f, -1.0f, -0.0f));
        mesh.AddNormal(new Vector3( 0.0f,  0.0f, -1.0f));
        mesh.AddNormal(new Vector3( 1.0f,  0.0f,  0.0f));
        mesh.AddNormal(new Vector3( 0.0f, -0.0f,  1.0f));
        mesh.AddNormal(new Vector3(-1.0f,  0.0f,  0.0f));
        mesh.AddUv(new Vector2(1.0f, 0.0f));
        mesh.AddUv(new Vector2(1.0f, 1.0f));
        mesh.AddUv(new Vector2(0.0f, 1.0f));
        mesh.AddUv(new Vector2(0.0f, 0.0f));

        mesh.AddFace(new Face(0, 0, 0, 1, 1, 0, 2, 2, 0));
        mesh.AddFace(new Face(2, 2, 0, 3, 3, 0, 0, 0, 0));
        mesh.AddFace(new Face(4, 3, 1, 5, 0, 1, 6, 1, 1));
        mesh.AddFace(new Face(6, 1, 1, 7, 2, 1, 4, 3, 1));
        mesh.AddFace(new Face(0, 3, 2, 3, 0, 2, 5, 1, 2));
        mesh.AddFace(new Face(5, 1, 2, 4, 2, 2, 0, 3, 2));
        mesh.AddFace(new Face(3, 3, 3, 2, 0, 3, 6, 1, 3));
        mesh.AddFace(new Face(6, 1, 3, 5, 2, 3, 3, 3, 3));
        mesh.AddFace(new Face(2, 3, 4, 1, 0, 4, 7, 1, 4));
        mesh.AddFace(new Face(7, 1, 4, 6, 2, 4, 2, 3, 4));
        mesh.AddFace(new Face(1, 3, 5, 0, 0, 5, 4, 1, 5));
        mesh.AddFace(new Face(4, 1, 5, 7, 2, 5, 1, 3, 5));
        if (transform != null) mesh.TransformMesh(transform);
        mesh._name = "procedural_box";
        return mesh;
    }

    public interface IProjectorXZ 
    {
        float Project(float x, float z);
    }
    private struct DefaultProjector : IProjectorXZ
    {
        public float Project(float x, float z) => 0.0f;

    }
    public static ObjMesh CreatePolyStrip(List<Vector2> points, float stripWidth = 1.0f, Transform transform = null, IProjectorXZ projector = null)
    {
        int n_pts = points.Count;
        if (projector == null) 
        {
            projector = new DefaultProjector();
        }
        ObjMesh mesh;
        if (n_pts == 0) return CreatePlane(stripWidth, stripWidth, 1, 1, transform);
        if (n_pts == 1)
        {
            mesh = CreatePlane(stripWidth, stripWidth, 1, 1, transform);
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.m03 = points[0].x;
            matrix.m23 = points[0].y;
            return mesh.TransformMesh(matrix);
        };
        
        float u_length = 0.0f;
        for (int i = 0; i < points.Count - 1; i++) u_length += (points[i] - points[i + 1]).magnitude;
        mesh = new ObjMesh();
        float u_coord = 0.0f, du;
        Vector2 p0 = points[0];
        Vector2 dp20, dp10 = points[1] - points[0];
        Vector2 n_1 = Vector2.Perpendicular(dp10).normalized * stripWidth * 0.5f;
        mesh.AddVertex(new Vector3(p0.x + n_1.x, projector.Project(p0.x + n_1.x, p0.y + n_1.y), p0.y + n_1.y));
        mesh.AddVertex(new Vector3(p0.x - n_1.x, projector.Project(p0.x - n_1.x, p0.y - n_1.y), p0.y - n_1.y));
        mesh.AddUv(new Vector2(0.0f, 1.0f));
        mesh.AddUv(new Vector2(0.0f, 0.0f));
        Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f);
        mesh.AddNormal(normal);
        int f_index = 2;
        Vector2 p1, p2, intersection_1 = new Vector2(), intersection_2 = new Vector2(), n_2;
        for (int index = 0; index < points.Count - 1; index++) 
        {
            p1 = points[index];
            p2 = points[index + 1];
            dp10 = p2 - p1;
            du = dp10.magnitude;
            if (du < 1e-6f) continue;
            dp20 = p2 - p1;
            u_coord += du;
            n_2 = Vector2.Perpendicular(dp20).normalized * stripWidth * 0.5f;
            if (!IntersectLines(ref intersection_1, p0 + n_1, p1 + n_1, p1 + n_2, p2 + n_2)) 
            {
                p0 = p1;
                n_1 = n_2;
                continue;
            }

            if (!IntersectLines(ref intersection_2, p0 - n_1, p1 - n_1, p1 - n_2, p2 - n_2))
            {
                p0 = p1;
                n_1 = n_2;
                continue;
            }
            f_index += 2;
            p0 = p1;
            n_1 = n_2;
            mesh.AddVertex(new Vector3(intersection_1.x, projector.Project(intersection_1.x, intersection_1.y), intersection_1.y));
            mesh.AddVertex(new Vector3(intersection_2.x, projector.Project(intersection_2.x, intersection_2.y), intersection_2.y));
            mesh.AddUv    (new Vector2(u_coord / u_length, 1.0f));
            mesh.AddUv    (new Vector2(u_coord / u_length, 0.0f));
            mesh.AddFace  (new Face(f_index - 4, f_index - 4, 0, f_index - 3, f_index - 3, 0, f_index - 2, f_index - 2, 0));
            mesh.AddFace  (new Face(f_index - 3, f_index - 3, 0, f_index - 1, f_index - 1, 0, f_index - 2, f_index - 2, 0));
        }
        p0 = points[points.Count- 1];
        f_index += 2;
        mesh.AddVertex(new Vector3(p0.x + n_1.x, projector.Project(p0.x + n_1.x, p0.y + n_1.y), p0.y + n_1.y));
        mesh.AddVertex(new Vector3(p0.x - n_1.x, projector.Project(p0.x - n_1.x, p0.y - n_1.y), p0.y - n_1.y));
        mesh.AddUv    (new Vector2(1.0f, 1.0f));
        mesh.AddUv    (new Vector2(1.0f, 0.0f));
        mesh.AddFace  (new Face(f_index - 4, f_index - 4, 0, f_index - 3, f_index - 3, 0, f_index - 2, f_index - 2, 0));
        mesh.AddFace  (new Face(f_index - 3, f_index - 3, 0, f_index - 1, f_index - 1, 0, f_index - 2, f_index - 2, 0));
        mesh._name = "procedural_poly_strip_xz";

        return mesh;
    }

    string _name    ; // = "no name";
    string _material; // = "no material";
    string _source  ; // "no source";
    List<Vector3> _vertices; 
    List<Vector3> _normals ; 
    List<Vector2> _uvs     ; 
    List<Face>    _faces   ; 
    Bounds        _bbox    ;
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append($"\t\"Name\"       : \"{_name}\",\n");
        sb.Append($"\t\"Material\"   : \"{_material}\",\n");
        sb.Append($"\t\"Source\"     : \"{_source}\",\n");
        sb.Append($"\t\"BouningBox\" : \n");
        sb.Append("\t{\n");
        sb.Append($"\t\t\"Min\" :{{\"x\": {_bbox.max.x}, \"y\": {_bbox.max.y}, \"z\": {_bbox.max.z}}},\n");
        sb.Append($"\t\t\"Max\" :{{\"x\": {_bbox.min.x}, \"y\": {_bbox.min.y}, \"z\": {_bbox.min.z}}},\n");
        
        sb.Append("\t\"Vertices\": {\n");
        foreach(var v in Vertices)
            sb.Append($"\t\t{{\"x\": {v.x}, \"y\": {v.y}, \"z\": {v.z}}},\n");
        sb.Remove(sb.Length - 4, 3);
        sb.Append("\n\t},\n");


        sb.Append("\t\"Normals\": {\n");
        foreach (var v in Normals)
            sb.Append($"\t\t{{\"x\": {v.x}, \"y\": {v.y}, \"z\": {v.z}}},\n");
        sb.Remove(sb.Length - 4, 3);
        sb.Append("\n\t},\n");


        sb.Append("\t\"Uvs\": {\n");
        foreach (var v in Uvs)
            sb.Append($"\t\t{{\"x\": {v.x}, \"y\": {v.y}}},\n");
        sb.Remove(sb.Length - 4, 3);
        sb.Append("\n\t},\n");


        sb.Append("\t\"Faces\": {\n");
        foreach (var f in Faces)
            sb.Append($"{f},\n");
        sb.Remove(sb.Length - 4, 3);
        sb.Append("\n\t},\n");
        sb.Append("}");
        return sb.ToString();
    }
    public Bounds BouningBox      => _bbox;
    public List<Vector3> Vertices => _vertices;
    public List<Vector3> Normals  => _normals ;
    public List<Vector2> Uvs      => _uvs     ;
    public List<Face>    Faces    => _faces   ;
    public Bounds        Bbox     => _bbox    ;
    public string Name       =>_name;
    public string Material   =>_material;
    public string Source     =>_source;
    public int VerticesCount => _vertices.Count;
    public int NormalsCount  => _normals.Count;
    public int UvsCount      => _uvs.Count;
    public int FacesCount    => _faces.Count;

    private static bool InRange<T>(List<T> list, int index) => index < 0 || index >= list.Count;

    public bool SetVertex(int index, Vector3 vertex) 
    {
        if (!InRange(Vertices, index)) return false;
        _bbox.Encapsulate(vertex);
        Vertices[index] = vertex;
        return true;
    }
    public bool SetNormal(int index, Vector3 normal)
    {
        if (!InRange(Normals, index)) return false;
        Normals[index] = normal;
        return true;
    }
    public bool SetUv(int index, Vector2 uv)
    {
        if (!InRange(Uvs, index)) return false;
        Uvs[index] = uv;
        return true;
    }
    public bool SetFace(int index, Face face)
    {
        if (!InRange(Faces, index)) return false;
        Faces[index] = face;
        return true;
    }
    public ObjMesh AddVertex(Vector3 vertex)
    {
        _bbox.Encapsulate(vertex);
        Vertices.Add(vertex);
        return this;
    }

    public ObjMesh AddNormal(Vector3 normal)
    {
        Normals.Add(normal);
        return this;
    }

    public ObjMesh AddUv(Vector2 uv)
    {
        Uvs.Add(uv);
        return this;
    }

    public ObjMesh AddFace(Face face)
    {
        Faces.Add(face);
        return this;
    }

    public void Cleanup(Face face)
    {
        Faces.Clear();
        Vertices.Clear();
        Normals.Clear();
        Uvs.Clear();
    }

    public ObjMesh TransformMesh(Matrix4x4 transform) 
    {
        for (int index = 0; index < VerticesCount; index++) _vertices[index] = transform.MultiplyPoint (_vertices[index]);
        for (int index = 0; index < NormalsCount;  index++) _normals [index] = transform.MultiplyVector(_normals[index]);
        return this;
    }

    public ObjMesh TransformMesh(Transform transform) => TransformMesh(transform.localToWorldMatrix);

    public ObjMesh Merge(ObjMesh other) 
    {
        int  vOffset = VerticesCount;
        int uvOffset = UvsCount;
        int  nOffset = NormalsCount;
        foreach (var v in other.Vertices) AddVertex(v);
        foreach (var v in other.Normals)  AddNormal(v);
        foreach (var v in other.Uvs)      AddUv    (v);
        foreach (var face in other.Faces) AddFace(new Face(face.p_1 + vOffset, face.uv1 + uvOffset, face.n_1 + nOffset,
                                                           face.p_2 + vOffset, face.uv2 + uvOffset, face.n_2 + nOffset,
                                                           face.p_3 + vOffset, face.uv3 + uvOffset, face.n_3 + nOffset));

        return this;
    }
    public IEnumerable MergeEnumerable(ObjMesh other)
    {
        int vOffset = VerticesCount;
        int uvOffset = UvsCount;
        int nOffset = NormalsCount;
        foreach (var v in other.Vertices) AddVertex(v);
        yield return null;
        foreach (var v in other.Normals) AddNormal(v);
        yield return null;
        foreach (var v in other.Uvs) AddUv(v);
        yield return null;
        foreach (var face in other.Faces) AddFace(new Face(face.p_1 + vOffset, face.uv1 + uvOffset, face.n_1 + nOffset,
                                                           face.p_2 + vOffset, face.uv2 + uvOffset, face.n_2 + nOffset,
                                                           face.p_3 + vOffset, face.uv3 + uvOffset, face.n_3 + nOffset));
    }
    public Mesh ToUnityMesh() 
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices  = new Vector3[VerticesCount];
        Vector2[] uvs       = new Vector2[VerticesCount];
        Vector3[] normals   = new Vector3[VerticesCount];
        int    [] triangles = new int    [FacesCount * 3];
        int tris_id = 0, p1, p2, p3;
        try
        { 
            foreach (var f in Faces) 
            {
                p1 = f.p_1; // vert_id;
                p2 = f.p_2; // vert_id + 1; 
                p3 = f.p_3; // vert_id + 2;
                
                vertices[p1] = Vertices[p1];
                vertices[p2] = Vertices[p2];
                vertices[p3] = Vertices[p3];

                uvs     [p1] = Uvs[f.uv1];
                uvs     [p2] = Uvs[f.uv2];
                uvs     [p3] = Uvs[f.uv3];

                normals [p1] = Normals[f.n_1];
                normals [p2] = Normals[f.n_2];
                normals [p3] = Normals[f.n_3];

                triangles[tris_id    ] = p1;
                triangles[tris_id + 1] = p2;
                triangles[tris_id + 2] = p3;

                tris_id += 3;
            }
        }
        catch
        {
            Debug.Log($"Index out of bound exeption {tris_id}");
            Debug.Log($"Vertex array length {Vertices.Count}");
        }
        mesh.vertices  = vertices;
        mesh.uv        = uvs;
        mesh.normals   = normals;
        mesh.triangles = triangles;
        return mesh;
    }

    public ObjMesh() 
    {
        _name     = "no name";
        _material = "no material";
        _source   = "no source";
        _vertices = new List<Vector3>();
        _normals  = new List<Vector3>();
        _uvs      = new List<Vector2>();
        _faces    = new List<Face>   ();
        _bbox     = new Bounds();
    }
}
