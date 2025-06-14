using Entities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;


namespace Services.GeneralFunctions;

public class FileFunctions : IFileFunctions
{
    public ILogger _Logger { get; set; }

    public FileFunctions(ILogger<FileFunctions> logger)
    {
        _Logger = logger;
    }

    public List<T> ReadListFromFile<T>(string Path)
    {
        return ReadFromFile<List<T>>(Path);
    }
    public T ReadFromFile<T>(string path)
    {

        var directoryPath = GetPath(path);
        CheckAndCreate(directoryPath);
        
        string jsonFromFile = File.ReadAllText(directoryPath);
        if (!string.IsNullOrEmpty(jsonFromFile))
        {
            return JsonConvert.DeserializeObject<T>(jsonFromFile);
        }
        return default;
    }

    public void WriteListToFile<T>(string Path, List<T> items, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        WriteToFile(Path, items, filePath, memberName, lineNumber);
    }

    /*
    public void WriteToFile<T>(string Path, T item, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        // Obtener la ruta del directorio

        var directoryPath = GetPath(Path);

        CheckAndCreate(directoryPath);
        try
        {
            string Data = "";
            if (typeof(T) == typeof(string))
            {
                Data = item.ToString();
            }
            else if (typeof(T) == typeof(List<string>))
            {
                // Si es una lista de strings, se unen los elementos con un salto de línea
                var stringList = item as List<string>;
                Data = string.Join(Environment.NewLine, stringList);
            }
            else
            {
                Data = JsonConvert.SerializeObject(item, Formatting.Indented);
                Data = Data.Replace("\n", Environment.NewLine);
            }
            File.WriteAllText(directoryPath, Data);
        }
        catch (Exception ex)
        {
            try
            {
                _Logger?.LogWithoutWriting($"El archivo no pudo ser escrito: {ex.Message}", Logger.Logger.Log.LogTypes.Error);

                _ServiceControl.ShutdownWithError(ex);
            }
            catch (Exception ex2)
            {
                _ServiceControl.ShutdownWithError(ex2);
            }
        }
    }
    */
    public void WriteToFile<T>(string PathName, T item, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        // Obtener la ruta del directorio
        var directoryPath = GetPath(PathName);

        try
        {
            string baseFilePath = PathName;
            int part = 1;
            long maxFileSize = 10 * 1024 * 1024; // 10 MB

            while (new FileInfo(directoryPath).Length > maxFileSize)
            {
                part++;
                var name = Path.GetFileNameWithoutExtension(baseFilePath);
                var extension = Path.GetExtension(baseFilePath);
                directoryPath = GetPath($"{name}_part{part}{extension}");
                CheckAndCreate(directoryPath);
            }

            using (StreamWriter writer = new StreamWriter(directoryPath, false))
            {
                if (typeof(T) == typeof(string))
                {
                    writer.Write(item.ToString());
                }
                else if (typeof(T) == typeof(List<string>))
                {
                    var stringList = item as List<string>;
                    foreach (var line in stringList)
                    {
                        writer.WriteLine(line);
                    }
                }
                else
                {
                    string jsonData = JsonConvert.SerializeObject(item, Formatting.Indented);
                    using (StringReader reader = new StringReader(jsonData))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _Logger?.LogError($"El archivo no pudo ser escrito: {ex.Message}");
        }
    }


    private void CheckAndCreate(string path)
    {
        if (!File.Exists(path))
        {
            var DirectoryPath = Path.GetFullPath(Path.Combine(path, "../"));
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
                _Logger?.LogInformation($"Directorio creado: {path}");
            }
            using (FileStream fs = File.Create(path))
            {
                _Logger?.LogInformation($"Archivo creado: {path}");
            }
            DirectoryPath = default;
        }
    }


    private string MakeValidFileName(string name)
    {
        bool FirstDoblePoint = true; // Para saber evitar el C_//, iria C://
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (FirstDoblePoint && c == ':') { FirstDoblePoint = false; continue; }
            if (c == '/' || c == '\\') continue;
            name = name.Replace(c, '_');
        }
        return name;
    }

    private string GetPath(string BasePath)
    {
#if DEBUG
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        while (currentDirectory != null && !Directory.EnumerateFiles(currentDirectory, "*.sln").Any())
        {
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }
        string? solutionPath = currentDirectory;
        if (solutionPath == null)
        {
            _Logger.LogError($"No se ha podido obtener el directorio de la solucion {AppDomain.CurrentDomain.BaseDirectory}");
            throw new Exception($"No se ha podido obtener el directorio de la solucion {AppDomain.CurrentDomain.BaseDirectory}");
        }
        string directoryPath = Path.GetFullPath(Path.Combine(solutionPath, BasePath));
#else
        string directoryPath = Path.GetFullPath(BasePath);
#endif
        return MakeValidFileName(directoryPath);
    }
}



public interface IFileFunctions
{
    public ILogger _Logger { get; set; }
    //    public IServiceControl _ServiceControl { get; set; }


    public List<T> ReadListFromFile<T>(string filePath);
    public T ReadFromFile<T>(string filePath);

    public void WriteListToFile<T>(string Path, List<T> items, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    public void WriteToFile<T>(string Path, T item, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
}
