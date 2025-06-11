using Data;
using Entities;
using Newtonsoft.Json;
using Services.Databases;
using Services.GeneralFunctions.Settings;
using Services.SensorsAndActuators;
using Services.Variables;

namespace Api;

public class WhenStart : IHostedService
{
    private readonly ISettings _settings;
    private readonly ISensors _sensor;
    private readonly ICalculateDynamicVariables _calculateDynamicVariables;
    private readonly IGeneralVariables _GeneralVariables;
    private readonly ILogger<WhenStart> _logger;

    public WhenStart(ISettings settings, ISensors sensors, ICalculateDynamicVariables calculateDynamicVariables, ILogger<WhenStart> logger, IGeneralVariables generalVariables)
    {
        _settings = settings;
        _sensor = sensors;
        _calculateDynamicVariables = calculateDynamicVariables;
        _GeneralVariables = generalVariables;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
//        var Settings = _settings.ReadSettings();
//        Settings.ReferenceCodeLength = 8;
//        _settings.WriteSettings(Settings);

        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

