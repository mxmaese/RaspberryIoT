using Entities;
using Services.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Administrate;

public class Sensor : ISensor
{
    public IDatabasesActions _DatabasesActions;
    public Sensor(IDatabasesActions databasesActions)
    {
        _DatabasesActions = databasesActions;
    }

    public List<Entities.Sensor> GetSensor(int SensorId)
    {
        return _DatabasesActions.GetDevice<Entities.Sensor>(new Entities.Sensor { SensorId = SensorId });
    }
    public List<Entities.Sensor> GetSensorByUserId(int UserId)
    {
        return _DatabasesActions.GetDevice<Entities.Sensor>(new Entities.Sensor { OwnerId = UserId });
    }
    public void CreateSensor(Entities.Sensor sensor)
    {
        _DatabasesActions.CreateDevice(sensor);
    }
    public void UpdateSensor(Entities.Sensor sensor)
    {
        _DatabasesActions.UpdateDevice(sensor);
    }
    public void DeleteSensor(int SensorId)
    {
        _DatabasesActions.DeleteDevice<Entities.Sensor>(SensorId);
    }
}
public interface ISensor
{
    List<Entities.Sensor> GetSensor(int SensorId);
    List<Entities.Sensor> GetSensorByUserId(int UserId);
    void CreateSensor(Entities.Sensor sensor);
    void UpdateSensor(Entities.Sensor sensor);
    void DeleteSensor(int SensorId);
}
