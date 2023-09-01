using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct StatusMessage 
{
    public string[] errors;
    public string[] status;
    public override string ToString() => $"{{\n\t\"status\": [{string.Join(',', status)}],\n\t\"errors\": [{string.Join(',', errors)}]\n}}";
}

[System.Serializable]
public struct PortsInfoMessage
{
    public string[] errors;
    public string[] ports;
    public override string ToString() => $"{{\n\t\"errors\": [{string.Join(',', errors)}],\n\t\"ports\":  [{string.Join(',', ports)}]\n}}";
}

[System.Serializable]
public struct UARTMessage
{
    private static readonly string q = "\"\"\"";
    public string port;
    public int mode;
    public int baudrate;
    public int timeout;
    public int bytesize;
    public int stopbits;
    public Vector3 northDirection;
    public Vector2[] message;
    public string Message =>
               $"{{" +
               $"{q}port{q}:{q}{port}{q}," +
               $"{q}baudrate{q}:{baudrate}," +
               $"{q}mode{q}:{mode}," +
               $"{q}timeout{q}:{timeout}," +
               $"{q}bytesize{q}:{bytesize}," +
               $"{q}stopbits{q}:{stopbits}," +
               $"{q}message{q}:{q}{string.Join('|', message.Select(f => $"{f.x.ToString().Replace(',', '.')},{f.y.ToString().Replace(',', '.')}"))}{q}" +
               $"}}";

    public override string ToString()
    {
        return $"{{\n" +
               $"\t\"port\":     \"{port}\",\n" +
               $"\t\"baudrate\": {baudrate},\n" +
               $"\t\"mode\":     {mode},\n" +
               $"\t\"timeout\":  {timeout},\n" +
               $"\t\"bytesize\": {bytesize},\n" +
               $"\t\"stopbits\": {stopbits},\n" +
               $"\t\"message\":  [{string.Join(',', message.Select(f => $"{f.x.ToString().Replace(',', '.')},{f.y.ToString().Replace(',', '.')}"))}]\n" +
               $"}}";
    }
}
public class PythonScriptsExecutor 
{
    private string _pythonCode;
    private string _pythonFile;
    private string _output;
    private string _args;
    private bool   _isCodeValid = false;
    public bool IsValid => _isCodeValid;
    public string PythonCode => _pythonCode;
    public string Output => _output;
    public string Args => _args;

    // private bool LoadCode(string codeSrc)
    // {
    //     if (!File.Exists(codeSrc)) return false;
    //     try
    //     {
    //         _pythonCode = File.ReadAllText(codeSrc);
    //     }
    //     catch (System.Exception ex)
    //     {
    //         return false;
    //     }
    //     return true || ValidateCode();
    // }
    private bool ValidateCode() 
    {
        _isCodeValid = true;
        if (_pythonCode == "") 
        {
            _isCodeValid = false;
            return _isCodeValid;
        }
        return _isCodeValid;
    }
    public PythonScriptsExecutor Run(string args)
    {
        if (!IsValid) return this;
        Process process = new Process();
        _args = args;
        process.StartInfo.FileName               = "python"; // «апускаем командную строку
        process.StartInfo.Arguments              = $" {_pythonFile} {args}";
        process.StartInfo.UseShellExecute        = false; // Ќе используем оболочку командной строки
        process.StartInfo.RedirectStandardOutput = true; // ѕеренаправл€ем вывод в стандартный поток
        process.StartInfo.CreateNoWindow         = true;
        process.Start();
        using (StreamReader reader = process.StandardOutput) _output = reader.ReadToEnd();
        process.WaitForExit();
        return this;
    }
    ~PythonScriptsExecutor() 
    {
       if(File.Exists(_pythonFile)) File.Delete(_pythonFile);
    }
    public PythonScriptsExecutor(string codeSource) 
    {
        _pythonCode = codeSource;
        if (!ValidateCode()) return;
        _pythonFile = $"C:/Windows/Temp/python_script_{_pythonCode.GetHashCode()}.py";
        using (StreamWriter writer = new StreamWriter(_pythonFile)) writer.Write(_pythonCode);
    }
}