using Services.GeneralFunctions.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeneralFunctions;

public interface IFileFunctions
{
    public ILogger _Logger { get; set; }
//    public IServiceControl _ServiceControl { get; set; }


    public List<T> ReadListFromFile<T>(string filePath);
    public T ReadFromFile<T>(string filePath);

    public void WriteListToFile<T>(string Path, List<T> items, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void WriteToFile<T>(string Path, T item, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);

    public void CheckIserviceControlAndUpdate(IServiceControl seriveControl);
    public void CheckILoggerAndUpdate(ILogger logger);
}
