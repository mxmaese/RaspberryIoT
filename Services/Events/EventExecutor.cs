using System.Text.RegularExpressions;
using System.Linq;
using Entities;
using NCalc;
using Services.Databases;
using Services.Variables;
using Microsoft.Extensions.Logging;

namespace Services.Events;

public class EventExecutor : IEventExecutor
{
    private readonly IDatabasesActions _database;
    private readonly IGeneralVariables _variables;
    private readonly ILogger<EventExecutor> _logger;

    public EventExecutor(IDatabasesActions database, IGeneralVariables variables, ILogger<EventExecutor> logger)
    {
        _database = database;
        _variables = variables;
        _logger = logger;
    }

    public void ExecuteEvent(Event ev)
    {
        if (!ev.IsEnabled) return;

        if (!string.IsNullOrEmpty(ev.Condition))
        {
            var expression = new Expression(ev.Condition);
            foreach (var param in ExtractParameterNames(ev.Condition))
            {
                int vid = int.Parse(param.Substring(1));
                expression.Parameters[param] = _variables.GetVariableState(vid);
            }
            try
            {
                var result = expression.Evaluate();
                if (!(result is bool b ? b : Convert.ToBoolean(result)))
                    return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating event condition {Name}", ev.Name);
                return;
            }
        }

        ExecuteActions(ev.Actions);
        ev.LastExecutedAt = DateTime.UtcNow;
        _database.UpdateEvent(ev);
    }

    private void ExecuteActions(string actions)
    {
        if (string.IsNullOrWhiteSpace(actions)) return;
        foreach (var action in actions.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = action.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;
            var target = parts[0].Trim();
            var expressionString = parts[1];
            if (!target.StartsWith("[v") || !target.EndsWith("]")) continue;
            int vid = int.Parse(target.Trim('[', 'v', ']'));
            var expression = new Expression(expressionString);
            foreach (var param in ExtractParameterNames(expressionString))
            {
                int pvid = int.Parse(param.Substring(1));
                expression.Parameters[param] = _variables.GetVariableState(pvid);
            }
            try
            {
                var result = expression.Evaluate();
                _variables.UpdateVariableValue(vid, result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error executing action {Action}", action);
            }
        }
    }

    private static List<string> ExtractParameterNames(string formula)
    {
        var matches = Regex.Matches(formula, @"\[v[0-9]+\]");
        return matches.Select(m => m.Value.Trim('[', ']')).Distinct().ToList();
    }
}

public interface IEventExecutor
{
    void ExecuteEvent(Event ev);
}
