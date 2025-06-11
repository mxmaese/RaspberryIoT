using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Client;

public class ClientSignalRRequestManager : IClientSignalRRequestManager
{
    public static Dictionary<string, string> TokensPins { get; private set; } = new Dictionary<string, string>();

    //recived from server
    public void ReciveActuatorValue(string token, object value)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        // Logic to handle the received actuator value
        Console.WriteLine($"Received actuator value: {value} for token: {token}");
    }

    //sended to server
    public void SendSensorValue(string token, object value)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        ClientSignalRConnetionManager.HubConnection.SendAsync("UpdateSensorValue", token, value).Wait();
    }
    public void AddTokenPin(string token, string pin)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(pin))
        {
            throw new ArgumentException("Token or pin cannot be null or empty.");
        }

        if (!TokensPins.ContainsKey(token))
        {
            TokensPins.Add(token, pin);
            ClientSignalRConnetionManager.HubConnection.SendAsync("SetUpCredentials", token).Wait();
        }
    }

}
public interface IClientSignalRRequestManager
{
    public void ReciveActuatorValue(string token, object value);
    public void SendSensorValue(string token, object value);
    public void AddTokenPin(string token, string pin);
}
