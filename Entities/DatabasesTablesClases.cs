using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using static Entities.Variable;
namespace Entities;


public interface IDevice
{
    string DeviceReference { get; set; }
    string Name { get; set; }
    int? LocationId { get; set; }
    int OwnerId { get; set; }
    string? Status { get; set; }
    int? GroupId { get; set; }
    int AssignedVariableId { get; set; }
    string Token { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime LastReferenceChange { get; set; }

    Location? Location { get; set; }
    Group? Group { get; set; }
    User Owner { get; set; }
    Variable? AssignedVariable { get; set; }
}
public class Sensor : IDevice 
{
    public int SensorId { get; set; }
    public string DeviceReference { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? LocationId { get; set; }
    public int OwnerId { get; set; }
    public string? Status { get; set; }
    public int? GroupId { get; set; }
    public int AssignedVariableId { get; set; }
    public string Token { get; set; } = null!; // Token for the arduino 
    public DateTime CreatedAt { get; set; }
    public DateTime LastReferenceChange { get; set; }

    public Location? Location { get; set; }
    public Group? Group { get; set; }
    public User Owner { get; set; } = null!;
    public Variable? AssignedVariable { get; set; }
    public List<SensorHistory> Histories { get; set; } = null!;

    public static Sensor GetNull(int sensorId = -1, string? name = null, int? location = null, int ownerId = -1, string? status = null, int? groupId = null, int assignedVariableId = -1, string token = null, string deviceReference = null)
    {
        return new Sensor()
        {
            SensorId = sensorId,
            Name = name,
            LocationId = location,
            OwnerId = ownerId,
            Status = status,
            GroupId = groupId,
            AssignedVariableId = assignedVariableId,
            DeviceReference = deviceReference,
            Token = token,
        };
    }
}
public class Actuator : IDevice
{
    public int ActuatorId { get; set; }
    public string DeviceReference { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? LocationId { get; set; }
    public int OwnerId { get; set; }
    public string? Status { get; set; }
    public int? GroupId { get; set; }
    public int AssignedVariableId { get; set; }
    public string Token { get; set; } = null!; // Token for the arduino 
    public DateTime CreatedAt { get; set; }
    public DateTime LastReferenceChange { get; set; }

    public Location? Location { get; set; }
    public Group? Group { get; set; }
    public User Owner { get; set; } = null!;
    public Variable? AssignedVariable { get; set; }

    public static Actuator GetNull(int actuatorId = -1, string? name = null, int? location = null, int ownerId = -1, string? status = null, int? groupId = null, int assignedVariableId = -1, string token = null, string deviceReference = null)
    {
        return new Actuator()
        {
            ActuatorId = actuatorId,
            Name = name,
            LocationId = location,
            OwnerId = ownerId,
            Status = status,
            GroupId = groupId,
            AssignedVariableId = assignedVariableId,
            DeviceReference = deviceReference,
            Token = token,
        };
    }
}
public class SensorHistory
{
    public int HistoryId { get; set; }
    public string DeviceReference { get; set; } = null!;
    public string Value { get; set; } = null!;
    public DateTime Timestamp { get; set; }

    public Sensor? Sensor { get; set; }
}
public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ApiToken { get; set; } = null!; // Token to authenticate API requests
    public DateTime CreatedAt { get; set; }
}
public class Group
{
    public int GroupId { get; set; }
    public string Name { get; set; } = null!;
    public int OwnerId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class Permission
{
    public int PermissionId { get; set; }
    public int GroupId { get; set; }
    public string? DeviceReference { get; set; }
    public int UserId { get; set; }
    public string PermissionLevel { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Sensor? Sensor { get; set; }
    public Actuator? Actuator { get; set; }

    [NotMapped]
    public IDevice? Device
    {
        get => (IDevice?)Sensor ?? Actuator;
        set
        {
            if (value is Sensor s) Sensor = s;
            if (value is Actuator a) Actuator = a;
        }
    }
}
public class DeviceGroup
{
    public int AssignmentId { get; set; }
    public string DeviceReference { get; set; } = null!;
    public int GroupId { get; set; }

    public Sensor? Sensor { get; set; }
    public Actuator? Actuator { get; set; }

    [NotMapped]
    public IDevice? Device
    {
        get => (IDevice?)Sensor ?? Actuator;
        set
        {
            if (value is Sensor s) Sensor = s;
            if (value is Actuator a) Actuator = a;
        }
    }
}
public class DeviceUser
{
    public int AssignmentId { get; set; }
    public string DeviceReference { get; set; } = null!;
    public int UserId { get; set; }

    public Sensor? Sensor { get; set; }
    public Actuator? Actuator { get; set; }

    [NotMapped]
    public IDevice? Device
    {
        get => (IDevice?)Sensor ?? Actuator;
        set
        {
            if (value is Sensor s) Sensor = s;
            if (value is Actuator a) Actuator = a;
        }
    }
}
public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; } = null!;
    public int GroupId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class Variable
{
    public int VariableId { get; set; } 
    public string? Name { get; set; } = null!;
    [NotMapped]
    private VariableType? _type = VariableType.None;
    public VariableType? Type
    {
        get => _type ?? VariableType.None;
        set => _type = value;
    }
    public string? Value { get; set; }
    public bool IsDynamic { get; set; }
    public string? Formula { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Variable()
    {
        CreatedAt = DateTime.UtcNow;
    }
    public static Variable GetNull(int? variableId = null, string? name = null, VariableType? type = null, string? value = null, bool? isDynamic = null, string? formula = null, int? ownerId = null)
    {
        return new Variable()
        {
            VariableId = variableId ?? -1,
            Name = name,
            Type = type,
            Value = value,
            IsDynamic = isDynamic ?? false,
            Formula = formula,
            OwnerId = ownerId ?? -1,
        };
    }

    public enum VariableType
    {
        None,
        Int,
        Float,
        Bool,
        String
    }
}
public class Language
{
    public int LanguageId { get; set; }
    public string LanguageName { get; set; }
}

public class Traduction
{
    public int TraductionId { get; set; }
    public string TraductionReference { get; set; }
    public int LanguageId { get; set; }
    public string Value { get; set; }
}

public class Event
{
    public int EventId { get; set; }
    public string? Name { get; set; }
    public int OwnerId { get; set; }
    public string Actions { get; set; } = null!;
    public EventTriggerType TriggerType { get; set; }
    public int? IntervalMinutes { get; set; }
    public string? DailyTime { get; set; }
    public DateTime? LastExecution { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? Owner { get; set; }

    public enum EventTriggerType
    {
        Api,
        Timer,
        Both
    }

    public static Event GetNull(int eventId = -1, int ownerId = -1, string? name = null)
    {
        return new Event()
        {
            EventId = eventId,
            OwnerId = ownerId,
            Name = name
        };
    }
}