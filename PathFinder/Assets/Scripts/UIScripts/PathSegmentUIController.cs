using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Vector3UI
{
    private GroupBox _vectorContainer;
    private Label _vectorName;
    private Label _xName;
    private Label _yName;
    private Label _zName;
    private Label _xValue;
    private Label _yValue;
    private Label _zValue;

    public string VectorName
    {
        get => _vectorName.text;
        set => _vectorName.text = string.IsNullOrEmpty(value) ? _vectorName.text : value;
    }
    public string XLabelName
    {
        get => _xName.text;
        set => _xName.text = string.IsNullOrEmpty(value) ? _xName.text : value;
    }
    public string YLabelName
    {
        get => _yName.text;
        set => _yName.text = string.IsNullOrEmpty(value) ? _yName.text : value;
    }
    public string ZLabelName
    {
        get => _zName.text;
        set => _zName.text = string.IsNullOrEmpty(value) ? _zName.text : value;
    }
    public float XLabelValue
    {
        get => float.Parse(_xValue.text);
        set => _xValue.text = $"{System.String.Format("{0:0.000}", value)}";
    }
    public float YLabelValue
    {
        get => float.Parse(_yValue.text);
        set => _yValue.text = $"{System.String.Format("{0:0.000}", value)}";
    }
    public float ZLabelValue
    {
        get => float.Parse(_zValue.text);
        set => _zValue.text = $"{System.String.Format("{0:0.000}", value)}";
    }

    public Vector3UI(VisualElement parent = null, bool vertical = true, string vectorName = "vectorName")
    {
        if(!InitVector(parent, vertical, vectorName)) throw new System.Exception("Vector params ui init error...");
    }

    private bool InitVector(VisualElement parent = null, bool vertical = true, string vectorName = "vectorName")
    {
        _vectorContainer = VisualElementUtils.CreateGroup(parent, vertical);
        GroupBox vectorGroup = VisualElementUtils.CreateGroup(_vectorContainer, vertical, Position.Relative);

        Label[] xlabel = VisualElementUtils.InitTextNamedField(vectorGroup, "x", "0.0");
        if (xlabel == null) return false;
        Label[] ylabel = VisualElementUtils.InitTextNamedField(vectorGroup, "y", "0.0");
        Label[] zlabel = VisualElementUtils.InitTextNamedField(vectorGroup, "z", "0.0");

        _xName = xlabel[0];
        _yName = ylabel[0];
        _zName = zlabel[0];
        _xValue = xlabel[1];
        _yValue = ylabel[1];
        _zValue = zlabel[1];

        _vectorName = new Label();
        _vectorName.text = vectorName;
        _vectorName.AddToClassList("Labels");

        _vectorContainer.Add(_vectorName);
        _vectorContainer.Add(vectorGroup);

        return true;
    }
}

