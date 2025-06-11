using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Services.GeneralFunctions.Logger;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using Entities.SavedClass;

namespace Services.GeneralFunctions;

public class ServiceControl : ControllerBase, IServiceControl
{

    private readonly IHostApplicationLifetime _lifetime;
    private IFileFunctions _FileFunctions;

    public ServiceControl(IHostApplicationLifetime lifetime, IFileFunctions fileFunctions)
    {
        _lifetime = lifetime;
        _FileFunctions = fileFunctions;
    }


    public void ShutdownWithError(Exception ex)
    {
        // Detiene la aplicación

        SaveError(ex);

        Shutdown();
    }
    public void SaveError(Exception ex)
    {
        #if DEBUG
            var path = "Data/Error/errors.txt";
#else
            var path = "../Data/Error/errors.txt";
#endif

        var AlredySaved = _FileFunctions.ReadListFromFile<SavedErrors>(path);
        var ToSave = new SavedErrors();
        ToSave = ex != null ? SavedErrors.ConvertExeption(ex) : SavedErrors.ConvertExeption(new Exception($"La exception es null en SaveError"));
        try
        {
            if (AlredySaved != null)
            {
                if (AlredySaved.Any(L => L.Message == ToSave.Message && L.innerException == ToSave.innerException && L.Source == ToSave.Source && L.StackTraceString == ToSave.StackTraceString))
                {
                    AlredySaved.Where(L => L.Message == ToSave.Message && L.innerException == ToSave.innerException && L.Source == ToSave.Source && L.StackTraceString == ToSave.StackTraceString).ToList().ForEach(L => L.Time = DateTime.Now);
                }
                else
                {
                    AlredySaved.Add(ToSave);
                }
                //            ToSave.AddRange(AlredySaved);
            }
        }
        catch (Exception e)
        {
            AlredySaved.Add(ToSave);
            AlredySaved.Add(SavedErrors.ConvertExeption(e));
        }

        _FileFunctions.WriteListToFile(path, AlredySaved.DefaultIfEmpty() == default ? new List<SavedErrors>(): AlredySaved);
#if DEBUG
            throw ex;
#endif
    }
    public void Shutdown()
    {
        // Detiene la aplicación
        StopService();
        _lifetime.StopApplication();
    }
    public void StopService()
    {
    }
    public void StartService()
    {
    }
}
