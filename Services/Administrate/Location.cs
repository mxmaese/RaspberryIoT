using Entities;
using Services.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Administrate;

public class Location : ILocation
{
    private readonly IDatabasesActions _DatabasesActions;

    public Location(IDatabasesActions databasesActions)
    {
        _DatabasesActions = databasesActions;
    }

    public Entities.Location GetLocation(int LocationId)
    {
        return _DatabasesActions.GetLocation(LocationId);
    }
    public void CreateLocation(Entities.Location location)
    {
        _DatabasesActions.CreateLocation(location);
    }
    public void UpdateLocation(Entities.Location location)
    {
        _DatabasesActions.UpdateLocation(location);
    }
    public void DeleteLocation(int LocationId)
    {
        _DatabasesActions.DeleteLocation(LocationId);
    }
}
public interface ILocation
{
    Entities.Location GetLocation(int LocationId);
    void CreateLocation(Entities.Location location);
    void UpdateLocation(Entities.Location location);
    void DeleteLocation(int LocationId);
}
