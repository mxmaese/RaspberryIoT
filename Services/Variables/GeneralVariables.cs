using Data;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Databases;
using Services.GeneralFunctions;
using Services.GeneralFunctions.Settings;
using Services.SensorsAndActuators;
using Services.SignalR.Server;
using Services.Traductions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Variables;

public class GeneralVariables : IGeneralVariables
{
    private IDatabasesActions _databasesActions;
    private IActuators _actuatorsActions;
    private readonly ISettings _SettingsActions;
    private readonly IManageSignalRConnection _manageSignalRConnection;
    private readonly ITraductionManager _traductionManager;
    private readonly ILogger<GeneralVariables> _logger;

    public static event Action<int>? OnVariableCalculated;

    public GeneralVariables(ISettings settings, IManageSignalRConnection manageSignalRConnection, IDatabasesActions databasesActions, ITraductionManager traductionManager, ILogger<GeneralVariables> logger)
    {
        _SettingsActions = settings;
        _manageSignalRConnection = manageSignalRConnection;
        _traductionManager = traductionManager;
        _databasesActions = databasesActions;
        _logger = logger;
    }
    
    public Variable? GetVaraible(int VariableId)
    {
        var variable = _databasesActions.GetVariable(Variable.GetNull(variableId: VariableId )).FirstOrDefault();
        return variable;
    }
    public List<Variable> GetVariableByUser(int UserId)
    {
        return _databasesActions.GetVariable(Variable.GetNull(ownerId: UserId));
    }
    public Variable? GetVariableByName(string VariableName)
    {
        return _databasesActions.GetVariable(Variable.GetNull(name: VariableName )).FirstOrDefault();
    }

    public List<Variable> GetDynamicVariables()
    {
        return _databasesActions.GetVariable(Variable.GetNull(isDynamic: true));
    }

    public object GetVariableState(int VariableId)
    {
        var variable = GetVaraible(VariableId);
        if (variable == null)
        {
            throw new Exception("Variable not found");
        }
        switch (variable.Type)
        {
            case Variable.VariableType.Int:
                return Convert.ToInt32(variable.Value, CultureInfo.InvariantCulture);
            case Variable.VariableType.Float:
                return float.Parse(variable.Value, CultureInfo.InvariantCulture);
            case Variable.VariableType.Bool:
                return Convert.ToBoolean(variable.Value);
            case Variable.VariableType.String:
                return variable.Value;
            default:
                throw new Exception("Variable type not found");
        }
    }
    private static string ToInvariantString(object value)
    {
        // Números y IFormattable → usa ‘.’ como separador
        if (value is IFormattable f)
            return f.ToString(null, CultureInfo.InvariantCulture);

        // Cadenas: si viene "25,5" la convertimos a 25.5
        if (value is string s && s.Contains(','))
            return s.Replace(',', '.');

        return value.ToString() ?? string.Empty;
    }

    public void UpdateVariableValue(int VariableId, object value)
    {
        var variable = GetVaraible(VariableId);
        if (variable == null)
        {
            throw new Exception("Variable not found");
        }

        // Update the variable value
        variable.Value = ToInvariantString(value);

        _databasesActions.UpdateVariable(variable);


        OnVariableCalculated?.Invoke(VariableId);

        var actuatorsAffected = _actuatorsActions.GetActuatorByVariable(VariableId);

        if (actuatorsAffected != null && actuatorsAffected.Count > 0)
        {
            foreach (var actuator in actuatorsAffected)
            {
                if (!ManageSignalRConnection.Connections.Any(c => c.Value.Contains(actuator.Token))) { continue; }
                _manageSignalRConnection.UpdateActuatorValue(actuator.Token, value);
            }
        }
    }

    public void GetIActuator(IActuators actuators)
    {
        _actuatorsActions = actuators;
    }
    public async Task<List<(EditVariableErrorMessages Error, string message)>> EditVariable(Variable OriginalVariable)
    {
        var Response = CheckEditVariable(OriginalVariable);
        if (Response != default)
        {
            var output = new List<(EditVariableErrorMessages Error, string message)>();
            foreach (var item in Response)
            {
                var message = _traductionManager.GetTraduction(EditActuatorErrorToTraductionReference[item]);
                output.Add((item, message));
            }
            return output;
        }

        // Desvinculá cualquier instancia previa y traé una nueva no rastreada
        var variable = _databasesActions.GetVariable(Variable.GetNull(variableId: OriginalVariable.VariableId)).
            AsQueryable().FirstOrDefault();
        if (variable == null) 
        {
            _logger.LogWarning($"Variable with ID {OriginalVariable.VariableId} not found for editing.");
            return null;
        }
        bool hasToChangeValue = !OriginalVariable.IsDynamic && variable.Value != OriginalVariable.Value && OriginalVariable.Value != null;
        

        // Crear una nueva entidad con los valores actualizados
        variable.Name = OriginalVariable.Name;
        variable.Formula = OriginalVariable.Formula ?? variable.Formula;
        variable.IsDynamic = OriginalVariable.IsDynamic;
        variable.Type = OriginalVariable.Type;
        
        // Reatachar y actualizar
        _databasesActions.UpdateVariable(variable);
        if (hasToChangeValue)
        {
            UpdateVariableValue(variable.VariableId, OriginalVariable.Value);
        }

        return default;
    }
    public string? UpdateSensorToken(int sensorId)
    {
        return _databasesActions.ReloadSensorToken(sensorId, true);
    }
    public List<EditVariableErrorMessages>? CheckEditVariable(Variable RegisterSensor)
    {
        var output = new List<EditVariableErrorMessages>();
        if (RegisterSensor.Name.IsNullOrEmpty()) output.Add(EditVariableErrorMessages.NotInsertedName);
        if (RegisterSensor.Value.IsNullOrEmpty() && !RegisterSensor.IsDynamic) output.Add(EditVariableErrorMessages.NotInsertedValue);
        if (RegisterSensor.Formula.IsNullOrEmpty() && RegisterSensor.IsDynamic) output.Add(EditVariableErrorMessages.NotInsertedFormula);

        return output.IsNullOrEmpty() ? default : output;
    }

    public enum EditVariableErrorMessages
    {
        NotInsertedName,
        NotInsertedValue,
        NotInsertedFormula,
        NotInsertedType,
    }
    private Dictionary<EditVariableErrorMessages, string> EditActuatorErrorToTraductionReference = new Dictionary<EditVariableErrorMessages, string>() {
        {EditVariableErrorMessages.NotInsertedName, "web.devices.edit.variables.notinsertedname" },// "NotInsertedAssignedVariable"},
        {EditVariableErrorMessages.NotInsertedValue, "web.devices.edit.variables.notinsertedvalue"},//"AssignedVariableAlreadyUsed" },
        {EditVariableErrorMessages.NotInsertedFormula, "web.devices.edit.variables.notinsertedformula" },//"NotInsertedName" }, 
        {EditVariableErrorMessages.NotInsertedType, "web.devices.edit.variables.notinsertedtype" },//"NotInsertedName" }, 
    };
}

public interface IGeneralVariables
{

    Variable? GetVaraible(int VariableId);
    List<Variable> GetVariableByUser(int UserId);
    Variable? GetVariableByName(string VariableName);
    List<Variable> GetDynamicVariables();
    object GetVariableState(int VariableId);
    void GetIActuator(IActuators actuators);
    void UpdateVariableValue(int VariableId, object value);

    Task<List<(GeneralVariables.EditVariableErrorMessages Error, string message)>> EditVariable(Variable OriginalVariable);
}
