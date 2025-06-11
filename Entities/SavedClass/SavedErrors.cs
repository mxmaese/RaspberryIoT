using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.SavedClass;

public class SavedErrors
{
    public class InnerException
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string StackTraceString { get; set; }
    }
    public DateTime Time { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
    public string StackTraceString { get; set; }
    public InnerException innerException { get; set; }

    public static SavedErrors ConvertExeption(Exception ex)
    {
        var NewError = new SavedErrors()
        {
            Time = DateTime.Now,
            Source = ex.Source,
            StackTraceString = ex.StackTrace,
            Message = ex.Message,
            innerException = new InnerException()
            {
                Source = ex.InnerException?.Source,
                Message = ex.InnerException?.Message,
                StackTraceString = ex.InnerException?.StackTrace
            }
        };
        return NewError;
    }
}
public class BackendStatus
{
    public bool BackEndStatus { get; set; }
}