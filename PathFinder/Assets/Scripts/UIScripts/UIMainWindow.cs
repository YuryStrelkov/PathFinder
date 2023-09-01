using UnityEngine.UIElements;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(UIDocument))]
public class UIMainWindow : MonoBehaviour
{
    UIDocument _document;
    public VisualElement Root
    {
        get
        {
            if (_root == null) _root = _document.rootVisualElement.Q<VisualElement>("uiRoot");
            return _root;
        }
    }
    ExitForm         _exitForm;
    PathDataSaveForm _pathDataSaveForm;
    UIForm           _aboutForm;
    MessageForm      _messageForm;
    SettingsForm     _settingsForm;
    SendPathForm     _sendPathForm;

    public UIForm active = null;
    VisualElement _root = null;
    Button        _loadMapBtn;
    Button        _moveBtn;
    Button        _settingsBtn;
    Button        _savePathBtn;
    Button        _aboutBtn;
    Button        _exitBtn;
    Button        _saveMapBtn;
    
    public void RunScripts(string _scriptPath, string arg1, string arg2)
    {
        Process process = new Process();
        process.StartInfo.FileName = "python"; // Запускаем командную строку
        string command = $" {_scriptPath} {arg1} {arg2}";
        process.StartInfo.Arguments = command; // Указываем команду для запуска скрипта
        process.StartInfo.UseShellExecute = false; // Не используем оболочку командной строки
        process.StartInfo.RedirectStandardOutput = true; // Перенаправляем вывод в стандартный поток
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd(); // Считываем вывод из консоли Python
        process.WaitForExit();
        UnityEngine.Debug.Log(output);
    }
    private void InitSettingsForm() => _settingsForm = new SettingsForm(this);
    private void InitAboutForm() => _aboutForm = new UIForm(this, "AboutForm");
    private void InitMoveForm() => _sendPathForm = new SendPathForm(this);
    private void InitApplicationExitForm() => _exitForm = new ExitForm(this);
    private void InitSavePathForm() => _pathDataSaveForm = new PathDataSaveForm(this);
    private void InitMessageForm() => _messageForm = new MessageForm(this);

    private void InitElements() 
    {
        _document    = GetComponent<UIDocument>();
        _savePathBtn = Root.Q<Button>("SavePathBtn");
        _saveMapBtn  = Root.Q<Button>("SaveMapBtn");
        _loadMapBtn  = Root.Q<Button>("LoadMapBtn");
        _moveBtn     = Root.Q<Button>("StartMovementBtn");
        _settingsBtn = Root.Q<Button>("SettingsBtn");
        _aboutBtn    = Root.Q<Button>("AboutBtn");
        _exitBtn     = Root.Q<Button>("ExitBtn");
        InitSavePathForm();
        InitApplicationExitForm();
        InitMoveForm();
        InitSettingsForm();
        InitAboutForm();
        InitMessageForm();
    }
    private void InitCalbacks() 
    {
        _loadMapBtn.RegisterCallback<ClickEvent>(evt =>
        {
            if (active != null) return;
            string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("SaveProcessedPathArray", "", false);
            if (paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            AreaMap.Instance.LoadMap(paths[0]);
        });

        _saveMapBtn.RegisterCallback<ClickEvent>(evt =>
        {
            if (active != null) return;
            if (!PointsContainer.Instance.ContainsAnyData)
            {
                _messageForm.MessageText = "Нет путей, которые можно сохранить...";
                _messageForm.Show();
                return;
            }
            string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("SaveMap", "", false);
            if (paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            AreaMap.Instance.SaveMap(paths[0]);
            // RunScripts("sum.py", "1.2", "-1.1");
        });

        _moveBtn.RegisterCallback<ClickEvent>(evt => {
            if (active != null) return;
            if (!PointsContainer.Instance.ContainsAnyData)
            {
                _messageForm.MessageText = "Нет путей, которые можно отпрваить на устройство...";
                _messageForm.Show();
                return;
            }
            if (_sendPathForm.IsShown) return;
            _sendPathForm.Show();
        });

        _exitBtn.RegisterCallback<ClickEvent>(evt => {
            if (active != null) return;
            if (_exitForm.IsShown) return;
            _exitForm.Show();
        });

        _savePathBtn.RegisterCallback<ClickEvent>(evt => {
            if (active != null) return;
            if (!PointsContainer.Instance.ContainsAnyData)
            {
                _messageForm.MessageText = "Нет путей, которые можно сохранить...";
                _messageForm.Show();
                return;
            }
            if (_pathDataSaveForm.IsShown) return;
            _pathDataSaveForm.Show();
        });

        _settingsBtn.RegisterCallback<ClickEvent>(evt =>
        {
            if (active != null) return;
            if (_settingsForm.IsShown) return;
            _settingsForm.Show();
        });
        
        _aboutBtn.RegisterCallback<ClickEvent>(evt =>
        {
            if (active != null) return;
            if (_aboutForm.IsShown) return;
            _aboutForm.Show();
        });

        var _switchProjectionBtn = Root.Q<Button>("switchProjectionBtn");
        _switchProjectionBtn.RegisterCallback<ClickEvent>((evt) =>
        { 
            if (active != null) return;
            CamController.Instance.SwitchProjection();
        });
    }

    void Start()
    {
        InitElements();
        InitCalbacks();
    }
}
