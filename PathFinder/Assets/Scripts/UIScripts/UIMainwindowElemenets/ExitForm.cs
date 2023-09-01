using UnityEngine.UIElements;
using UnityEngine;

public class ExitForm : UIForm
{
    private VisualElement _saveButton;
    private VisualElement _exitButton;
    public override bool Init()
    {
        if (!base.Init()) return false;
        if ((_saveButton   = Q<Button>("SaveAllBeforeExitBtn")) == null) return false;
        if ((_exitButton = Q<Button>("JustExitBtn")) == null) return false;
        _saveButton.RegisterCallback<ClickEvent>(evt => {
            string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("SaveMap", "", false);
            if (paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            AreaMap.Instance.SaveMap(paths[0]);
            Application.Quit();
        });
        _exitButton.RegisterCallback<ClickEvent>(evt => Application.Quit());
        return true;
    }
    public new void Show() 
    {
        if (!PointsContainer.Instance.ContainsAnyData) 
        {
            Application.Quit();
            return;
        }
        base.Show();
    }
    public ExitForm(UIMainWindow _parent) : base(_parent, "ApplicationExitForm")
    {}
}
