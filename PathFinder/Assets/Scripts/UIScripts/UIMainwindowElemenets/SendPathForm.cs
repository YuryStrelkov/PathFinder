using UnityEngine.UIElements;
using UnityEngine;

public class SendPathForm : UIForm
{
    private static readonly int[] Baudrates = new int[] { 50, 75, 110, 134, 150, 200, 300, 600, 1200, 1800, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 500000, 576000, 921600, 1000000, 1152000, 1500000, 2000000, 2500000, 3000000, 3500000, 4000000 };
    private static readonly int[] Bytesizes = new int[] { 5, 6, 7, 8 };
    private static readonly int[] Timeouts  = new int[] { -1, 0, 1, 2, 3, 4, 5 };
    private Label                 _avaliableDevicesCountLabel;
    // private Label                 _statuslabel;
    private DropdownField         _avaliableDevicesOptions;
    private DropdownField         _baudrateOptions;
    private DropdownField         _bytesizeOptions;
    private DropdownField         _timeoutOptions;
    private Button                _sendPathToDeviceBtn;
    private TextAsset             _srcCode;
    private Label                 _pathSendLogLabel;
    private PythonScriptsExecutor _pythonScript;

    private string _port;
    private int    _baudrate;
    private int    _timeout;
    private int    _bytesize;

    public override bool Init()
    {
        if (!base.Init()) return false;
        if ((_avaliableDevicesCountLabel = Q<Label>        ("AvaliableDevicesCountLabel")) == null) return false;
        if ((_pathSendLogLabel           = Q<Label>        ("PathSendLogLabel"          )) == null) return false;
        if ((_avaliableDevicesOptions    = Q<DropdownField>("AvaliableDevicesOptions"   )) == null) return false;
        if ((_sendPathToDeviceBtn        = Q<Button>       ("SendPathToDeviceBtn"       )) == null) return false;
        if ((_baudrateOptions            = Q<DropdownField>("BaudrateOptions"           )) == null) return false;
        if ((_bytesizeOptions            = Q<DropdownField>("BytesizeOptions"           )) == null) return false;
        if ((_timeoutOptions             = Q<DropdownField>("TimeoutOptions"            )) == null) return false;

        if ((_srcCode = Resources.Load<TextAsset>("uart_command")) == null) return false;
        _avaliableDevicesOptions.choices = new System.Collections.Generic.List<string>();
        _avaliableDevicesOptions.RegisterValueChangedCallback(evt => { _port     = _avaliableDevicesOptions.choices[_avaliableDevicesOptions.index]; });
        _baudrateOptions.        RegisterValueChangedCallback(evt => { _baudrate = Baudrates[_baudrateOptions.index]; });
        _bytesizeOptions.        RegisterValueChangedCallback(evt => { _bytesize = Bytesizes[_bytesizeOptions.index]; });
        _timeoutOptions.         RegisterValueChangedCallback(evt => { _timeout  = Timeouts [_timeoutOptions. index]; });
        _sendPathToDeviceBtn.    RegisterCallback<ClickEvent>(evt => _pathSendLogLabel.text += $"\n{SendMessageToUART()}");
        _port = "";
        _baudrate = Baudrates[_baudrateOptions.index];
        _bytesize = Bytesizes[_bytesizeOptions.index];
        _timeout  = Timeouts [_timeoutOptions.index];
        _pythonScript = new PythonScriptsExecutor(_srcCode.text);
        return true;
    }

    private string SendMessageToUART(bool processed = true) 
    {
        if(_port == "") return $"{{\n\t\"errors\": [\"COM ports scan error. No ports found...\"]\n}}";
        var points = PointsContainer.Instance.PointsRaw;
        if (points == null) return $"{{\n\t\"errors\": [\"Path data is empty\"]\n}}";
        UARTMessage message = new UARTMessage()
        {
            baudrate = _baudrate,
            bytesize = _bytesize,
            mode     = 0,
            port     = _port,
            timeout  = _timeout,
            stopbits = 1,
            northDirection = AreaMap.Instance.NorthDirection,
            message = points
        };
        return _pythonScript.Run($"uart_command {message.Message}").Output;
    }

    private string CollectCOMPorts() 
    {
        string output = _pythonScript.Run("collect_ports").Output;
        PortsInfoMessage ports;
        try
        {
            ports = JsonUtility.FromJson<PortsInfoMessage>(output);

            if (ports.ports.Length == 0) 
            {
                _avaliableDevicesCountLabel.text = $"  Обнаружено устройств: {0}";
                _avaliableDevicesOptions.choices.Clear();
                _avaliableDevicesOptions.index = -1;
                _port = "";
                return $"{{\n\t\"errors\": [\"COM ports scan error. No ports found...\"]\n}}";
            }
        }
        catch (System.Exception ex)
        {
            return $"{{\n\t\"errors\": [\"{string.Join(',', ex.Data.Values)}\"]\n}}";
        }
        _avaliableDevicesCountLabel.text = $"  Обнаружено устройств\t:\t{ports.ports.Length}";
        _avaliableDevicesOptions.choices.AddRange(ports.ports);
        _avaliableDevicesOptions.index = 0;
        return $"{{\n\t\"status\": [\"COM ports scan successfully complete. Found ports count {ports.ports.Length}.\"]\n}}";
    }
    public new void  Show() 
    {
        base.Show();
        _pathSendLogLabel.text += $"\n{CollectCOMPorts()}";
    }
    public SendPathForm(UIMainWindow _parent) : base(_parent, "SendPathDataToCOM")
    {
    }
}
