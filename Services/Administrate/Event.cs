using Services.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;


namespace Services.Administrate;

public class Event : IEvent
{
    private readonly IDatabasesActions _DatabasesActions;

    public Event(IDatabasesActions databasesActions)
    {
        _DatabasesActions = databasesActions;
    }

    public Entities.Event? GetEvent(int eventId)
    {
        return _DatabasesActions.GetEvent(Entities.Event.GetNull(eventId: eventId)).FirstOrDefault();
    }
    public void CreateEvent(Entities.Event @event)
    {
        _DatabasesActions.CreateEvent(@event);
    }
    public void UpdateEvent(Entities.Event @event)
    {
        _DatabasesActions.UpdateEvent(@event);
    }
    public void DeleteEvent(int eventId)
    {
        _DatabasesActions.DeleteEvent(eventId);
    }
}
public interface IEvent
{
    Entities.Event? GetEvent(int eventId);
    void CreateEvent(Entities.Event @event);
    void UpdateEvent(Entities.Event @event);
    void DeleteEvent(int eventId);
}
