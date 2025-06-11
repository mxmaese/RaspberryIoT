using Entities;
using Services.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Administrate;

public class User : IUser
{
    private readonly IDatabasesActions _DatabasesActions;

    public User(IDatabasesActions databasesActions)
    {
        _DatabasesActions = databasesActions;
    }

    public Entities.User GetUser(int User)
    {
        return _DatabasesActions.GetUser(new Entities.User { UserId = User }) ?? new ();
    }
    public void CreateUser(Entities.User user)
    {
        _DatabasesActions.CreateUser(user);
    }
    public void UpdateUser(Entities.User user)
    {
        _DatabasesActions.UpdateUser(user);
    }
    public void DeleteUser(int UserId)
    {
        _DatabasesActions.DeleteUser(UserId);
    }
}
public interface IUser
{
    Entities.User GetUser(int UserId);
    void CreateUser(Entities.User user);
    void UpdateUser(Entities.User user);
    void DeleteUser(int UserId);
}
