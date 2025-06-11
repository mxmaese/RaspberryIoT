using Services.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Administrate;

public class Variable : IVariable
{
    private readonly IDatabasesActions _DatabasesActions;

    public Variable(IDatabasesActions databasesActions)
    {
        _DatabasesActions = databasesActions;
    }

    public List<Entities.Variable> GetVariable(int VariableId)
    {
        return _DatabasesActions.GetVariable(Entities.Variable.GetNull(variableId: VariableId));
    }
    public void CreateVariable(Entities.Variable variable)
    {
        _DatabasesActions.CreateVariable(variable);
    }
    public void UpdateVariable(Entities.Variable variable)
    {
        _DatabasesActions.UpdateVariable(variable);
    }
    public void DeleteVariable(int VariableId)
    {
        _DatabasesActions.DeleteVariable(VariableId);
    }
}
public interface IVariable
{
    List<Entities.Variable> GetVariable(int VariableId);
    void CreateVariable(Entities.Variable variable);
    void UpdateVariable(Entities.Variable variable);
    void DeleteVariable(int VariableId);
}
