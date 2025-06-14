using Entities;
using NCalc;
using Services.Databases;
using Services.Variables;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.RegularExpressions;
using Services.GeneralFunctions;
using static Services.Variables.GeneralVariables;
using Services.Traductions;

namespace Services.Events;

public class Events : IEvents
{
    private readonly IDatabasesActions _databaseActions;
    private readonly IGeneralVariables _variables;
    private readonly ILogger<Events> _logger;
    private readonly ITraductionManager _traductionManager;

    public Events(IDatabasesActions database, IGeneralVariables variables, ILogger<Events> logger, ITraductionManager traductionManager)
    {
        _databaseActions = database;
        _variables = variables;
        _logger = logger;
        _traductionManager = traductionManager;
    }

    public Event? GetEvent(int eventId)
    {
        return _databaseActions.GetEvent(Event.GetNull(eventId: eventId)).FirstOrDefault();
    }
    public List<Event>? GetEventsByUser(int userId)
    {
        return _databaseActions.GetEvent(Event.GetNull(ownerId: userId));
    }

    public User GetOwner(int eventId)
    {
        var evt = _databaseActions.GetEvent(Event.GetNull(eventId: eventId)).FirstOrDefault();
        if (evt == null) return null;
        return _databaseActions.GetUser(User.GetNull(userId: evt.OwnerId));
    }

    public void TriggerEvent(int eventId)
    {
        var evt = _databaseActions.GetEvent(Event.GetNull(eventId: eventId)).FirstOrDefault();
        if (evt != null)
        {
            Execute(evt);
        }
    }

    public void Execute(Event evt)
    {
        if (string.IsNullOrWhiteSpace(evt.Actions)) return;

        var actions = evt.Actions.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var action in actions)
        {
            var trimmed = action.Trim();

            if (trimmed.StartsWith("toggle", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(trimmed, @"toggle\s*\[v(\d+)\]", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int vid = int.Parse(match.Groups[1].Value);
                    var current = _variables.GetVariableState(vid);
                    if (current is bool b)
                    {
                        _variables.UpdateVariableValue(vid, !b);
                    }
                }
                continue;
            }

            var parts = trimmed.Split('=', 2);
            if (parts.Length != 2) continue;

            var targetMatch = Regex.Match(parts[0], @"\[v(\d+)\]");
            if (!targetMatch.Success) continue;

            int targetId = int.Parse(targetMatch.Groups[1].Value);
            var expr = new Expression(parts[1]);
            foreach (var param in ExtractParameterNames(parts[1]))
            {
                int varId = int.Parse(param.Substring(1));
                expr.Parameters[param] = _variables.GetVariableState(varId);
            }
            try
            {
                var result = expr.Evaluate();
                _variables.UpdateVariableValue(targetId, result!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action {Action}", trimmed);
            }
        }
        evt.LastExecution = DateTime.UtcNow;
        _databaseActions.UpdateEvent(evt);
    }

    private static List<string> ExtractParameterNames(string formula)
    {
        var matches = Regex.Matches(formula, @"\[(v)[0-9]+\]");
        var names = new List<string>();
        foreach (Match m in matches)
        {
            names.Add(m.Value.Trim('[', ']'));
        }
        return names.Distinct().ToList();
    }

    public async Task<List<(EditEventErrorMessages Error, string message)>> EditVariable(Event OriginalEvent)
    {
        var Response = CheckEditEvent(OriginalEvent);
        if (Response != default)
        {
            var output = new List<(EditEventErrorMessages Error, string message)>();
            foreach (var item in Response)
            {
                var message = _traductionManager.GetTraduction(EditEventErrorToTraductionReference[item]);
                output.Add((item, message));
            }
            return output;
        }

        // Desvinculá cualquier instancia previa y traé una nueva no rastreada
        var Event = _databaseActions.GetEvent(Entities.Event.GetNull(eventId: OriginalEvent.EventId)).
            AsQueryable().FirstOrDefault();
        if (Event == null)
        {
            _logger.LogWarning($"Event with ID {OriginalEvent.EventId} not found for editing.");
            return null;
        }

        // Crear una nueva entidad con los valores actualizados
        Event.Name = OriginalEvent.Name;
        Event.Actions = OriginalEvent.Actions;
        Event.TriggerType = OriginalEvent.TriggerType;
        Event.Interval = OriginalEvent.Interval ?? Event.Interval;
        Event.DailyTime = OriginalEvent.DailyTime ?? Event.DailyTime;

        // Reattach and update
        _databaseActions.UpdateEvent(Event);

        return default;
    }
    public List<EditEventErrorMessages>? CheckEditEvent(Event RegisterSensor)
    {
        var output = new List<EditEventErrorMessages>();
        if (RegisterSensor.Name.IsNullOrEmpty()) output.Add(EditEventErrorMessages.NotInsertedName);
        if (RegisterSensor.Actions.IsNullOrEmpty()) output.Add(EditEventErrorMessages.NotInsertedAction);

        return output.IsNullOrEmpty() ? default : output;
    }


    public enum EditEventErrorMessages
    {
        NotInsertedName,
        NotInsertedAction,
        NotValidAction,
    }
    private Dictionary<EditEventErrorMessages, string> EditEventErrorToTraductionReference = new Dictionary<EditEventErrorMessages, string>() {
        {EditEventErrorMessages.NotInsertedName, "web.devices.edit.event.notinsertedname" },// "NotInsertedAssignedVariable"},
        {EditEventErrorMessages.NotInsertedAction, "web.devices.edit.event.notinsertedaction" },// "NotInsertedAssignedVariable"},
        {EditEventErrorMessages.NotValidAction, "web.devices.edit.event.notvalidaction" },// "NotInsertedAssignedVariable"},
    };
}

public interface IEvents
{
    Event? GetEvent(int eventId);
    List<Event>? GetEventsByUser(int userId);
    User GetOwner(int eventId);
    void TriggerEvent(int eventId);
    void Execute(Event evt);
    Task<List<(Events.EditEventErrorMessages Error, string message)>> EditVariable(Event OriginalEvent);
}
