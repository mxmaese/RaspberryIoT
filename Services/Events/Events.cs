using Entities;
using NCalc;
using Services.Databases;
using Services.Variables;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.RegularExpressions;

namespace Services.Events;

public class Events : IEvents
{
    private readonly IDatabasesActions _database;
    private readonly IGeneralVariables _variables;
    private readonly ILogger<Events> _logger;

    public Events(IDatabasesActions database, IGeneralVariables variables, ILogger<Events> logger)
    {
        _database = database;
        _variables = variables;
        _logger = logger;
    }

    public void TriggerEvent(int eventId)
    {
        var evt = _database.GetEvent(Event.GetNull(eventId: eventId)).FirstOrDefault();
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
        _database.UpdateEvent(evt);
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
}

public interface IEvents
{
    void TriggerEvent(int eventId);
    void Execute(Event evt);
}
