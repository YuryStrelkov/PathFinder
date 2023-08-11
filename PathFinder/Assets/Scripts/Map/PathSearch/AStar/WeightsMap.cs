using System.Linq;
using System.Text;
using UnityEngine;

public struct WeightsMap
{
    public static readonly float MAX_WEIGHT = 100.0f;
    public static readonly float MIN_WEIGHT = 1.0f;

    int _rows, _cols;
    float[] _wieghts;
    public WeightsMap(int rows, int cols, float[] wieghts, bool invertWeights = true)
    {
        _rows = rows;
        _cols = cols;
        _wieghts = new float[wieghts.Length];
        float wieghts_max = wieghts.Max();
        float wieghts_min = wieghts.Min();
        float ratio = 1.0f / (wieghts_max - wieghts_min) * (MAX_WEIGHT - MIN_WEIGHT);
        for (int index = 0; index < _wieghts.Length; index++)
        {
            _wieghts[index] = (wieghts[index] - wieghts_min) * ratio + MIN_WEIGHT;
        }
    }
    public WeightsMap(Texture2D texture, bool invertWeights = true)
    {
        _rows = texture.height;
        _cols = texture.width;
        _wieghts = new float[texture.height * texture.width];
        Color32[] pixels = texture.GetPixels32();

        //float devider = 1.0f / 3.0f / 255.0f;
        float devider = 1.0f / 255.0f;

        float ratio = MAX_WEIGHT - MIN_WEIGHT;

        if (invertWeights)
        {
            for (int index = 0; index < pixels.Length; index++)
                _wieghts[index] = MIN_WEIGHT + ratio * (1.0f - (0.299f * pixels[index].r + 0.5870f * pixels[index].g + 0.1140f * pixels[index].b) * devider);
            return;
        }
        for (int index = 0; index < pixels.Length; index++)
            _wieghts[index] = MIN_WEIGHT + ratio * (0.299f * pixels[index].r + 0.5870f * pixels[index].g + 0.1140f * pixels[index].b) * devider;
    }
    public int Rows { get => _rows; }
    public int Cols { get => _cols; }
    public int NCells { get => Rows * Cols; }
    public float[] Wieghts { get => _wieghts; }
    public float this[int index]
    {
        get
        {
            if (index < 0) return MAX_WEIGHT;
            if (index >= NCells) return MAX_WEIGHT;
            return _wieghts[index];
        }
        set
        {
            if (index < 0) return;
            if (index >= NCells) return;
            _wieghts[index] = Mathf.Clamp(value, MIN_WEIGHT, MAX_WEIGHT);
        }
    }
    public float this[int row, int col]
    {
        get
        {
            return this[row * Cols + col];
        }
        set
        {
            this[row * Cols + col] = value;
        }
    }
    public float this[Point pt]
    {
        get
        {
            return this[pt.row * Cols + pt.col];
        }
        set
        {
            this[pt.row * Cols + pt.col] = value;
        }
    }
    public void Invert()
    {
        for (int i = 0; i < _wieghts.Length; i++) _wieghts[i] = Mathf.Max(MAX_WEIGHT - _wieghts[i], MIN_WEIGHT);
    }
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append($"{{\n\"Rows\"   : {Rows},\n");
        builder.Append($"\"Cols\"   : {Cols},\n");
        builder.Append($"\"Wieghts\": [\n");
        for (int i = 0; i < NCells; i++)
        {
            if (i % Cols == 0)
            {
                builder.Append('\n');
            }
            builder.Append($"{Wieghts[i],5:F3}");

            builder.Append(", ");
        }
        builder.Append("]\n}}");


        return builder.ToString();
    }

    /// <summary>
    /// проверяет можно ли пройти из точки pt1 в точку pt2
    /// </summary>
    /// <param name="pt1"></param>
    /// <param name="pt2"></param>
    /// <returns></returns>
    public bool RayCast(Point pt1, Point pt2, float threshod = 100) 
    {
        int x0 = pt1.col;
        int y0 = pt1.row;
        
        int x1 = pt2.col;
        int y1 = pt2.row;

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;
        for (; ; )
        {
            if (this[y0, x0] >= threshod) return false;
            if (x0 == x1 && y0 == y1)     return true;
            e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy)  { err += dx; y0 += sy; }
        }
    }
    public bool PhisicalRayCast(Vector3 pt1, Vector3 pt2, Vector2 grid_size, Vector2 grid_pos, float threshod = 100) 
    {
        ////
        /// works bad
        ////

        Vector3 dir = (pt2 - pt1).normalized;
        Vector2Int curr_cell =  new Vector2Int((int)(((pt1.x - grid_pos.y) / grid_size.y + 0.5f) * (Rows - 1)),
                                               (int)(((pt1.z - grid_pos.x) / grid_size.x + 0.5f) * (Cols - 1)));

        Vector2Int target_cell = new Vector2Int((int)(((pt2.x - grid_pos.y) / grid_size.y + 0.5f) * (Rows - 1)),
                                                (int)(((pt2.z - grid_pos.x) / grid_size.x + 0.5f) * (Cols - 1)));

        Vector2 unit_step = new Vector2(Mathf.Sqrt(1.0f + (dir.z / dir.x) * (dir.z / dir.x)),
                                        Mathf.Sqrt(1.0f + (dir.x / dir.z) * (dir.x / dir.z)));

        Vector2Int v_step = new Vector2Int();
        Vector2 ray_length = new Vector2();

        if (dir.x < 0)
        {
            v_step.x = -1;
            ray_length.x = (pt1.x - curr_cell.x) * unit_step.x;
        }
        else 
        {
            v_step.x = 1;
            ray_length.x = ((curr_cell.x + 1) - pt1.x) * unit_step.x;
        }

        if (dir.z < 0)
        {
            v_step.y = -1;
            ray_length.y = (pt1.z - curr_cell.y) * unit_step.y;
        }
        else
        {
            v_step.y = 1;
            ray_length.y = ((curr_cell.y + 1) - pt1.z) * unit_step.y;
        }

        float max_dist = grid_size.magnitude;
        float curr_dist = 0.0f;
        while (true) 
        {
            if (curr_dist > max_dist) return false;
            if (this[curr_cell.y, curr_cell.x] >= threshod) return false;
            if (curr_cell == target_cell) return true;
            
            if (ray_length.x < ray_length.y)
            {
                curr_cell.x += v_step.x;
                ray_length.x += unit_step.x;
                curr_dist = ray_length.x;
                continue;
            }
            curr_cell.y += v_step.y;
            ray_length.y += unit_step.y;
            curr_dist = ray_length.y;
        }

    }
}

