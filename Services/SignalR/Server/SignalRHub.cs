using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Services.SensorsAndActuators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Server;

public class SignalRHub : Hub
{
    private readonly ILogger<SignalRHub> _logger;
    private readonly IManageSignalRConnection _manageConnections;

    public SignalRHub(ILogger<SignalRHub> logger, IManageSignalRConnection manageConnections)
    {
        _logger = logger;
        _manageConnections = manageConnections;
    }
    public override Task OnConnectedAsync()
    {
        // You can add logic here when a client connects
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // You can add logic here when a client disconnects
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        _manageConnections.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SetUpCredentials(string token)
    {
        _manageConnections.AddConnection(Context.ConnectionId, token);
    }

    public async Task UpdateSensorValue(string token, object value)
    {
        if (value == null)
        {
            _logger.LogError("Value is null.");
            return;
        }
        _manageConnections.UpdateSensorValue(token, value);
    }
}
