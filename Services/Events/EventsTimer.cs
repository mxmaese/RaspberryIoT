using System;
using System.Threading;
using Entities;
using Microsoft.Extensions.Hosting;
using Services.Databases;
using Services.GeneralFunctions.Settings;

namespace Services.Events;

public class EventsTimer : IHostedService
{
    private readonly IEventExecutor _executor;
    private readonly IDatabasesActions _database;
    private readonly ISettings _settingsActions;
    private SettingsClass _settings;
    private Timer _timer;

    public EventsTimer(IEventExecutor executor, IDatabasesActions database, ISettings settings)
    {
        _executor = executor;
        _database = database;
        _settingsActions = settings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(Work, null, TimeSpan.Zero, TimeSpan.Zero);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }

    private void Work(object state)
    {
        ReloadSettings();
        if (_settings.EventsCheckInterval <= 0)
            return;

        var events = _database.GetEvents(new Event());
        foreach (var ev in events)
        {
            if (!ev.IsEnabled) continue;
            bool shouldRun = false;
            var now = DateTime.UtcNow;
            if (ev.IntervalMinutes.HasValue && ev.IntervalMinutes.Value > 0)
            {
                if (!ev.LastExecutedAt.HasValue || (now - ev.LastExecutedAt.Value).TotalMinutes >= ev.IntervalMinutes.Value)
                    shouldRun = true;
            }
            if (ev.ScheduledTime.HasValue)
            {
                var todayExecution = DateTime.UtcNow.Date + ev.ScheduledTime.Value;
                if (now >= todayExecution && (!ev.LastExecutedAt.HasValue || ev.LastExecutedAt.Value < todayExecution))
                    shouldRun = true;
            }
            if (shouldRun)
            {
                _executor.ExecuteEvent(ev);
            }
        }
    }

    private void ReloadSettings()
    {
        _settings = _settingsActions.ReadSettings();
        if (_settings.EventsCheckInterval > 0)
        {
            _timer.Change(TimeSpan.FromMinutes(_settings.EventsCheckInterval), TimeSpan.FromMinutes(_settings.EventsCheckInterval));
        }
        else
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }
}
