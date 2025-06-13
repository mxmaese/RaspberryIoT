using Entities;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Services.Databases;

namespace Services.Events;

public class EventTimer : IHostedService
{
    private readonly IDatabasesActions _database;
    private readonly IEvents _events;
    private Timer? _timer;

    public EventTimer(IDatabasesActions database, IEvents events)
    {
        _database = database;
        _events = events;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(Work, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }

    private void Work(object? state)
    {
        var events = _database.GetEvents();
        foreach (var evt in events)
        {
            if (evt.TriggerType == Event.EventTriggerType.Timer || evt.TriggerType == Event.EventTriggerType.Both)
            {
                if (evt.Interval.HasValue)
                {
                    if (evt.LastExecution == null || DateTime.UtcNow - evt.LastExecution.Value >= evt.Interval.Value)
                    {
                        _events.Execute(evt);
                    }
                }
                else if (evt.DailyTime != null)
                {
                    var now = TimeOnly.FromDateTime(DateTime.UtcNow);
                    if (evt.LastExecution == null || (evt.LastExecution.GetValueOrDefault().Day != DateTime.Now.Day && DateTime.Now.TimeOfDay < evt.DailyTime))
                    {
                        _events.Execute(evt);
                    }
                }
            }
        }
    }
}
