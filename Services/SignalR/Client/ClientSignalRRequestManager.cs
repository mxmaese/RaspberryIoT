using Microsoft.AspNetCore.SignalR.Client;
using Services.GeneralFunctions;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Client;

public class ClientSignalRRequestManager : IClientSignalRRequestManager
{
    public readonly IFileFunctions _fileFunctions;

    public ClientSignalRRequestManager(IFileFunctions fileFunctions)
    {
        _fileFunctions = fileFunctions;
    }

    public static Dictionary<string, string> TokensPins { get; private set; } = new Dictionary<string, string>();

    //recived from server
    public void ReciveActuatorValue(string token, object value)
    {
        Console.WriteLine($"Received actuator value: {value} for token: {token}");
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        // Logic to handle the received actuator value
        if (TokensPins.TryGetValue(token, out string pin))
        {
            int pinNumber = int.Parse(pin);
            ChangePinState(pinNumber, Convert.ToBoolean(value));
        }
        else
        {
            throw new KeyNotFoundException($"Token '{token}' not found in TokensPins dictionary.");
        }
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
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.");
        }

        if (!TokensPins.ContainsKey(token))
        {
            TokensPins.Add(token, pin);
            ClientSignalRConnetionManager.HubConnection.SendAsync("SetUpCredentials", token).Wait();
        }
    }

    public void RegistDevices()
    {
        var devices = _fileFunctions.ReadFromFile<List<ClientDTO>>("../Devices.txt");
        if (devices != null && devices.Count != 0)
        {
            foreach (var device in devices)
            {
                if (!TokensPins.ContainsKey(device.Token))
                {
                    AddTokenPin(device.Token, device.Pin);
                }
            }
        }
        else
        {

        }
    }

    public void ChangePinState(int pin, bool state)
    {
        using var controller = new GpioController(); 

        controller.OpenPin(pin, PinMode.Output);

        controller.Write(pin, state ? PinValue.High : PinValue.Low);

        controller.ClosePin(pin);
    }

    public class ClientDTO
    {
        public string Token { get; set; } = null!;
        public string? Pin { get; set; }
    }

}
public interface IClientSignalRRequestManager
{
    public void ReciveActuatorValue(string token, object value);
    public void SendSensorValue(string token, object value);
    public void AddTokenPin(string token, string pin);
    public void RegistDevices();
}
