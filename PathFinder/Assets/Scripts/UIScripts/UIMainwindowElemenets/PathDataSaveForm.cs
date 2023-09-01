using UnityEngine.UIElements;

public class PathDataSaveForm : UIForm
{
    private int           _saveOption;
    private Button        _saveButton;
    private DropdownField _saveOptionDropDown;

    private void SavePaths()
    {
        switch (_saveOption)
        {
            case 3: PointsContainer.Instance.SaveRawPathJson(); break;
            case 2: PointsContainer.Instance.SaveProcessedPathJson(); break;
            case 0: PointsContainer.Instance.SaveRawPath(); break;
            case 1: PointsContainer.Instance.SaveProcessedPath(); break;
            case 4: PointsContainer.Instance.SaveAllPaths(); break;
        }
    }

    public override bool Init()
    {
        if (!base.Init()) return false;
        _saveButton         = Q<Button>        ("SavePathDataBtn");
        _saveOptionDropDown = Q<DropdownField> ("PathSaveOptionDropDown");
        _saveButton.RegisterCallback<ClickEvent>(evt => {
            SavePaths();
            Hide();
        });
        _saveOptionDropDown.RegisterValueChangedCallback(evt => _saveOption = _saveOptionDropDown.index);
        return true;
    }
    public PathDataSaveForm(UIMainWindow _parent) : base(_parent, "PathSaveSelectionForm")
    {
    }
}
