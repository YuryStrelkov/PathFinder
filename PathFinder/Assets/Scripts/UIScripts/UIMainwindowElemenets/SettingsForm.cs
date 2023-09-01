using UnityEngine.UIElements;
using UnityEngine;

public class SettingsForm : UIForm
{
    private int    _weightMapResolution;
    private int    _referenceMapResolution;
    private int    _decimation;
    private int    _smoothing;
    private string _weightMapDirectory;
    private string _referenceMapDirectory;
    private float  _lineWidth;
    private Color  _lineColor;

    private DropdownField _weightMapResolutionOptions;
    private DropdownField _referenceMapResolutionOptions;
    private DropdownField _decimationOptions;
    private DropdownField _smoothingOptions;

    private Button        _selectWeightMapDirectoryBtn;
    private Button        _selectReferenceMapDirectoryBtn;
    private Label         _selectWeightMapDirectoryLabel;
    private Label         _selectReferenceMapDirectoryLabel;

    private Button        _saveSettingsBtn;
    private Button        _loadSettingsBtn;
    private Button        _resetSettingsBtn;

    private void LinkCallbacks() 
    {
        _weightMapResolutionOptions.   RegisterValueChangedCallback(evt => _weightMapResolution    = _weightMapResolutionOptions.index);
        _referenceMapResolutionOptions.RegisterValueChangedCallback(evt => _referenceMapResolution = _referenceMapResolutionOptions.index);
        _decimationOptions.            RegisterValueChangedCallback(evt => _decimation             = _decimationOptions.index);
        _smoothingOptions.             RegisterValueChangedCallback(evt => _smoothing              = _smoothingOptions.index);

        _selectWeightMapDirectoryBtn.RegisterCallback<ClickEvent>(evt =>
        {
            string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Выбор директории хранения технической карты...", "", false);
            if (paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            if(_weightMapDirectory == paths[0]) return;
            _selectWeightMapDirectoryLabel.text = $"Директория сохранения : {paths[0]}";
            _weightMapDirectory = paths[0];
        });

        _selectReferenceMapDirectoryBtn.RegisterCallback<ClickEvent>(evt =>
        {
            string[] paths = SFB.StandaloneFileBrowser.OpenFolderPanel("Выбор директории хранения опорных ракурсов...", "", false);
            if (paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            if(_referenceMapDirectory == paths[0]) return;
            _selectReferenceMapDirectoryLabel.text = $"Директория сохранения : {paths[0]}";
            _referenceMapDirectory = paths[0];
        });

    }
    public override bool Init()
    {
        if(!base.Init())return false;
        if ((_weightMapResolutionOptions    = Q<DropdownField>("WeightMapResolution"))    == null) return false;
        if ((_referenceMapResolutionOptions = Q<DropdownField>("ReferenceMapResolution")) == null) return false;
        if ((_decimationOptions             = Q<DropdownField>("Decimation"))             == null) return false;
        if ((_smoothingOptions              = Q<DropdownField>("Smoothing"))              == null) return false;

        if ((_selectWeightMapDirectoryBtn      = Q<Button>("WeightMapSaveDirectory"))        == null) return false;
        if ((_selectReferenceMapDirectoryBtn   = Q<Button>("ReferenceMapSaveDirectory"))     == null) return false;
        if ((_selectWeightMapDirectoryLabel    = Q<Label>("WeightMapSaveDirectoryLabel"))    == null) return false;
        if ((_selectReferenceMapDirectoryLabel = Q<Label>("ReferenceMapSaveDirectoryLabel")) == null) return false;

        if ((_saveSettingsBtn  = Q<Button>("SaveSettings"))  == null) return false;
        if ((_loadSettingsBtn  = Q<Button>("LoadSettings"))  == null) return false;
        if ((_resetSettingsBtn = Q<Button>("ResetSettings")) == null) return false;
        
        LinkCallbacks();

        return true;
    }
    public SettingsForm(UIMainWindow _parent) : base(_parent, "SettingsForm")
    {
    }
}
