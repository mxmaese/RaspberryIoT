using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using NCalc;
using Services.Databases;
using Services.SensorsAndActuators;

namespace Services.Variables;

public class CalculateDynamicVariables : ICalculateDynamicVariables
{
    private readonly IGeneralVariables _GeneralVariables;
    private readonly ISensors _Sensors;
    private readonly IDatabasesActions _DatabaseActions;
    private readonly ILogger<CalculateDynamicVariables> _Logger;
    

    public CalculateDynamicVariables(IGeneralVariables generalVariables, ISensors sensors, IDatabasesActions databasesActions, ILogger<CalculateDynamicVariables> logger)
    {
        _GeneralVariables = generalVariables;
        _Sensors = sensors;
        _DatabaseActions = databasesActions;
        _Logger = logger;
    }

    public void CalculateAllDynamicVariables()
    {
        var dynamicVariables = _GeneralVariables.GetDynamicVariables();
        foreach (var variable in dynamicVariables)
        {
            CalculateSingleVariable(variable.VariableId);
        }
    }


    public void CalculateSingleVariable(int VariableId)
    {
        var variable = _GeneralVariables.GetVaraible(VariableId);
        if (variable == null)
        {
            throw new Exception("Variable not found");
        }
        if (variable.IsDynamic)
        {
            var expression = new Expression(variable.Formula);

            // Obtener los nombres de los parámetros en la fórmula usando expresiones regulares
            var parameterNames = ExtractParameterNames(variable?.Formula);

            // Asignar valores a los parámetros a partir de la base de datos
            foreach (var parameterName in parameterNames)
            {
                if (parameterName.StartsWith("v"))
                {
                    // Si es una variable ([vX])
                    int variableId = int.Parse(parameterName.Substring(1));
                    var paramVariable = _GeneralVariables.GetVaraible(variableId);
                    if (paramVariable == null)
                    {
                        throw new Exception($"Variable '{parameterName}' not found in the database.");
                    }
                    var paramVariableValue = _GeneralVariables.GetVariableState(variableId);
                    expression.Parameters[parameterName] = _GeneralVariables.GetVariableState(paramVariable.VariableId);
                }else
                {
                    throw new Exception($"Unknown parameter type: '{parameterName}'");
                }
            }

            // Evaluar la expresión y asignar el valor a la variable
            try
            {
                var result = expression?.Evaluate();
                _GeneralVariables.UpdateVariableValue(VariableId, result);
            }
            catch (Exception ex)
            {
                if (ex is EvaluationException || ex.Message == "Parameter was not defined Arg_ParamName_Name" || ex.Message.Contains("Parameter was not defined"))
                {
                    _Logger.LogWarning($"Error evaluating dynamic variable formula: {variable.Formula}");
#if DEBUG
                    /*throw new Exception($"Error evaluating dynamic variable formula: {variable.Formula}");*/
#endif
                }
                else
                {
                    _Logger.LogError(ex, "Unexpected error evaluating dynamic variable formula: {Formula}", variable.Formula);
                    throw;
                }
            }
            
        }
    }

    //crea una funcion para actualizar todas las variables dinamicas, que usen una variable en especifico
    public void CalculateDynamicVariablesThatDependsOn(int variableId)
    {
        var variablesThatDepend = GetVariablesThatDepends(variableId);
        foreach (var variable in variablesThatDepend)
        {
            CalculateSingleVariable(variable.VariableId);
        }
    }

    public List<Variable> GetVariablesThatDepends(int variableId)
    {
        var DynamicVariables = _GeneralVariables.GetDynamicVariables();
        var result = new List<Variable>();

        foreach(var variable in DynamicVariables)
        {
            var expression = new Expression(variable.Formula);

            // Obtener los nombres de los parámetros en la fórmula usando expresiones regulares
            var parameterNames = ExtractParameterNames(variable?.Formula);
            foreach (var parameterName in parameterNames)
            {
                int actualVariableId = int.Parse(parameterName.Substring(1));
                if (actualVariableId != variableId) continue; // Solo considerar si la variable es la que buscamos
                result.Add(variable);
                break;
            }
        }
        return result;
    }

    // Función auxiliar para extraer los nombres de los parámetros de la fórmula
    private List<string> ExtractParameterNames(string formula)
    {
        // Utilizar una expresión regular para encontrar posibles nombres de variables en la fórmula ([vX] o [sX])
        var matches = Regex.Matches(formula, @"\[(v)[0-9]+\]");

        var parameterNames = new List<string>();

        foreach (Match match in matches)
        {
            // Quitar los corchetes
            var paramName = match.Value.Trim('[', ']');
            parameterNames.Add(paramName);

        }

        return parameterNames.Distinct().ToList();
    }
}
public interface ICalculateDynamicVariables
{
    void CalculateSingleVariable(int VariableId);
    void CalculateAllDynamicVariables();
    void CalculateDynamicVariablesThatDependsOn(int variableId);
}
