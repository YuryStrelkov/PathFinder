using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainWindowUI : MonoBehaviour
{
    #region CommonPathParameters
    UIDocument _document;

    private DropdownField _file;
    private DropdownField _edit;
    private DropdownField _settings;
    private Button _btn;
    private Button _2d3d;
    private ScrollView _scrollView;

    VisualElement _root = null;

    VisualElement Root
    {
        get
        {
            if (_root == null) _root = _document.rootVisualElement.Q<VisualElement>("uiRoot");
            return _root;
        }
    }

    private bool InitDropDowns(VisualElement parent = null)
    {
        GroupBox container = VisualElementUtils.CreateGroup(parent == null ? Root : parent, false, Position.Relative);

        List<string> dd1 = new List<string> { "item dd1 1", "item dd1 2", "item dd1 3", "item dd1 4", "item dd1 5", "item dd1 6" };
        List<string> dd2 = new List<string> { "item dd2 1", "item dd2 2", "item dd2 3", "item dd2 4", "item dd2 5", "item dd2 6" };
        List<string> dd3 = new List<string> { "item dd3 1", "item dd3 2", "item dd3 3", "item dd3 4", "item dd3 5", "item dd3 6" };


        _file = VisualElementUtils.CreateDropDown(container, "File", Position.Relative, dd1);
        if (_file == null) return false;
        _edit = VisualElementUtils.CreateDropDown(container, "Edit", Position.Relative, dd2);
        _settings = VisualElementUtils.CreateDropDown(container, "Settings", Position.Relative, dd3);

        return true;
    }

    private bool InitButtons(VisualElement parent = null)
    {
        GroupBox container = VisualElementUtils.CreateGroup(parent == null ? Root : parent, true, Position.Relative);

        GroupBox hor_container_1 = VisualElementUtils.CreateGroup(container, false, Position.Relative);

        _btn = VisualElementUtils.CreateButton(hor_container_1, "BUSIDo");
        if (_btn == null) return false;

        GroupBox hor_container_2 = VisualElementUtils.CreateGroup(container, false, Position.Relative);

        _2d3d = VisualElementUtils.CreateButton(hor_container_2, "2D/3D");
        if (_2d3d == null) return false;


        return true;
    }

    private bool InitScrollView(VisualElement parent = null)
    {
        GroupBox container = VisualElementUtils.CreateGroup(parent == null ? Root : parent, true, Position.Relative);

        _scrollView = VisualElementUtils.CreateScrollView(container, Position.Relative);
        if (_scrollView == null) return false;

        PathSegmentUIController temp = new PathSegmentUIController();
        _scrollView.Add(temp.Root);

        return true;
    }

    private bool InitMainWindow()
    {
        GroupBox container = VisualElementUtils.CreateGroup(Root, true, Position.Relative);
        container.style.flexShrink = new StyleFloat(1.0f);
        container.style.flexGrow = new StyleFloat(1.0f);

        if (!InitDropDowns(container)) throw new System.Exception("Dropdowns init error...");
        if (!InitButtons(container)) throw new System.Exception("Buttons init error...");
        if (!InitScrollView(container)) throw new System.Exception("Scroll view init error...");


        return true;
    }

    #endregion
    void Start()
    {
        _document = GetComponent<UIDocument>();
        if (!InitMainWindow()) throw new System.Exception("Init main window error...");
    }
}
