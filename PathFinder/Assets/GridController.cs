using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridController : MonoBehaviour
{
    [SerializeField]
    TMP_Text _xLabel;
    [SerializeField]
    TMP_Text _yLabel;
    [SerializeField]
    TMP_Text _stepLabel;
    [SerializeField]
    GameObject _grid;

    private static readonly float UnityPlaneHalfSize = 5.0f;
    private static readonly float XLabelDx = 1.3f;
    private static readonly float YLabelDy = 1.3f;
    void Start() => SetupSize(new Vector2(10, 10));
    public void SetupSize(Vector2 size)
    {
        if (!_grid) return;
        _grid.transform.localScale = new Vector3(size.x / UnityPlaneHalfSize, 1.0f, size.y / UnityPlaneHalfSize);

        if (_xLabel)
        {
            _xLabel.transform.position = new Vector3(UnityPlaneHalfSize * _grid.transform.lossyScale.x + XLabelDx, 0, 2.73f);
         
        }

        if (_yLabel)
        {
            _yLabel.transform.position = new Vector3(-0.3f, 0, UnityPlaneHalfSize * _grid.transform.lossyScale.z + YLabelDy);
      
        }

        if (_stepLabel) 
        {
            _stepLabel.transform.position = new Vector3(0, 0, -UnityPlaneHalfSize * _grid.transform.lossyScale.z - YLabelDy);
        }

    }
 
}
