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
        var events = _database.GetEvent(Event.GetNull());
        foreach (var evt in events)
        {
            if (evt.TriggerType == Event.EventTriggerType.Timer || evt.TriggerType == Event.EventTriggerType.Both)
            {
                if (evt.IntervalMinutes.HasValue)
                {
                    if (evt.LastExecution == null || (DateTime.UtcNow - evt.LastExecution.Value).TotalMinutes >= evt.IntervalMinutes.Value)
                    {
                        _events.Execute(evt);
                    }
                }
                else if (!string.IsNullOrEmpty(evt.DailyTime))
                {
                    if (TimeOnly.TryParse(evt.DailyTime, out var time))
                    {
                        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
                        if (Math.Abs((now - time).TotalMinutes) < 0.5)
                        {
                            if (evt.LastExecution == null || evt.LastExecution.Value.Date < DateTime.UtcNow.Date)
                            {
                                _events.Execute(evt);
                            }
                        }
                    }
                }
            }
        }
    }
}
