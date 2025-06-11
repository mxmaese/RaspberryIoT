using Antlr.Runtime;
using Data;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using Services.Databases;
using Services.GeneralFunctions;
using Services.Traductions;
using Services.Users;
using Services.Variables;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Services.Users.Users;

namespace Services.SensorsAndActuators;

public class Actuators : IActuators
{
    private readonly IGeneralVariables _generalVariables;
    private readonly IDatabasesActions _databasesActions;
    private readonly ITraductionManager _traductionManager;
    private readonly ILogger<Actuators> _logger;

    public Actuators(IGeneralVariables generalVariables, IDatabasesActions databasesActions, ITraductionManager traductionManager, ILogger<Actuators> logger)
    {
        _generalVariables = generalVariables;
        _databasesActions = databasesActions;
        _traductionManager = traductionManager;
        _logger = logger;
        _generalVariables.GetIActuator(this);
    }

    public List<Actuator>? GetActuator(int Id)
    {
        return _databasesActions.GetDevice(Actuator.GetNull(actuatorId: Id)).AsQueryable().AsNoTracking().ToList();
    }
    public List<Actuator>? GetActuator(string Reference)
    {
        return _databasesActions.GetDevice(Actuator.GetNull(token: Reference));
    }
    public List<Actuator>? GetActuatorByVariable(int variableId)
    {
        return _databasesActions.GetDevice(Actuator.GetNull(assignedVariableId: variableId));
    }

    public string? UpdateActuatorToken(int actuatorId)
    {
        return _databasesActions.ReloadActuatorToken(actuatorId, true);
    }


    public async Task<List<(EditActuatorErrorMessages Error, string message)>> EditActuator(Actuator OriginalActuator)
    {
        var Response = CheckEditActuator(OriginalActuator);
        if (Response != default)
        {
            var output = new List<(EditActuatorErrorMessages Error, string message)>();
            foreach (var item in Response)
            {
                var message = _traductionManager.GetTraduction(EditActuatorErrorToTraductionReference[item]);
                output.Add((item, message));
            }
            return output;
        }

        // Desvinculá cualquier instancia previa y traé una nueva no rastreada
        var actuator = _databasesActions.GetDevice(Actuator.GetNull(actuatorId: OriginalActuator.ActuatorId )).
            AsQueryable().AsNoTracking().FirstOrDefault();
        if (actuator == null) throw new Exception("Actuator not found");

        actuator.Name = OriginalActuator.Name;
        actuator.AssignedVariableId = OriginalActuator.AssignedVariableId;

        // Reattach and update
        _databasesActions.UpdateDevice(actuator);

        return default;
    }
    public List<EditActuatorErrorMessages>? CheckEditActuator(Actuator RegisterActuator)
    {
        var output = new List<EditActuatorErrorMessages>();
        if (RegisterActuator.Name.IsNullOrEmpty()) output.Add(EditActuatorErrorMessages.NotInsertedName);
        if (RegisterActuator.AssignedVariableId == default) output.Add(EditActuatorErrorMessages.NotInsertedAssignedVariable);
        
        bool IsVariableAvailable = _databasesActions.GetDevice(Actuator.GetNull(assignedVariableId: RegisterActuator.AssignedVariableId )).FirstOrDefault() == default;
        if (!IsVariableAvailable) output.Add(EditActuatorErrorMessages.AssignedVariableAlreadyUsed);

        return output.IsNullOrEmpty() ? default : output;
    }

    public enum EditActuatorErrorMessages
    {
        NotInsertedAssignedVariable,
        AssignedVariableAlreadyUsed,

        NotInsertedName
    }
    private Dictionary<EditActuatorErrorMessages, string> EditActuatorErrorToTraductionReference = new Dictionary<EditActuatorErrorMessages, string>() {
        {EditActuatorErrorMessages.NotInsertedAssignedVariable, "web.devices.edit.actuators.notinsertedusername" },// "NotInsertedAssignedVariable"},
        {EditActuatorErrorMessages.AssignedVariableAlreadyUsed, "web.devices.edit.actuators.assignedVariablealreadyused"},//"AssignedVariableAlreadyUsed" },
        {EditActuatorErrorMessages.NotInsertedName, "web.devices.edit.actuators.notinsertedname" },//"NotInsertedName" }, 
    };
}
public interface IActuators
{
    List<Actuator> GetActuator(int Id);
    List<Actuator> GetActuator(string Reference);
    public List<Actuator>? GetActuatorByVariable(int variableId);
    Task<List<(Actuators.EditActuatorErrorMessages Error, string message)>> EditActuator(Actuator OriginalActuator);
    string? UpdateActuatorToken(int actuatorId);
}
