using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIController : MonoBehaviour
{
    VisualElement _root = null;
    Button _savePathBtn;
    Button _saveMapBtn;
    Button _loadMapBtn;
    Button _moveBtn;
    Button _settingsBtn;
    Button _aboutBtn;
    Button _exitBtn;


    VisualElement _saveButtonsContainer;
    Button _savePathCloseBtn;
    Button _savePathRawJson;
    Button _savePathProcessedJson;
    Button _savePathRawArray;
    Button _savePathProcessedArray;
    Button _saveAllPathInfo;


    VisualElement _exitFormContainer;
    VisualElement _moveForm;
    VisualElement _settingsForm;
    VisualElement _aboutForm;

    private void InitSettingsForm()
    {
        _settingsForm = Root.Q<VisualElement>("SettingsForm");
        Button _continue = _settingsForm.Q<Button>("ContinueBtn");
        _continue.RegisterCallback<ClickEvent>(evt =>
        {
            _settingsForm.style.display = DisplayStyle.None;
        });
    }

    private void InitAboutForm()
    {
        _aboutForm = Root.Q<VisualElement>("AboutForm");
        Button _continue = _aboutForm.Q<Button>("ContinueBtn");
        _continue.RegisterCallback<ClickEvent>(evt =>
        {
            _aboutForm.style.display = DisplayStyle.None;
        });
    }

    private void InitMoveForm()
    {
        _moveForm = Root.Q<VisualElement>("MoveForm");
        Button _continue = _moveForm.Q<Button>("ContinueBtn");
        _continue.RegisterCallback<ClickEvent>(evt =>
        {
            _moveForm.style.display = DisplayStyle.None;
        });
    }
    private void InitApplicationexitForm() 
    {
        _exitFormContainer = Root.Q<VisualElement>("ApplicationExitForm");
        Button _exit   = _exitFormContainer.Q<Button>("SaveAllBeforeExitBtn");
        Button _save   = _exitFormContainer.Q<Button>("JustExitBtn");
        Button _cancel = _exitFormContainer.Q<Button>("CancelExitFormBtn");
        _save.RegisterCallback<ClickEvent>(evt => {
            SaveAllPaths();
            Application.Quit();
        });
        _exit.RegisterCallback<ClickEvent>(evt => {
            Application.Quit();
        });
        _cancel.RegisterCallback<ClickEvent>(evt => {
            _exitFormContainer.style.display = DisplayStyle.None;
        });
    }

    private void InitSavePathForm() 
    {
        _saveButtonsContainer   = Root.Q<VisualElement>("PathSaveSelectionForm");
        _saveAllPathInfo        = Root.Q<Button>("SaveAllPathInfoBtn");
        _savePathCloseBtn       = Root.Q<Button>("CloseSavePathAsBtn");
        _savePathBtn.RegisterCallback<ClickEvent>(evt => _saveButtonsContainer.style.display = DisplayStyle.Flex);
        _savePathCloseBtn.RegisterCallback<ClickEvent>(evt => _saveButtonsContainer.style.display = DisplayStyle.None);
        _savePathRawJson        = Root.Q<Button>("SaveRawPathJson");
        _savePathProcessedJson  = Root.Q<Button>("SaveProcessedPathJson");
        _savePathRawArray       = Root.Q<Button>("SaveRawPathArray");
        _savePathProcessedArray = Root.Q<Button>("SaveProcessedPathArray");
        _savePathRawJson.RegisterCallback<ClickEvent>(evt => 
        {
            string paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveRawPathJson", "", "", "json");
            if (string.IsNullOrEmpty(paths)) return;
            PointsContainer.Instance.SaveRawPathJson(paths);
            _saveButtonsContainer.style.display = DisplayStyle.None;
        });
        _savePathProcessedJson.RegisterCallback<ClickEvent>(evt =>
        {
            string paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveProcessedPathJson", "", "", "json");
            if (string.IsNullOrEmpty(paths)) return;
            PointsContainer.Instance.SaveProcessedPathJson(paths);
            _saveButtonsContainer.style.display = DisplayStyle.None;
        });
        _savePathRawArray.RegisterCallback<ClickEvent>(evt =>
        {
            string paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveRawPathArray", "", "", "txt");
            if (string.IsNullOrEmpty(paths)) return;
            PointsContainer.Instance.SaveRawPath(paths);
            _saveButtonsContainer.style.display = DisplayStyle.None;
        });
        _savePathProcessedArray.RegisterCallback<ClickEvent>(evt =>
        {
            string paths = SFB.StandaloneFileBrowser.SaveFilePanel("SaveProcessedPathArray", "", "", "txt");
            if (string.IsNullOrEmpty(paths)) return;
            PointsContainer.Instance.SaveProcessedPath(paths);
            _saveButtonsContainer.style.display = DisplayStyle.None;
        });

        _saveAllPathInfo.RegisterCallback<ClickEvent>(evt =>
        {
            SaveAllPaths();
        });
    }
    private void SaveAllPaths()
    {
        if (!PointsContainer.Instance.ContainsAnyData) return;
        string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Save session as...", "", false);
        if (paths.Length == 0) return;
        if (string.IsNullOrEmpty(paths[0])) return;
        try
        {
            PointsContainer.Instance.SaveProcessedPathJson(paths[0] + "\\raw_path.json");
            PointsContainer.Instance.SaveRawPathJson(paths[0] + "\\processed_path.json");
            PointsContainer.Instance.SaveRawPath(paths[0] + "\\raw_path.txt");
            PointsContainer.Instance.SaveProcessedPath(paths[0] + "\\processed_path.txt");
            _saveButtonsContainer.style.display = DisplayStyle.None;
        }
        catch (System.Exception ex)
        {
            _saveButtonsContainer.style.display = DisplayStyle.None;
        }
    }

    UIDocument _document;
    public VisualElement Root
    {
        get
        {
            if (_root == null) _root = _document.rootVisualElement.Q<VisualElement>("uiRoot");
            return _root;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _document = GetComponent<UIDocument>();
        _savePathBtn = Root.Q<Button>("SavePathBtn");
        _saveMapBtn  = Root.Q<Button>("SaveMapBtn");
        _loadMapBtn  = Root.Q<Button>("LoadMapBtn");
        _moveBtn     = Root.Q<Button>("StartMovementBtn");
        _settingsBtn = Root.Q<Button>("SettingsBtn");
        _aboutBtn    = Root.Q<Button>("AboutBtn");
        _exitBtn     = Root.Q<Button>("ExitBtn");

        // _savePathBtn.RegisterCallback<ClickEvent>((evt) => PointsContainer.Instance.SaveProcessedPath(PointsContainer.SaveDataPath));
        InitSavePathForm();
        InitApplicationexitForm();
        InitMoveForm();
        InitAboutForm();

        _loadMapBtn.RegisterCallback<ClickEvent>(evt => 
        {
            string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("SaveProcessedPathArray", "", false);
            if (paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            AreaMap.Instance.LoadMap(paths[0]);
        });
        _exitBtn.RegisterCallback<ClickEvent>(evt => {
            if (_exitFormContainer.style.display == DisplayStyle.Flex) return;
            _exitFormContainer.style.display = DisplayStyle.Flex;
        });
        _moveBtn.RegisterCallback<ClickEvent>(evt => 
        {
            if (_moveForm.style.display == DisplayStyle.Flex) return;
            _moveForm.style.display = DisplayStyle.Flex;
        });
        _settingsBtn.RegisterCallback<ClickEvent>(evt =>
        {
            if (_settingsForm.style.display == DisplayStyle.Flex) return;
            _settingsForm.style.display = DisplayStyle.Flex;
        });

        _aboutBtn.RegisterCallback<ClickEvent>(evt =>
        {
            if (_aboutForm.style.display == DisplayStyle.Flex) return;
            _aboutForm.style.display = DisplayStyle.Flex;
        });
        var _switchProjectionBtn = Root.Q<Button>("switchProjectionBtn");
        _switchProjectionBtn.RegisterCallback<ClickEvent>((evt) => { CamController.Instance.SwitchProjection(); });
    }

}
