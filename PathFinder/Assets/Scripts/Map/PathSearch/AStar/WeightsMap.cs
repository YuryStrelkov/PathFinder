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
        float ratio = (MAX_WEIGHT - MIN_WEIGHT) / (wieghts_max - wieghts_min);
        for (int index = 0; index < _wieghts.Length; index++)
        {
            _wieghts[index] = (wieghts[index] - wieghts_min) * ratio + MIN_WEIGHT;
        }
    }
    public WeightsMap(Texture2D texture, bool invertWeights = true)
    {
        _cols = texture.height;
        _rows = texture.width;
        _wieghts = new float[texture.height * texture.width];
        Color32[] pixels = texture.GetPixels32();

        float devider = 1.0f / 255.0f;

        float ratio = MAX_WEIGHT - MIN_WEIGHT;
        
        int row, col;

        if (invertWeights)
        {
            for (int index = 0; index < pixels.Length; index++)
            {
                row = texture.height - 1 - index / texture.width;
                col = index % texture.width;
                _wieghts[row + texture.height * col] = MIN_WEIGHT + ratio * (1.0f - (0.299f * pixels[index].r + 0.5870f * pixels[index].g + 0.1140f * pixels[index].b) * devider);
            }
            return;
        }
        for (int index = 0; index < pixels.Length; index++)
        {
            row = texture.height - 1 - index / texture.width;
            col = index % texture.width;
            _wieghts[row + texture.height * col] =  MIN_WEIGHT + ratio * (0.299f * pixels[index].r + 0.5870f * pixels[index].g + 0.1140f * pixels[index].b) * devider;
        }
    }
    public int Rows { get => _rows; }
    public int Cols { get => _cols; }
    public int NCells { get => Rows * Cols; }
    public float[] Wieghts { get => _wieghts; }
    public float this[int index]
    {
        get => IsIndexValid(index) ? _wieghts[index] : MAX_WEIGHT;
        set
        {
            if (!IsIndexValid(index)) return;
            _wieghts[index] = Mathf.Clamp(value, MIN_WEIGHT, MAX_WEIGHT);
        }
    }

    public bool IsPointValid(Point pt) => IsPointValid(pt.row, pt.col);
    private bool IsPointValid(int row, int col)
    {
        if (col < 0) return false;
        if (row < 0) return false;
        if (col >= Cols) return false;
        if (row >= Rows) return false;
        return true;
    }
    private bool IsIndexValid(int index) => index < 0 || index >= NCells;

    public float this[int row, int col]
    {
        get => IsPointValid(row, col) ? _wieghts[row * Cols + col] : MAX_WEIGHT;
        set
        {
            if (!IsPointValid(row, col)) return;
            _wieghts[row * Cols + col] = value;
        }
    }
    public float this[Point pt]
    {
        get => IsPointValid(pt) ? _wieghts[pt.row * Cols + pt.col] : MAX_WEIGHT;
        set
        {
            if (!IsPointValid(pt)) return;
            _wieghts[pt.row * Cols + pt.col] = value;
        }
    }
    private static int CalcIndex(int index, int index_range)
    {
        if (index < 0) return index_range - 1 - (-index) % index_range;
        return index % index_range;
    }
    public Vector3 NormalAtPt(Point pt)
    {
        if (!IsPointValid(pt)) return Vector3.zero;

        int row_1 = CalcIndex(pt.row + 1, Rows);
        int row_0 = CalcIndex(pt.row - 1, Rows);

        int col_1 = CalcIndex(pt.col + 1, Cols);
        int col_0 = CalcIndex(pt.col - 1, Cols);

        return new Vector3((_wieghts[Cols * pt.row + col_1] - _wieghts[Cols * pt.row + col_0]) * 0.5f, 1.0f,
                           (_wieghts[Cols * row_1 + pt.col] - _wieghts[Cols * row_0 + pt.col]) * 0.5f).normalized;
    }

    public float GetWeightsBilinearNormalized(Vector2 uv) => (GetWeightsBilinear(uv) - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
    public float GetWeightsBiqubicNormalized(Vector2 uv)  => (GetWeightsBicubic (uv) - MIN_WEIGHT) / (MAX_WEIGHT - MIN_WEIGHT);
    public float GetWeightsBilinear(Vector2 uv)
    {
        if (uv.x < 0.0f) return MAX_WEIGHT;
        if (uv.y < 0.0f) return MAX_WEIGHT;

        if (uv.x > 1.0f) return MAX_WEIGHT;
        if (uv.y > 1.0f) return MAX_WEIGHT;

	    int col, row, col1, row1;
        float tx, ty;

        col = (int) (uv.x * (Cols - 1));
	    row = (int) (uv.y * (Rows - 1));

	    col1 = Mathf.Min(col + 1, Cols - 1);
        row1 = Mathf.Min(row + 1, Rows - 1);

        float dx = 1.0f / (Cols - 1);
        float dy = 1.0f / (Rows - 1);

        tx = Mathf.Min(1.0f, (uv.x - col * dx) / dx);
	    ty = Mathf.Min(1.0f, (uv.y - row * dy) / dy);

        float q00, q01, q10, q11;
        /// (col + row * Сols)
        q01 = _wieghts[row  + col1 * Cols];
        q00 = _wieghts[row  + col  * Cols];
        q10 = _wieghts[row1 + col  * Cols];
        q11 = _wieghts[row1 + col1 * Cols];

        return q00 + (q01 - q00) * tx + (q10 - q00) * ty + tx * ty * (q00 - q01 - q10 + q11);
    }
    private float getWeight(int row, int col) => _wieghts[row + col * Cols];
    private float xDerivative(int row, int col) => (getWeight(row, Mathf.Min(col + 1, Cols - 1)) - getWeight(row, Mathf.Max(col - 1, 0))) * 0.5f;
    private float yDerivative(int row, int col) => (getWeight(Mathf.Min(row + 1, Rows - 1), col) - getWeight(Mathf.Max(row - 1, 0), col)) * 0.5f;
    float xyDerivative(int row, int col)
    {
        int row1 = Mathf.Min(row + 1, Rows - 1);
        int row0 = Mathf.Max(0, row - 1);

        int col1 = Mathf.Min(col + 1, Cols - 1);
        int col0 = Mathf.Max(0, col - 1);

        return (getWeight(row1, col1) - getWeight(row1, col0)) * 0.25f -
               (getWeight(row0, col1) - getWeight(row0, col0)) * 0.25f;
    }
    public float GetWeightsBicubic(Vector2 uv)
    {
        if (uv.x < 0.0f) return MAX_WEIGHT;
        if (uv.y < 0.0f) return MAX_WEIGHT;

        if (uv.x > 1.0f) return MAX_WEIGHT;
        if (uv.y > 1.0f) return MAX_WEIGHT;

        int col, row, col1, row1;
        float tx, ty;

        col = (int)(uv.x * (Cols - 1));
        row = (int)(uv.y * (Rows - 1));

        col1 = Mathf.Min(col + 1, Cols - 1);
        row1 = Mathf.Min(row + 1, Rows - 1);

        float dx = 1.0f / (Cols - 1);
        float dy = 1.0f / (Rows - 1);

        tx = Mathf.Min(1.0f, (uv.x - col * dx) / dx);
        ty = Mathf.Min(1.0f, (uv.y - row * dy) / dy);

        float[] b = new float[16];

        float[] c = new float[16];

        b[0] = getWeight(row,  col);
        b[1] = getWeight(row,  col1);
        b[2] = getWeight(row1, col);
        b[3] = getWeight(row1, col1);

        b[4] = xDerivative(row,  col);
        b[5] = xDerivative(row,  col1);
        b[6] = xDerivative(row1, col);
        b[7] = xDerivative(row1, col1);

        b[8]  = yDerivative(row,  col);
        b[9]  = yDerivative(row,  col1);
        b[10] = yDerivative(row1, col);
        b[11] = yDerivative(row1, col1);

        b[12] = xyDerivative(row,  col);
        b[13] = xyDerivative(row,  col1);
        b[14] = xyDerivative(row1, col);
        b[15] = xyDerivative(row1, col1);

	    c[0]  =         b[0];
	    c[1]  =         b[8];
	    c[2]  = -3.0f * b[0] + 3.0f * b[2] - 2.0f * b[8] -        b[10];
	    c[3]  =  2.0f * b[0] - 2.0f * b[2] +        b[8] +        b[10];
	    c[4]  =         b[4];
	    c[5]  =         b[12];
	    c[6]  = -3.0f * b[4] + 3.0f * b[6] - 2.0f * b[12] -        b[14];
	    c[7]  =  2.0f * b[4] - 2.0f * b[6] +        b[12] +        b[14];
	    c[8]  = -3.0f * b[0] + 3.0f * b[1] - 2.0f * b[4]  -        b[5];
	    c[9]  = -3.0f * b[8] + 3.0f * b[9] - 2.0f * b[12] -        b[13];
	    c[10] =  9.0f * b[0] - 9.0f * b[1] - 9.0f * b[2]  + 9.0f * b[3]  + 6.0f * b[4]  + 3.0f * b[5]   - 6.0f * b[6] - 3.0f * b[7] +
	    	     6.0f * b[8] - 6.0f * b[9] + 3.0f * b[10] - 3.0f * b[11] + 4.0f * b[12] + 2.0f * b[13] + 2.0f * b[14] +        b[15];
	    c[11] = -6.0f * b[0] + 6.0f * b[1] + 6.0f * b[2]  - 6.0f * b[3]  - 4.0f * b[4]  - 2.0f * b[5]  + 4.0f * b[6]  + 2.0f * b[7] - 
	    	     3.0f * b[8] + 3.0f * b[9] - 3.0f * b[10] + 3.0f * b[11] - 2.0f * b[12] -        b[13] - 2.0f * b[14] -        b[15];
	    c[12] =  2.0f * b[0] - 2.0f * b[1] +        b[4]  +        b[5];
	    c[13] =  2.0f * b[8] - 2.0f * b[9] +        b[12] +        b[13];
	    c[14] = -6.0f * b[0] + 6.0f * b[1] + 6.0f * b[2]  - 6.0f * b[3]  - 3.0f * b[4]  - 3.0f * b[5]  + 3.0f * b[6]  + 3.0f * b[7] -
	    	     4.0f * b[8] + 4.0f * b[9] - 2.0f * b[10] + 2.0f * b[11] - 2.0f * b[12] - 2.0f * b[13] -        b[14] -        b[15];
	    c[15] =  4.0f * b[0] - 4.0f * b[1] - 4.0f * b[2]  + 4.0f * b[3]  + 2.0f * b[4]  + 2.0f * b[5]  - 2.0f * b[6]  - 2.0f * b[7] +
	    		 2.0f * b[8] - 2.0f * b[9] + 2.0f * b[10] - 2.0f * b[11] +        b[12] +        b[13] +        b[14] +        b[15];

        float x2 = tx * tx;
        float x3 = x2 * tx;
        float y2 = ty * ty;
        float y3 = y2 * ty;

        return (c[0]  + c[1]  * ty + c[2]  * y2 + c[3]  * y3) +
               (c[4]  + c[5]  * ty + c[6]  * y2 + c[7]  * y3) * tx +
               (c[8]  + c[9]  * ty + c[10] * y2 + c[11] * y3) * x2 +
               (c[12] + c[13] * ty + c[14] * y2 + c[15] * y3) * x3;
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

