using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GeneralFunctions;

public interface IServiceControl
{
    void Shutdown();
    public void ShutdownWithError(Exception ex);
    public void SaveError(Exception ex);

    public void StartService();
    public void StopService();
}
