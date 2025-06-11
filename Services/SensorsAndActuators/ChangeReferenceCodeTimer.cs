using Entities;
using Microsoft.Extensions.Hosting;
using Services.Databases;
using Services.GeneralFunctions.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SensorsAndActuators;

public class ChangeReferenceCodeTimer : IHostedService
{
    private Timer _timer;
    private readonly IDatabasesActions _DatabasesActions;
    private readonly ISettings _SettingsActions;
    private SettingsClass _Settings;

    public ChangeReferenceCodeTimer(IDatabasesActions databasesActions, ISettings settings)
    {
        _DatabasesActions = databasesActions;
        _SettingsActions = settings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        UpdateSettings();
        _timer = new Timer(ChangeReferenceCode, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(_Settings.ReloadPendingCodeReferenceInterval));
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void ChangeReferenceCode(object state)
    {
        UpdateSettings();
        ResetTimer();
        _DatabasesActions.ReloadAllPendingToken();
    }
    void UpdateSettings()
    {
        _Settings = _SettingsActions.ReadSettings();
    }
    void ResetTimer()
    {
        _timer.Change(TimeSpan.FromMinutes(_Settings.ReloadPendingCodeReferenceInterval), TimeSpan.FromMinutes(_Settings.ReloadPendingCodeReferenceInterval));
    }
}
