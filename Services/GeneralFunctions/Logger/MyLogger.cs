using Entities;
using Microsoft.IdentityModel.Tokens;
using Services.GeneralFunctions.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.GeneralFunctions.Logger;

public class Logger : ILogger
{
    //    private readonly IFileFunctions _FileFunctions;
    private readonly ISettings _SettingActions;
    private readonly IServiceControl _ServiceControl;

    private IFileFunctions _FileFunctions;

    private SettingsClass _Settings;


    public Logger(IFileFunctions fileFunctions, ISettings settingsActions)
    {
        _SettingActions = settingsActions;

        PendingLogs = new List<Log>();
    }

    private void UpdateSettings()
    {
        _Settings = _SettingActions.ReadSettings();
    }

    private void WriteLogBase(string message, string filePath, string memberName, int lineNumber, Log.LogTypes LogType)
    {
        LogWithoutWriting(message, LogType, filePath, memberName, lineNumber);
    }

    public void LogTrace(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteLogBase(message, filePath, memberName, lineNumber, Log.LogTypes.Trace);
    }

    public void LogInfo(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteLogBase(message, filePath, memberName, lineNumber, Log.LogTypes.Info);
    }

    public void LogWarning(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteLogBase(message, filePath, memberName, lineNumber, Log.LogTypes.Warning);
    }

    public void LogError(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteLogBase(message, filePath, memberName, lineNumber, Log.LogTypes.Error);
    }

    public void LogFatalError(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteLogBase(message, filePath, memberName, lineNumber, Log.LogTypes.FatalError);
    }

    private List<Log> PendingLogs;

    public void LogWithoutWriting(string message, Log.LogTypes LogType, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        UpdateSettings();

        Log LogToAdd = new Log(DateTime.Now, LogType, message, new Log.LogOrigin(filePath, memberName, lineNumber));
        PendingLogs.Add(LogToAdd);
    }

    public void WritePendingLogs([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        UpdateSettings();
        string? LogPath = _Settings.GetLogsRoot(SettingsClass.Log.LogTypes.Logger);

        if (PendingLogs.Count == 0) return;

        var Logs = ReadLog();
        Logs = (Logs == null || Logs.Count == 0) ? new List<Log>() : Logs;

        foreach (var PendingLog in PendingLogs)
        {
            try
            {
                if (Logs.Any(L => L.Message == PendingLog.Message && L.OriginFile == PendingLog.OriginFile && L.LogType == PendingLog.LogType))
                {
                    Logs.Where(L => L.Message == PendingLog.Message).ToList().ForEach(L => L.DateTime = DateTime.Now);
                    continue;
                }
                else
                {
                    Logs.Add(PendingLog);
                }
            }
            catch (Exception ex)
            {
                Logs.Add(PendingLog);

                StackFrame frame = GetCurrentFileName();
                Logs.Add(new Log { Message = ex.Message, DateTime = DateTime.Now, LogType = Log.LogTypes.Error, OriginFile = new Log.LogOrigin(frame.GetFileName(), frame.GetMethod()?.Name, frame.GetFileLineNumber()) });
            }
        }
        _FileFunctions.WriteListToFile(LogPath, Logs);
    }


    public List<Log> ReadLog([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        UpdateSettings();
        string? LogPath = _Settings.GetLogsRoot(SettingsClass.Log.LogTypes.Logger);
        var Logs = _FileFunctions.ReadListFromFile<Log>(LogPath);
        return Logs;
    }
    public List<Log> ReadLog(Log.LogTypes LogType)
    {
        UpdateSettings();
        string? LogPath = _Settings.GetLogsRoot(SettingsClass.Log.LogTypes.Logger);
        var Logs = _FileFunctions.ReadListFromFile<Log>(LogPath);
        var OutputLogs = Logs.Where(L => L.LogType == LogType).ToList();
        return OutputLogs;
    }
    public void CheckIfileFunctionAndUpdate(IFileFunctions fileFunctions)
    {
        if (_FileFunctions == null)
        {
            _FileFunctions = fileFunctions;
        }
    }

    public class Log
    {
        public DateTime? DateTime { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogTypes? LogType { get; set; }
        public string? Message { get; set; }
        public LogOrigin? OriginFile { get; set; }

        public Log() { }
        public Log(DateTime dateTime, LogTypes logType, string message, LogOrigin originFile)
        {
            DateTime = dateTime;
            LogType = logType;
            Message = message;
            OriginFile = originFile;
        }


        public enum LogTypes
        {
            Trace,
            Info,
            Warning,
            Error,
            FatalError
        }

        public class LogOrigin
        {
            public string? Path { get; set; }
            public string? Class { get; set; }
            public int? Line { get; set; }

            public LogOrigin() { }
            public LogOrigin(string path, string @class, int line)
            {
                Path = path;
                Class = @class;
                Line = line;
            }
        }
    }
    private StackFrame GetCurrentFileName()
    {
        // Obtener la pila de llamadas
        StackTrace stackTrace = new StackTrace(true);
        // Obtener el marco de la pila actual
        StackFrame frame = stackTrace.GetFrame(0);
        // Obtener el nombre del archivo
        return frame;
    }
}
