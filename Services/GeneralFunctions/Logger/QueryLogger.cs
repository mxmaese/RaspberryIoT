using Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Services.GeneralFunctions;
using Services.GeneralFunctions.Settings;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Services.GeneralFunctions.Logger.QueryLogger.SavedQuerys;

namespace Services.GeneralFunctions.Logger;

public class QueryLogger : DbCommandInterceptor
{
    private readonly ISettings _SettingsActions;
    private SettingsClass _Settings;
    private IFileFunctions _FileFunctions;
    private bool _isLogging;

    public QueryLogger(IFileFunctions fileFunctions, ISettings settings)
    {
        _FileFunctions = fileFunctions;
        _SettingsActions = settings;
        _Settings = _SettingsActions.ReadSettings();
        _isLogging = false; // Bandera para evitar bucles
        PendingQuerys = new List<SavedQuerys>();
    }

    public List<SavedQuerys> PendingQuerys;

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        if (!_isLogging)
        {
            _isLogging = true;  // Marcar que estamos logueando
            try
            {
#if DEBUG
                // Capturar la pila de llamadas en el momento en que se ejecuta la consulta solo en modo de depuración
                var stackTrace = new StackTrace(true);
                LogQuery(command.CommandText, stackTrace);
#else
                LogQuery(command.CommandText);
#endif
            }
            finally
            {
                _isLogging = false;  // Desmarcar después de loguear
            }
        }

        return base.ReaderExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        if (!_isLogging)
        {
            _isLogging = true;
            try
            {
                var stackTrace = new StackTrace(true);

                LogQuery(command.CommandText, stackTrace);
            }
            finally
            {
                _isLogging = false;
            }
        }

        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void LogQuery(string query, StackTrace? stackTrace = null)
    {
        _Settings = _SettingsActions.ReadSettings();
        if (_Settings.IsDebug)
        {
            // Obtener detalles del StackTrace solo si está en modo de depuración
            var frames = new List<SavedQueryFrame>();
            if (stackTrace != null)
            {
                foreach (var frame in stackTrace.GetFrames())
                {
                    var method = frame.GetMethod();
                    if (method != null)
                    {
                        var fileName = frame.GetFileName();
                        var lineNumber = frame.GetFileLineNumber();
                        var columnNumber = frame.GetFileColumnNumber();

                        if (fileName == null) continue;

                        frames.Add(new SavedQueryFrame
                        {
                            MethodName = method.Name,
                            FileName = fileName,
                            LineNumber = lineNumber,
                            ColumnNumber = columnNumber
                        });
                    }
                }
            }

            if (!frames.Select(p => p.MethodName).Contains("Synchronize_Pedidos_return_procesed_order") ||
                !frames.Select(p => p.FileName).Contains("C:\\Users\\Usuario\\Documents\\Syncro\\Servicies\\SyncroFunctions\\Pedidos.cs") ||
                !frames.Select(p => p.LineNumber).Contains(54)
                )
            {
                PendingQuerys.Add(new SavedQuerys()
                {
                    query = query,
                    StackTraceFrames = frames // Guardar los detalles de los frames
                });
            }
        }
    }
    public void WriteQuerys()
    {
#if DEBUG
        var path = "Data/SavedQuery/SavedQuery.json";
#else
        var path = "../Data/SavedQuery/SavedQuery.json";
#endif
        var AlredySaved = _FileFunctions.ReadListFromFile<SavedQuerys>(path);

        var ToSave = new List<SavedQuerys>();
        if (!AlredySaved.IsNullOrEmpty()) AlredySaved.ForEach(x => ToSave.Add(x));
        if (!PendingQuerys.IsNullOrEmpty()) PendingQuerys.ForEach(x => ToSave.Add(x));


        _FileFunctions.WriteListToFile(path, ToSave);
        // Asegurarse de que la lógica de logueo no desencadene más consultas a la base de datos
    }
    public class SavedQuerys
    {
        public string query { get; set; }
        public List<SavedQueryFrame> StackTraceFrames { get; set; }


        public class SavedQueryFrame
        {
            public string MethodName { get; set; }
            public string FileName { get; set; }
            public int LineNumber { get; set; }
            public int ColumnNumber { get; set; }
        }

    }
}
