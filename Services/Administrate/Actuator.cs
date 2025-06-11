using Services.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Administrate;

public class Actuator : IActuator
{
    public IDatabasesActions _DatabasesActions;
    public Actuator(IDatabasesActions databasesActions)
    {
        _DatabasesActions = databasesActions;
    }

    public List<Entities.Actuator> GetActuator(int ActuatorId)
    {
        return _DatabasesActions.GetDevice<Entities.Actuator>(new Entities.Actuator { ActuatorId = ActuatorId });
    }
    public List<Entities.Actuator> GetActuatorByUserId(int UserId)
    {
        return _DatabasesActions.GetDevice<Entities.Actuator>(new Entities.Actuator { OwnerId = UserId });
    }
    public void CreateActuator(Entities.Actuator actuator)
    {
        _DatabasesActions.CreateDevice(actuator);
    }
    public void UpdateActuator(Entities.Actuator actuator)
    {
        _DatabasesActions.UpdateDevice(actuator);
    }
    public void DeleteActuator(int ActuatorId)
    {
        _DatabasesActions.DeleteDevice<Entities.Actuator>(ActuatorId);
    }
}
public interface IActuator
{
    List<Entities.Actuator> GetActuator(int ActuatorId);
    List<Entities.Actuator> GetActuatorByUserId(int UserId);
    void CreateActuator(Entities.Actuator actuator);
    void UpdateActuator(Entities.Actuator actuator);
    void DeleteActuator(int ActuatorId);
}