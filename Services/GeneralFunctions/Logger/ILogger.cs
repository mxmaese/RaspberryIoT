using Entities;
using Services.GeneralFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeneralFunctions.Logger;

public interface ILogger
{
    public void LogTrace(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void LogInfo(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void LogWarning(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void LogError(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void LogFatalError(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);

    public void LogWithoutWriting(string message, Logger.Log.LogTypes LogType, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void WritePendingLogs([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);

    public void CheckIfileFunctionAndUpdate(IFileFunctions fileFunctions);

    public List<Logger.Log> ReadLog([CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public List<Logger.Log> ReadLog(Logger.Log.LogTypes LogType);
}