[RequireComponent(typeof(UIDocument))]
public class PathSegmentUIController : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset _namedTextFieldAsset;
    #region CommonPathParameters
    private Label _pathName;
    private Label _pathRawLength;
    private Label _pathProccesedLength;
    private Label _pathPointsCount;

    private Vector3UI _start;
    private Vector3UI _end;

    private Button _go;
    private Button _stop;
    private Button _back;
    private Button _goBack;
    private Button _goStraight;
    private Button _displayButton;
    private Button _loadButton;

    private DropdownField _decimate;
    private DropdownField _pathSmoothing;

    UIDocument _document;

    VisualElement _root = null;

    public VisualElement Root
    {
        get
        {
            if (_root == null) _root = _document.rootVisualElement.Q<VisualElement>("uiRoot");
            return _root;
        }
    }

    public string PathName
    {
        get => _pathName.text;
        set
        {
            _pathName.text = value;
            _displayButton.text = _pathName.text + " (Скрыть/показать)";
        }
    }

    public float PathRawLength
    {
        get => float.Parse(_pathRawLength.text);
        set => _pathRawLength.text = $"{System.String.Format("{0:0.000}", value)}";
    }

    public float PathProccesedLength
    {
        get => float.Parse(_pathProccesedLength.text);
        set => _pathProccesedLength.text = $"{System.String.Format("{0:0.000}", value)}";
    }

    public int PathPointsCount
    {
        get => int.Parse(_pathPointsCount.text);
        set => _pathPointsCount.text = value.ToString();
    }

    public Vector3 StartPoint
    {
        get => new Vector3(_start.XLabelValue, _start.YLabelValue, _start.ZLabelValue);
        set
        {
            _start.XLabelValue = value.x;
            _start.YLabelValue = value.y;
            _start.ZLabelValue = value.z;
        }
    }

    public Vector3 EndPoint
    {
        get => new Vector3(_end.XLabelValue, _end.YLabelValue, _end.ZLabelValue);
        set
        {
            _end.XLabelValue = value.x;
            _end.YLabelValue = value.y;
            _end.ZLabelValue = value.z;
        }
    }

    public DropdownField Decimate => _decimate;

    public DropdownField PathSmoothing => _pathSmoothing;

    public Button Go => _go;
    public Button Stop => _stop;
    public Button Back => _back;
    public Button GoStraight => _goStraight;
    public Button GoBack => _goBack;

    public void Add<T>(T element) where T : VisualElement => Root.Add(element);

   private bool InitCommonPathParamsUI(VisualElement parent = null) 
    {
        if (_namedTextFieldAsset == null) return false;

        GroupBox group = VisualElementUtils.CreateGroup(parent == null ? Root : parent, true, Position.Relative);

        //TODO runtime group styles setup
        bool state = true;

        Label[] tmpLabelArray = VisualElementUtils.InitTextNamedField(group, "Имя сегмента", "без имени");
        _pathName = tmpLabelArray[1];
        state &= _pathName != null;

        tmpLabelArray = VisualElementUtils.InitTextNamedField(group, "Длина сегмента", "0.0");
        _pathRawLength = tmpLabelArray[1];
        state &= _pathRawLength != null;

        tmpLabelArray = VisualElementUtils.InitTextNamedField(group, "Длина обработанного сегмента", "0.0");
        _pathProccesedLength = tmpLabelArray[1];
        state &= _pathProccesedLength != null;

        tmpLabelArray = VisualElementUtils.InitTextNamedField(group, "Количество точек", "0");
        _pathPointsCount = tmpLabelArray[1];
        state &= _pathPointsCount != null;

        return state;
    }

    private bool InitPathBoundsUI(VisualElement parent = null)
    {
        GroupBox container = VisualElementUtils.CreateGroup(parent == null ? Root : parent, false, Position.Relative);

        _start = new Vector3UI(container);
        _start.VectorName = "Начало сегмента";
        _end = new Vector3UI(container);
        _end.VectorName = "Конец сегмента";
      
        return true;
    }

    private bool InitDropDownUI(VisualElement parent = null)
    {
        GroupBox container = VisualElementUtils.CreateGroup(parent == null ? Root : parent, true, Position.Relative);

        List<string> dd1 = new List<string> { "item dd1 1", "item dd1 2", "item dd1 3", "item dd1 4", "item dd1 5", "item dd1 6" };
        List<string> dd2 = new List<string> { "item dd2 1", "item dd2 2", "item dd2 3", "item dd2 4", "item dd2 5", "item dd2 6" };

        _decimate = VisualElementUtils.CreateDropDown(container, "Decimate", Position.Relative, dd1);
        _decimate.AddToClassList("DropDowns");
        _pathSmoothing = VisualElementUtils.CreateDropDown(container, "Path smoothing", Position.Relative, dd2);
        _pathSmoothing.AddToClassList("DropDowns");

        return true;
    }

    private bool InitButtonsUI(VisualElement parent = null)
    {
        GroupBox container = VisualElementUtils.CreateGroup(parent == null ? Root : parent, true, Position.Relative);

        GroupBox hor_container_1 = VisualElementUtils.CreateGroup(container, false, Position.Relative);
        GroupBox hor_container_2 = VisualElementUtils.CreateGroup(container, false, Position.Relative);

        _goStraight = VisualElementUtils.CreateButton(hor_container_1, "иди вперед");
        if (_goStraight == null) return false;

        _goBack = VisualElementUtils.CreateButton(hor_container_1, "иди назад");
        if (_goBack == null) return false;

        _go = VisualElementUtils.CreateButton(hor_container_2, "иди");
        if (_go == null) return false;

        _stop = VisualElementUtils.CreateButton(hor_container_2, "стой");
        if (_stop == null) return false;

        _back = VisualElementUtils.CreateButton(hor_container_2, "обратно");
        if (_back == null) return false;

        return true;
    }

    private bool InitCollapsableLayout()
    {
        GroupBox container = VisualElementUtils.CreateGroup(Root, true, Position.Relative);
        container.style.flexShrink = new StyleFloat(1.0f);
        container.style.flexGrow = new StyleFloat(0.0f);

        _loadButton = VisualElementUtils.CreateButton(container, "Загрузить");
        _loadButton.style.flexShrink = new StyleFloat(1.0f);
        _loadButton.style.flexGrow = new StyleFloat(0.0f);

        _displayButton = VisualElementUtils.CreateButton(container, "Скрыть/показать");
        _displayButton.style.flexShrink = new StyleFloat(1.0f);
        _displayButton.style.flexGrow = new StyleFloat(0.0f);

        GroupBox collapsableLayout = VisualElementUtils.CreateGroup(container, true);

        if (!InitCommonPathParamsUI(collapsableLayout)) throw new System.Exception("Common path params ui init error...");
        if (!InitPathBoundsUI(collapsableLayout)) throw new System.Exception("Path bounds params init error...");
        if (!InitDropDownUI(collapsableLayout)) throw new System.Exception("Dropdowns init error...");
        if (!InitButtonsUI(collapsableLayout)) throw new System.Exception("Buttons init error...");

        _displayButton.RegisterCallback<ClickEvent>((evt) => 
        {
            collapsableLayout.style.display = collapsableLayout.style.display == DisplayStyle.None ? DisplayStyle.Flex: DisplayStyle.None;
        });

        _loadButton.RegisterCallback<ClickEvent>((evt) =>
        {
            Debug.Log("load");
            
        });

        return true;
    }

    #endregion
    void Start()
    {
        _document = GetComponent<UIDocument>();
        Root.style.width = new StyleLength(new Length(600.0f, LengthUnit.Pixel));
        Root.AddToClassList("Containers");

        if (!InitCollapsableLayout()) throw new System.Exception("Collapsable ui init error...");

        PathName = "BUSIDO";
        PathRawLength = 99999;
        PathPointsCount = 100;
        PathProccesedLength = 899;
        StartPoint = new Vector3(10, 12, 21);
        EndPoint = new Vector3(1, 1, 2);
    }
}
