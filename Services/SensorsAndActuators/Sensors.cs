using Antlr.Runtime;
using Data;
using Entities;
using Services.Databases;
using Services.GeneralFunctions;
using Services.GeneralFunctions.Settings;
using Services.Traductions;
using Services.Variables;
using System.Runtime.InteropServices;
using static Services.SensorsAndActuators.Actuators;

namespace Services.SensorsAndActuators;

public class Sensors : ISensors
{
    private readonly ISettings _SettingsActions;
    private readonly IDatabasesActions _databasesActions;
    private readonly IGeneralVariables _GeneralVariables;
    private readonly ITraductionManager _traductionManager;

    private SettingsClass _Settings;

    public Sensors(ISettings settings, IDatabasesActions databasesActions, IGeneralVariables generalVariables, ITraductionManager traductionManager)
    {
        _SettingsActions = settings;
        _databasesActions = databasesActions;
        _GeneralVariables = generalVariables;
        _traductionManager = traductionManager;
    }


    public void SaveInformation(string token, object value)
    {
        _Settings = _SettingsActions.ReadSettings();


        var sensor = GetSensorByToken(token);
        if (sensor == null)
        {
            throw new Exception("Sensor not found");
        }
        if (sensor.AssignedVariableId != 0)
        {
            _GeneralVariables.UpdateVariableValue(sensor.AssignedVariableId, value);
        }

        _databasesActions.AddDeviceHistory(sensor.DeviceReference, value.ToString());
    }

    public List<SensorHistory> GetSensorHistory(string token)
    {
        var sensor = GetSensorByToken(token);
        if (sensor == null)
        {
            throw new Exception("Sensor not found");
        }

        return _databasesActions.GetDeviceHistory(new SensorHistory() { DeviceReference = sensor.Token }).ToList();
    }
    public SensorHistory? GetLastSensorHistory(int SensorId)
    {
        var sensor = GetSensor(SensorId);
        if (sensor == null)
        {
            throw new Exception("Sensor not found");
        }

        return _databasesActions.GetDeviceHistory(new SensorHistory() { DeviceReference = sensor.Token }).OrderByDescending(history => history.Timestamp).FirstOrDefault();
    }

    public Sensor? GetSensor(int SensorId)
    {
        return _databasesActions.GetDevice(Sensor.GetNull(sensorId: SensorId)).FirstOrDefault();
    }
    public Sensor? GetSensorByToken(string token)
    {
        return _databasesActions.GetDevice(Sensor.GetNull(token: token )).FirstOrDefault();
    }
    public Sensor? GetSensorByReference(string deviceReference)
    {
        return _databasesActions.GetDevice(Sensor.GetNull(deviceReference: deviceReference )).FirstOrDefault();
    }

    public async Task<List<(EditSensorErrorMessages Error, string message)>> EditSensor(Sensor OriginalSensor)
    {
        var Response = CheckEditSensor(OriginalSensor);
        if (Response != default)
        {
            var output = new List<(EditSensorErrorMessages Error, string message)>();
            foreach (var item in Response)
            {
                var message = _traductionManager.GetTraduction(EditActuatorErrorToTraductionReference[item]);
                output.Add((item, message));
            }
            return output;
        }

        // Desvinculá cualquier instancia previa y traé una nueva no rastreada
        var sensor = _databasesActions.GetDevice(Sensor.GetNull(sensorId: OriginalSensor.SensorId)).
            AsQueryable().FirstOrDefault();
        if (sensor == null) throw new Exception("Actuator not found");

        // Crear una nueva entidad con los valores actualizados
        sensor.Name = OriginalSensor.Name;
        sensor.AssignedVariableId = OriginalSensor.AssignedVariableId;

        // Reattach and update
        _databasesActions.UpdateDevice(sensor);

        return default;
    }
    public string? UpdateSensorToken(int sensorId)
    {
        return _databasesActions.ReloadSensorToken(sensorId, true);
    }
    public List<EditSensorErrorMessages>? CheckEditSensor(Sensor RegisterSensor)
    {
        var output = new List<EditSensorErrorMessages>();
        if (RegisterSensor.Name.IsNullOrEmpty()) output.Add(EditSensorErrorMessages.NotInsertedName);
        if (RegisterSensor.AssignedVariableId == default) output.Add(EditSensorErrorMessages.NotInsertedAssignedVariable);

        return output.IsNullOrEmpty() ? default : output;
    }

    public enum EditSensorErrorMessages
    {
        NotInsertedAssignedVariable,
        NotInsertedName
    }
    private Dictionary<EditSensorErrorMessages, string> EditActuatorErrorToTraductionReference = new Dictionary<EditSensorErrorMessages, string>() {
        {EditSensorErrorMessages.NotInsertedAssignedVariable, "web.devices.edit.sensors.notinsertedusername" },// "NotInsertedAssignedVariable"},
        {EditSensorErrorMessages.NotInsertedName, "web.devices.edit.sensors.notinsertedname" },//"NotInsertedName" }, 
    };
}
public interface ISensors
{
    public void SaveInformation(string SensorReference, object value);
    public List<SensorHistory> GetSensorHistory(string SensorReference);
    public Sensor? GetSensor(int SensorId);
    public Sensor? GetSensorByReference(string deviceReference);
    public Sensor? GetSensorByToken(string token);
    public SensorHistory? GetLastSensorHistory(int SensorReference);
    public string? UpdateSensorToken(int sensorId);

    Task<List<(Sensors.EditSensorErrorMessages Error, string message)>> EditSensor(Sensor OriginalSensor);
}