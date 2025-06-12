using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data;

//crea el contexto de la base de datos
public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {

    }
    
    public DbSet<Sensor>        Sensors        => Set<Sensor>();
    public DbSet<Actuator>      Actuators      => Set<Actuator>();
    public DbSet<SensorHistory> SensorHistory  => Set<SensorHistory>();
    public DbSet<User>          Users          => Set<User>();
    public DbSet<Group>         Groups         => Set<Group>();
    public DbSet<Permission>    Permissions    => Set<Permission>();
    public DbSet<DeviceGroup>   DevicesGroups  => Set<DeviceGroup>();
    public DbSet<DeviceUser>    DevicesUsers   => Set<DeviceUser>();
    public DbSet<Location>      Locations      => Set<Location>();
    public DbSet<Variable>      Variables      => Set<Variable>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Traduction> Translations => Set<Traduction>();
    public DbSet<Event> Events => Set<Event>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ——— SENSORS ——————————————————————————
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("sensors");
            entity.HasKey(e => e.SensorId);

            entity.Property(e => e.SensorId)           .HasColumnName("sensor_id");
            entity.Property(e => e.DeviceReference)    .HasColumnName("device_reference");
            entity.Property(e => e.Name)               .HasColumnName("name");
            entity.Property(e => e.LocationId)         .HasColumnName("location_id");
            entity.Property(e => e.OwnerId)            .HasColumnName("owner_id");
            entity.Property(e => e.Status)             .HasColumnName("status");
            entity.Property(e => e.GroupId)            .HasColumnName("group_id");
            entity.Property(e => e.AssignedVariableId) .HasColumnName("assigned_variable_id");
            entity.Property(e => e.Token)              .HasColumnName("token");
            entity.Property(e => e.CreatedAt)          .HasColumnName("created_at");
            entity.Property(e => e.LastReferenceChange).HasColumnName("last_reference_change");

            entity.HasIndex(e => e.DeviceReference).IsUnique();
            entity.HasAlternateKey(e => e.DeviceReference);

            entity.HasOne(u => u.Owner).WithMany().HasForeignKey(e => e.OwnerId);
            entity.HasOne(u => u.Group).WithMany().HasForeignKey(e => e.GroupId);
            entity.HasOne(u => u.AssignedVariable).WithMany().HasForeignKey(e => e.AssignedVariableId)
                                      .OnDelete(DeleteBehavior.NoAction);
        });

        // ——— ACTUATORS ————————————————————————
        modelBuilder.Entity<Actuator>(entity =>
        {
            entity.ToTable("actuators");
            entity.HasKey(e => e.ActuatorId);

            entity.Property(e => e.ActuatorId)         .HasColumnName("actuator_id");
            entity.Property(e => e.DeviceReference)    .HasColumnName("device_reference");
            entity.Property(e => e.Name)               .HasColumnName("name");
            entity.Property(e => e.LocationId)         .HasColumnName("location_id");
            entity.Property(e => e.OwnerId)            .HasColumnName("owner_id");
            entity.Property(e => e.Status)             .HasColumnName("status");
            entity.Property(e => e.GroupId)            .HasColumnName("group_id");
            entity.Property(e => e.AssignedVariableId) .HasColumnName("assigned_variable_id");
            entity.Property(e => e.Token)              .HasColumnName("token");
            entity.Property(e => e.CreatedAt)          .HasColumnName("created_at");
            entity.Property(e => e.LastReferenceChange).HasColumnName("last_reference_change");

            entity.HasIndex(e => e.DeviceReference).IsUnique();
            entity.HasAlternateKey(e => e.DeviceReference);

            entity.HasOne(u => u.Owner).WithMany().HasForeignKey(e => e.OwnerId);
            entity.HasOne(u => u.Group).WithMany().HasForeignKey(e => e.GroupId);
            entity.HasOne(u => u.AssignedVariable).WithMany().HasForeignKey(e => e.AssignedVariableId)
                                      .OnDelete(DeleteBehavior.NoAction);
        });

        // ——— SENSOR HISTORY ————————————————————
        modelBuilder.Entity<SensorHistory>(entity =>
        {
            entity.ToTable("sensor_history");
            entity.HasKey(e => e.HistoryId);

            entity.Property(e => e.HistoryId)       .HasColumnName("history_id");
            entity.Property(e => e.DeviceReference) .HasColumnName("device_reference");
            entity.Property(e => e.Value)           .HasColumnName("value");
            entity.Property(e => e.Timestamp)       .HasColumnName("timestamp");

            entity.HasOne(e => e.Sensor)                 // navegación
                  .WithMany(s => s.Histories)              // colección en Sensor (opcional)
                  .HasForeignKey(e => e.DeviceReference) // FK local
                  .HasPrincipalKey(s => s.DeviceReference); // clave en la principal
        });

        // ——— USERS ————————————————————————————
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)    .HasColumnName("user_id");
            entity.Property(e => e.UserName)  .HasColumnName("username");
            entity.Property(e => e.Email)     .HasColumnName("email");
            entity.Property(e => e.Password)  .HasColumnName("password");
            entity.Property(e => e.ApiToken)  .HasColumnName("api_token");
            entity.Property(e => e.CreatedAt) .HasColumnName("created_at");
        });

        // ——— GROUPS ———————————————————————————
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("groups");
            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.GroupId)    .HasColumnName("group_id");
            entity.Property(e => e.Name)       .HasColumnName("name");
            entity.Property(e => e.OwnerId)    .HasColumnName("owner_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt)  .HasColumnName("created_at");

            entity.HasOne<User>().WithMany().HasForeignKey(e => e.OwnerId);
        });

        // ——— PERMISSIONS ——————————————————————
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionId);

            entity.Property(e => e.PermissionId)    .HasColumnName("permission_id");
            entity.Property(e => e.GroupId)         .HasColumnName("group_id");
            entity.Property(e => e.DeviceReference) .HasColumnName("device_reference");
            entity.Property(e => e.UserId)          .HasColumnName("user_id");
            entity.Property(e => e.PermissionLevel) .HasColumnName("permission_level");
            entity.Property(e => e.CreatedAt)       .HasColumnName("created_at");

            entity.HasOne<Group>() .WithMany().HasForeignKey(e => e.GroupId);
            entity.HasOne<Sensor>() .WithMany().HasForeignKey(e => e.DeviceReference)
                                     .HasPrincipalKey(s => s.DeviceReference);
            entity.HasOne<Actuator>().WithMany().HasForeignKey(e => e.DeviceReference)
                                     .HasPrincipalKey(a => a.DeviceReference);
            entity.HasOne<User>()  .WithMany().HasForeignKey(e => e.UserId);
        });

        // ——— DEVICE ↔ GROUP ————————————————
        modelBuilder.Entity<DeviceGroup>(entity =>
        {
            entity.ToTable("assignments_device_group");
            entity.HasKey(e => e.AssignmentId);

            entity.Property(e => e.AssignmentId)    .HasColumnName("assignment_id");
            entity.Property(e => e.DeviceReference) .HasColumnName("device_reference");
            entity.Property(e => e.GroupId)         .HasColumnName("group_id");

            entity.HasOne<Group>() .WithMany().HasForeignKey(e => e.GroupId);
            entity.HasOne<Sensor>() .WithMany().HasForeignKey(e => e.DeviceReference)
                                     .HasPrincipalKey(s => s.DeviceReference);
            entity.HasOne<Actuator>().WithMany().HasForeignKey(e => e.DeviceReference)
                                     .HasPrincipalKey(a => a.DeviceReference);
        });

        // ——— DEVICE ↔ USER ————————————————————
        modelBuilder.Entity<DeviceUser>(entity =>
        {
            entity.ToTable("assignments_device_user");
            entity.HasKey(e => e.AssignmentId);

            entity.Property(e => e.AssignmentId)    .HasColumnName("assignment_id");
            entity.Property(e => e.DeviceReference) .HasColumnName("device_reference");
            entity.Property(e => e.UserId)          .HasColumnName("user_id");

            entity.HasOne<User>()   .WithMany().HasForeignKey(e => e.UserId);
            entity.HasOne<Sensor>() .WithMany().HasForeignKey(e => e.DeviceReference)
                                     .HasPrincipalKey(s => s.DeviceReference);
            entity.HasOne<Actuator>().WithMany().HasForeignKey(e => e.DeviceReference)
                                     .HasPrincipalKey(a => a.DeviceReference);
        });

        // ——— LOCATIONS ————————————————————————
        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("locations");
            entity.HasKey(e => e.LocationId);

            entity.Property(e => e.LocationId) .HasColumnName("location_id");
            entity.Property(e => e.Name)       .HasColumnName("name");
            entity.Property(e => e.GroupId)    .HasColumnName("group_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt)  .HasColumnName("created_at");

            entity.HasOne<Group>().WithMany().HasForeignKey(e => e.GroupId);
        });

        // ——— VARIABLES ————————————————————————
        modelBuilder.Entity<Variable>(entity =>
        {
            entity.ToTable("variables");
            entity.HasKey(e => e.VariableId);

            entity.Property(e => e.VariableId) .HasColumnName("variable_id");
            entity.Property(e => e.Name)       .HasColumnName("name");
            entity.Property(e => e.Type)       .HasColumnName("type");
            entity.Property(e => e.Value)      .HasColumnName("value");
            entity.Property(e => e.IsDynamic)  .HasColumnName("is_dynamic");
            entity.Property(e => e.Formula)    .HasColumnName("formula");
            entity.Property(e => e.OwnerId)    .HasColumnName("owner_id");
            entity.Property(e => e.CreatedAt)  .HasColumnName("created_at");

            entity.HasOne<User>().WithMany().HasForeignKey(e => e.OwnerId);
        });

        // ——— LANGUAGES —————————————————————
        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("languages");
            entity.HasKey(e => e.LanguageId);

            entity.Property(e => e.LanguageId)  .HasColumnName("language_id");
            entity.Property(e => e.LanguageName).HasColumnName("name");
        });

        // ——— TRANSLATIONS —————————————————————
        modelBuilder.Entity<Traduction>(entity =>
        {
            entity.ToTable("translations");
            entity.HasKey(e => e.TraductionId);

            entity.Property(e => e.TraductionId)       .HasColumnName("traduction_id");
            entity.Property(e => e.LanguageId)         .HasColumnName("language_id");
            entity.Property(e => e.TraductionReference).HasColumnName("traduction_reference");
            entity.Property(e => e.Value)              .HasColumnName("value");
        });

        // ——— EVENTS ————————————————————————
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.EventId);

            entity.Property(e => e.EventId)        .HasColumnName("event_id");
            entity.Property(e => e.Name)           .HasColumnName("name");
            entity.Property(e => e.OwnerId)        .HasColumnName("owner_id");
            entity.Property(e => e.Actions)        .HasColumnName("actions");
            entity.Property(e => e.Condition)      .HasColumnName("condition");
            entity.Property(e => e.IntervalMinutes).HasColumnName("interval_minutes");
            entity.Property(e => e.ScheduledTime)  .HasColumnName("scheduled_time");
            entity.Property(e => e.IsEnabled)      .HasColumnName("is_enabled");
            entity.Property(e => e.CreatedAt)      .HasColumnName("created_at");
            entity.Property(e => e.LastExecutedAt) .HasColumnName("last_executed_at");

            entity.HasOne<User>().WithMany().HasForeignKey(e => e.OwnerId);
        });
    }
}