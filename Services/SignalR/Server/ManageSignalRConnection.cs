using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Services.SensorsAndActuators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Server;

public class ManageSignalRConnection : IManageSignalRConnection
{
    public static Dictionary<string, List<string>> Connections { get; private set; } = new();

    private readonly ILogger<ManageSignalRConnection> _logger;
    private readonly IHubContext<SignalRHub> _hubContext;
    private ISensors _sensors;
    private IActuators _actuators;

    public ManageSignalRConnection(ILogger<ManageSignalRConnection> logger, IHubContext<SignalRHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public void AddConnection(string connectionId, string token)
    {
        if (string.IsNullOrEmpty(connectionId) || string.IsNullOrEmpty(token))
        {
            _logger.LogError("ConnectionId or token is null or empty.");
            return;
        }

        if (Connections.ContainsKey(connectionId))
        {
            _logger.LogWarning($"ConnectionId {connectionId} already exists. Adding token {token}.");
            Connections[connectionId].Add(token);
        }
        else
        {
            Connections.Add(connectionId, new List<string>() { token });
            _logger.LogInformation($"Added new connection: {connectionId} with token {token}.");
        }
    }

    public void UpdateActuatorValue(string token, object value)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("token is null or empty.");
            return;
        }

        if (value == null)
        {
            _logger.LogError("Value is null.");
            return;
        }

        var ConnectionId = Connections.Any(c => c.Value.Contains(token)) ? Connections.FirstOrDefault(c => c.Value.Contains(token)).Key : null;
        if (string.IsNullOrEmpty(ConnectionId))
        {
            _logger.LogError($"The connectionId: {ConnectionId} it doesn't exist");
            return;
        }

        _hubContext.Clients.Client(ConnectionId).SendAsync("UpdateValue", token, value).Wait();
        _logger.LogInformation($"Updated actuator value for token {token} with value {value}.");
    }

    public void RemoveConnection(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            _logger.LogError("ConnectionId is null or empty.");
            return;
        }

        if (Connections.Remove(connectionId))
        {
            _logger.LogInformation($"Removed connection: {connectionId}.");
        }
        else
        {
            _logger.LogWarning($"ConnectionId {connectionId} not found.");
        }
    }

    public void UpdateSensorValue(string token, object value)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("token is null or empty.");
            return;
        }

        if (value == null)
        {
            _logger.LogError("Value is null.");
            return;
        }

        _sensors.SaveInformation(token, value);
        _logger.LogInformation($"Updated sensor value for token {token} with value {value}.");
    }

    public void GetISensor(ISensors sensors)
    {
        _sensors = sensors ?? throw new ArgumentNullException(nameof(sensors), "ISensors cannot be null.");
    }
    public void GetIActuator(IActuators sensors)
    {
        _actuators = sensors ?? throw new ArgumentNullException(nameof(sensors), "IActuators cannot be null.");
    }
}

public interface IManageSignalRConnection
{
    void AddConnection(string connectionId, string token);
    void RemoveConnection(string connectionId);

    void UpdateSensorValue(string token, object value);
    void UpdateActuatorValue(string token, object value);

    public void GetISensor(ISensors sensors);
    public void GetIActuator(IActuators sensors);
}