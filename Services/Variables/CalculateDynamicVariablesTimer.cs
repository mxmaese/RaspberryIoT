using Entities;
using Microsoft.Extensions.Hosting;
using Services.GeneralFunctions.Settings;

namespace Services.Variables;

public class CalculateDynamicVariablesTimer : IHostedService
{
    private readonly ICalculateDynamicVariables _CalculateDynamicVariables;
    private readonly ISettings _SettingsActions;
    private SettingsClass _Settings;
    private Timer _Timer;
    public CalculateDynamicVariablesTimer(ICalculateDynamicVariables calculateDynamicVariables, ISettings settings)
    {
        _CalculateDynamicVariables = calculateDynamicVariables;
        _SettingsActions = settings;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _Timer = new Timer(work, null, TimeSpan.Zero, TimeSpan.Zero);
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public void work(object state)
    {
        ReloadSettings();
        if (_Settings.CalculateDynamicVariablesInterval > 0)
        {
            _CalculateDynamicVariables.CalculateAllDynamicVariables();
            return;
        }
    }
    public void ReloadSettings()
    {
        _Settings = _SettingsActions.ReadSettings();
        if (_Settings.CalculateDynamicVariablesInterval > 0)
        {
            _Timer.Dispose();
            _Timer.Change(TimeSpan.FromMinutes(_Settings.CalculateDynamicVariablesInterval), TimeSpan.FromMinutes(_Settings.CalculateDynamicVariablesInterval));
        }
        else
        {
            _Timer.Dispose();
            _Timer.Change(TimeSpan.FromMinutes(5), TimeSpan.FromDays(30));
        }
    }
}
