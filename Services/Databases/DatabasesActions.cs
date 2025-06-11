using Data;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using Services.GeneralFunctions.Settings;
using Services.SensorsAndActuators;
using Services.Variables;
using System.Runtime.CompilerServices;
using System.Text;

namespace Services.Databases;

public class DatabasesActions : IDatabasesActions
{
    private readonly DatabaseContext _Context;
    private readonly ISettings _SettingsActions;
    private SettingsClass _Settings;

    public DatabasesActions(DatabaseContext context, ISettings settingsActions)
    {
        _Context = context;
        _SettingsActions = settingsActions;
        ReloadSettings();
    }

    #region Helpers
    private TResult ExecuteReadWithRetry<TResult>(Func<TResult> action)
    {
        var strategy = _Context.Database.CreateExecutionStrategy();
        return strategy.Execute(action);      // ← NO BeginTransaction
    }
    private async Task ExecuteWithRetryAsync(Func<Task> action)
    {
        var strategy = _Context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _Context.Database.BeginTransactionAsync();
            await action();
            await tx.CommitAsync();
        });
    }
    private async Task<TResult> ExecuteWithRetryAsync<TResult>(Func<Task<TResult>> action)
    {
        // Estrategia de reintento propia de EF Core (maneja transientes, deadlocks, etc.)
        var strategy = _Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Si sólo LEÉS datos no hace falta transacción explícita.
            // Pero si querés mantenerla, dejalo:
            await using var tx = await _Context.Database.BeginTransactionAsync();

            var result = await action();

            // Sólo commit si iniciaste transacción
            await tx.CommitAsync();
            return result;
        });
    }
    private void ExecuteWithRetry(Action action)
    {
        var strategy = _Context.Database.CreateExecutionStrategy();
        strategy.Execute(() =>
        {
            using var tx = _Context.Database.BeginTransaction();
            action();
            tx.Commit();
        });
    }
    private TResult ExecuteWriteWithRetry<TResult>(Func<TResult> action)
    {
        // Estrategia de reintento propia de EF Core (maneja transientes, deadlocks, etc.)
        var strategy = _Context.Database.CreateExecutionStrategy();

        return strategy.Execute(() =>
        {
            // Si sólo LEÉS datos no hace falta transacción explícita.
            // Pero si querés mantenerla, dejalo:
            using var tx = _Context.Database.BeginTransaction();

            var result = action();

            // Sólo commit si iniciaste transacción
            tx.Commit();
            return result;
        });
    }
    #endregion

    //Sensors And Actuators
    #region Sensors
    public void AddDeviceHistory(string DeviceId, string Value)
    {
        _Context.SensorHistory.Add(new SensorHistory()
        {
            DeviceReference = DeviceId,
            Value = Value,
            Timestamp = DateTime.Now
        });
        _Context.SaveChanges();
    }
    public List<SensorHistory> GetDeviceHistory(SensorHistory deviceHistory)
    {
        return ExecuteReadWithRetry(() =>
        {
            return _Context.SensorHistory.AsNoTracking().Where(x => x.DeviceReference == deviceHistory.DeviceReference || x.DeviceReference == deviceHistory.DeviceReference).ToList();
        });
    }
    #endregion

    #region Devices
    public List<T> GetDevice<T>(T DeviceToSearch) where T : class
    {
        return ExecuteReadWithRetry(() =>
        {
            if (DeviceToSearch is Sensor sensor)
            {
                return _Context.Sensors.AsNoTracking().Where(x => (x.SensorId != -1 && x.SensorId == sensor.SensorId) || (x.DeviceReference != null && x.DeviceReference == sensor.DeviceReference) || (x.Token != null && x.Token == sensor.Token) || (x.OwnerId != -1 && x.OwnerId == sensor.OwnerId) || (x.AssignedVariableId != -1 && x.AssignedVariableId == sensor.AssignedVariableId)).Cast<T>().ToList();
            }
            else if (DeviceToSearch is Actuator actuator)
            {
                return _Context.Actuators.AsNoTracking().Where(x => (x.ActuatorId != -1 && x.ActuatorId == actuator.ActuatorId) || (x.DeviceReference != null && x.DeviceReference == actuator.DeviceReference) || (x.Token != null && x.Token == actuator.Token) || (x.OwnerId != -1 && x.OwnerId == actuator.OwnerId) || (x.AssignedVariableId != -1 && x.AssignedVariableId == actuator.AssignedVariableId)).Cast<T>().ToList();
            }
            else
            {
                throw new Exception("DeviceReference type not found");
            }
        });
    }
    public void CreateDevice<T>(T Device)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                if (Device is Sensor sensor)
                {
                    sensor.Token = GenerateUniqueToken();
                    sensor.CreatedAt = DateTime.Now;
                    sensor.LastReferenceChange = DateTime.Now;
                    sensor.DeviceReference = GenerateDeviceReference();
                    _Context.Sensors.Add(sensor);
                }
                else if (Device is Actuator actuator)
                {
                    actuator.Token = GenerateUniqueToken();
                    actuator.CreatedAt = DateTime.Now;
                    actuator.LastReferenceChange = DateTime.Now;
                    actuator.DeviceReference = GenerateDeviceReference();
                    _Context.Actuators.Add(actuator);
                }
                else
                {
                    throw new Exception("DeviceReference type not found");
                }

                _Context.SaveChanges();
            }
            catch
            {
                throw;
            }
        });
    }

    public void ReloadAllPendingToken()
    {
        ReloadSettings();
        ExecuteWithRetry(() =>
        {
            try
            {
                var AllPendingSensors = _Context.Sensors.Where(x => x.LastReferenceChange < DateTime.Now.AddMinutes(-_Settings.RealoadPendingCodeReferenceInterval)).ToList();
                if (AllPendingSensors.Any())
                {
                    foreach (var sensor in AllPendingSensors)
                    {
                        sensor.Token = GenerateUniqueToken();
                        sensor.LastReferenceChange = DateTime.Now;
                        _Context.Sensors.Update(sensor);
                    }
                }
                var AllPendingActuators = _Context.Actuators.Where(x => x.LastReferenceChange < DateTime.Now.AddMinutes(-_Settings.RealoadPendingCodeReferenceInterval)).ToList();
                if (AllPendingActuators.Any())
                {
                    foreach (var actuator in AllPendingActuators)
                    {
                        var ReferenceCode = GenerateUniqueToken();
                        actuator.Token = ReferenceCode;
                        actuator.LastReferenceChange = DateTime.Now;
                        _Context.Actuators.Update(actuator);
                    }
                }
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    public string? ReloadSensorToken(int sensorId, bool ignorarIntervalo = false)
    {
        ReloadSettings(); // mismo paso que en el método original
        return ExecuteWriteWithRetry(() =>
        {
            try
            {
                var sensor = _Context.Sensors.FirstOrDefault(s => s.SensorId == sensorId);
                if (sensor == null)
                    throw new InvalidOperationException($"Sensor {sensorId} no encontrado.");

                // Si se respeta el intervalo y todavía no venció, salimos sin cambios
                if (!ignorarIntervalo &&
                    sensor.LastReferenceChange >= DateTime.Now.AddMinutes(-_Settings.RealoadPendingCodeReferenceInterval))
                {
                    return null;
                }

                // Generamos y asignamos el nuevo código
                sensor.Token = GenerateUniqueToken();
                sensor.LastReferenceChange = DateTime.Now;

                _Context.Sensors.Update(sensor);
                _Context.SaveChanges();

                return sensor.Token;
            }
            catch
            {
                throw;
            }
        });
    }

    public string? ReloadActuatorToken(int actuatorId, bool ignorarIntervalo = false)
    {
        ReloadSettings(); // mismo paso que en el método original
        return ExecuteWriteWithRetry(() =>
        {
            try
            {
                var Actuator = _Context.Actuators.FirstOrDefault(s => s.ActuatorId == actuatorId);
                if (Actuator == null)
                    throw new InvalidOperationException($"Actuador {actuatorId} no encontrado.");

                // Si se respeta el intervalo y todavía no venció, salimos sin cambios
                if (!ignorarIntervalo &&
                    Actuator.LastReferenceChange >= DateTime.Now.AddMinutes(-_Settings.RealoadPendingCodeReferenceInterval))
                {
                    return null;
                }

                // Generamos y asignamos el nuevo código
                Actuator.Token = GenerateUniqueToken();
                Actuator.LastReferenceChange = DateTime.Now;

                _Context.Actuators.Update(Actuator);
                _Context.SaveChanges();

                return Actuator.Token;
            }
            catch
            {
                throw;
            }
        });
    }
    public void UpdateDevice<T>(T device) where T : class
    {
        ExecuteWithRetry(() =>
        {
            // 1. Busco en el ChangeTracker si ya hay una entidad con la MISMA clave
            var entryType = _Context.Model.FindEntityType(typeof(T));
            var pk = entryType.FindPrimaryKey().Properties;

            // Clave compuesta → comparo todas las propiedades
            var duplicate = _Context.ChangeTracker.Entries<T>()
                              .FirstOrDefault(e =>
                                  pk.All(p =>
                                  {
                                      var current = e.Property(p.Name).CurrentValue;
                                      var newVal = typeof(T).GetProperty(p.Name)?.GetValue(device);
                                      return Equals(current, newVal);
                                  }));

            if (duplicate != null)
            {
                // 2. Si la entidad ya está en memoria, solo actualizo sus valores
                duplicate.CurrentValues.SetValues(device);
            }
            else
            {
                // 3. Si NO estaba, la adjunto como modificada
                _Context.Attach(device);
                _Context.Entry(device).State = EntityState.Modified;
            }

            _Context.SaveChanges();

        });
    }
    public void DeleteDevice<T>(int DeviceId)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                if (typeof(T) == typeof(Sensor))
                {
                    var sensor = _Context.Sensors.FirstOrDefault(x => x.SensorId == DeviceId);
                    if (sensor == null)
                    {
                        throw new Exception("Sensor not found");
                    }
                    _Context.Sensors.Remove(sensor);
                }
                else if (typeof(T) == typeof(Actuator))
                {
                    var actuator = _Context.Actuators.FirstOrDefault(x => x.ActuatorId == DeviceId);
                    if (actuator == null)
                    {
                        throw new Exception("Actuator not found");
                    }
                    _Context.Actuators.Remove(actuator);
                }
                else
                {
                    throw new Exception("DeviceReference type not found");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    #endregion

    #region Variables
    public List<Variable> GetVariable(Variable variable)
    {
        return ExecuteReadWithRetry(() => {
            return _Context.Variables.AsNoTracking().Where(x => (x.VariableId != -1 && x.VariableId == variable.VariableId) || (x.IsDynamic != false && x.IsDynamic == variable.IsDynamic) || (x.Name != null && x.Name == variable.Name) || (x.OwnerId != -1 && x.OwnerId == variable.OwnerId) || (x.Type != null && x.Type == variable.Type)).ToList();
        });
    }
    public void CreateVariable(Variable variable)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                variable.CreatedAt = DateTime.Now;
                _Context.Variables.Add(variable);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    public void UpdateVariable(Variable variable)
    {
        ExecuteWithRetry(() =>
        {
            // 1. Busco en el ChangeTracker si ya hay una entidad con la MISMA clave
            var entryType = _Context.Model.FindEntityType(typeof(Variable));
            var pk = entryType.FindPrimaryKey().Properties;

            // Clave compuesta → comparo todas las propiedades
            var duplicate = _Context.ChangeTracker.Entries<Variable>()
                              .FirstOrDefault(e =>
                                  pk.All(p =>
                                  {
                                      var current = e.Property(p.Name).CurrentValue;
                                      var newVal = typeof(Variable).GetProperty(p.Name)?.GetValue(variable);
                                      return Equals(current, newVal);
                                  }));

            if (duplicate != null)
            {
                // 2. Si la entidad ya está en memoria, solo actualizo sus valores
                duplicate.CurrentValues.SetValues(variable);
            }
            else
            {
                // 3. Si NO estaba, la adjunto como modificada
                _Context.Attach(variable);
                _Context.Entry(variable).State = EntityState.Modified;
            }

            _Context.SaveChanges();

        });
    }
    public void DeleteVariable(int VariableId)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                var variable = _Context.Variables.FirstOrDefault(x => x.VariableId == VariableId);
                if (variable == null)
                {
                    throw new Exception("variable not found");
                }
                _Context.Variables.Remove(variable);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    #endregion

    //Users
    #region Users
    public User? GetUser(User User)
    {
        return ExecuteReadWithRetry(() =>
        {
            return _Context.Users.AsNoTracking().FirstOrDefault(x => x.UserId == User.UserId || x.UserName == User.UserName);
        });
    }
    public User CreateUser(User user)
    {
        return ExecuteWriteWithRetry(() =>
        {
            try
            {
                _Context.Users.Add(user);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            return user;
        });
    }
    public void UpdateUser(User user)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                _Context.Users.Update(user);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    public void DeleteUser(int UserId)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                var user = _Context.Users.FirstOrDefault(x => x.UserId == UserId);
                if (user == null)
                {
                    throw new Exception("User not found");
                }
                _Context.Users.Remove(user);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    #endregion

    //Locations
    #region Locations
    public Location? GetLocation(int LocationId)
    {
        return ExecuteReadWithRetry(() =>
        {
            return _Context.Locations.AsNoTracking().FirstOrDefault(x => x.LocationId == LocationId);
        });
    }
    public void CreateLocation(Location location)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                _Context.Locations.Add(location);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    public void UpdateLocation(Location location)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                _Context.Locations.Update(location);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    public void DeleteLocation(int LocationId)
    {
        ExecuteWithRetry(() =>
        {
            try
            {
                var location = _Context.Locations.FirstOrDefault(x => x.LocationId == LocationId);
                if (location == null)
                {
                    throw new Exception("Location not found");
                }
                _Context.Locations.Remove(location);
                _Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }
    #endregion

    //Traductions
    #region Traductions
    public string GetTraduction(string Key, int languageId)
    {
        return ExecuteReadWithRetry(() =>
        {
            return _Context.Translations.AsNoTracking().FirstOrDefault(x => x.TraductionReference == Key && x.LanguageId == languageId)?.Value ?? Key;
        });
    }
    #endregion



    //Other
    #region Other
    void ReloadSettings()
    {
        _Settings = _SettingsActions.ReadSettings();
    }
    #endregion


    private static readonly Random Random = new Random();
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    // Método para generar un código alfanumérico
    public string GenerateCode()
    {
        StringBuilder result = new StringBuilder(_Settings.ReferenceCodeLength);
        for (int i = 0; i < _Settings.ReferenceCodeLength; i++)
        {
            result.Append(Characters[Random.Next(Characters.Length)]);
        }
        return result.ToString();
    }

    // Método para garantizar la unicidad del código en la base de datos
    public string GenerateUniqueToken()
    {
        string Token;
        do
        {
            Token = GenerateCode();
        } while (_Context.Actuators.Any(actual => actual.Token == Token) ||
                 _Context.Sensors.Any(actual => actual.Token == Token));
        return Token;
    }
    public string GenerateDeviceReference()
    {
        string newCode;
        do
        {
            newCode = GenerateCode();
        } while (_Context.Actuators.FirstOrDefault(actual => actual.DeviceReference == newCode) != default && _Context.Sensors.FirstOrDefault(actual => actual.DeviceReference == newCode) != default);
        return newCode;
    }
}

public interface IDatabasesActions
{
    //Sensor
    public string? ReloadSensorToken(int sensorId, bool ignorarIntervalo = false);

    //Actuator
    public string? ReloadActuatorToken(int sensorId, bool ignorarIntervalo = false);
    
    //Sensors And Actuators
    void AddDeviceHistory(string DeviceReference, string Value);
    List<SensorHistory> GetDeviceHistory(SensorHistory deviceHistory);
    List<T> GetDevice<T>(T Device) where T : class;
    void CreateDevice<T>(T Device);
    void ReloadAllPendingToken();
    void UpdateDevice<T>(T Device) where T : class;
    void DeleteDevice<T>(int DeviceId);
    public string GenerateUniqueToken();
    public string GenerateDeviceReference();

    //Variables
    List<Variable>? GetVariable(Variable Variable);
    void CreateVariable(Variable variable);
    void UpdateVariable(Variable variable);
    void DeleteVariable(int VariableId);

    //Users
    User? GetUser(User UserId);
    User CreateUser(User user);
    void UpdateUser(User user);
    void DeleteUser(int UserId);

    //Locations
    Location? GetLocation(int LocationId);
    void CreateLocation(Location location);
    void UpdateLocation(Location location);
    void DeleteLocation(int LocationId);

    //Traductions
    string GetTraduction(string Key, int languageId);
}