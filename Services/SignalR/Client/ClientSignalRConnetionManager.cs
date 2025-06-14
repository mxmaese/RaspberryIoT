using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Client;

public class ClientSignalRConnetionManager : IClientSignalRConnetionManager
{
    private readonly IClientSignalRRequestManager _requestManager;
    private readonly ILogger<ClientSignalRConnetionManager> _logger;

    public static HubConnection HubConnection { get; private set; }

    public ClientSignalRConnetionManager(IClientSignalRRequestManager requestManager, ILogger<ClientSignalRConnetionManager> logger)
    {
        _requestManager = requestManager;
        _logger = logger;
    }

    public HubConnection CreateConnection(string url)
    {
        try
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://31.97.163.24/Hub", options =>
                //.WithUrl("https://192.168.0.111:5000/Hub", options =>
                {
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        }
                        return message;
                    };
                }).Build();

            connection = ConfigurateConnection(connection);
            try
            {
                connection.StartAsync().Wait();
                _logger.LogInformation("SignalR connection created successfully.");
                HubConnection = connection;
                _requestManager.RegistDevices();
                return connection;
            } catch(Exception ex)
            {
                if (ex.Message.Contains("One or more errors occurred. (Se produjo un error durante el intento de conexión ya que la parte conectada no respondió adecuadamente tras un periodo de tiempo, o bien se produjo un error en la conexión establecida ya que el host conectado no ha podido responder."))
                {
                    _logger.LogError("Couldn't connect to the sever");
                }
                else
                {
                    throw;
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SignalR connection.");
            throw;
        }
    }

    public HubConnection ConfigurateConnection(HubConnection connection)
    {
        try
        {
            connection.On<string, object>("UpdateValue", (token, message) =>
            {
                Console.WriteLine($"Received actuator value: {message} for token: {token}");

                _requestManager.ReciveActuatorValue(token, message);
            });

            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring SignalR connection.");
            throw;
        }
    }
}

public interface IClientSignalRConnetionManager
{
    public HubConnection CreateConnection(string url);
    public HubConnection ConfigurateConnection(HubConnection connection);
}