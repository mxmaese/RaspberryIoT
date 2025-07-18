using Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Services.Databases;
using Services.GeneralFunctions.Settings;
using Services.GeneralFunctions;
using Services.SensorsAndActuators;
using Services.Traductions;
using Services.Variables;
using Services.Events;
using Web.Cookies;
using Services.Administrate;
using Services.Users;
using Services.GeneralFunctions.Logger;
using Services.Web.Cookies;
using MySqlConnector;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Services.Web.Auth;
using Services.SignalR.Server;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddControllers();


        // Configurar autenticación con cookies y JWT
        var cfg = builder.Configuration;
        var key = Encoding.UTF8.GetBytes(cfg["Jwt:Key"]);
        var issuer = cfg["Jwt:Issuer"]!;

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        })
        .AddJwtBearer("Events", o => o.TokenValidationParameters = ParamsFor("events-api"))
        .AddJwtBearer("Administrate", o => o.TokenValidationParameters = ParamsFor("administrate-api"))
        .AddJwtBearer("Devices", o => o.TokenValidationParameters = ParamsFor("devices-api"));

        TokenValidationParameters ParamsFor(string aud) => new()
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = aud,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        builder.Services.AddAuthorization();

        builder.WebHost.ConfigureKestrel(options =>
        {
            #if DEBUG
                options.ListenAnyIP(5000, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            #else
                options.ListenAnyIP(443, listenOptions =>
                {
                    listenOptions.UseHttps("../IotCertificate.pfx", "AdelV1043");
                });
            #endif
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<DatabaseContext>(sp =>
        {
            var settings = sp.GetRequiredService<ISettings>().ReadSettings();
            var cs = settings.DatabaseConection.GetConnectionString();

            // Tu factory estático
            return ContextFactory.CreateContext<DatabaseContext>(cs);
        });

        builder.Services.AddSingleton<IDatabasesActions, DatabasesActions>(/*provider =>
        {
            return new DatabasesActions(context, provider.GetRequiredService<ISettings>(), provider.GetRequiredService<IGeneralVariables>());
        }*/);

        builder.Services.AddSignalR();

        builder.Services.AddSingleton<IActuator, Actuator>();
        builder.Services.AddSingleton<ILocation, Location>();
        builder.Services.AddSingleton<IUsers, Users>();
        builder.Services.AddSingleton<ISensor, Sensor>();
        builder.Services.AddSingleton<IUser, User>();
        builder.Services.AddSingleton<IVariable, Variable>();


        builder.Services.AddSingleton<ISettings, Settings>();
        builder.Services.AddSingleton<IFileFunctions, FileFunctions>();
        builder.Services.AddSingleton<Services.GeneralFunctions.Logger.ILogger, Logger>();

        builder.Services.AddSingleton<IEvent, Event>();
        builder.Services.AddSingleton<IEvents, Events>();
        builder.Services.AddSingleton<IHostedService, EventTimer>();
        // Registro de ISensors con su implementación Sensors
        builder.Services.AddSingleton<IGeneralVariables, GeneralVariables>();
        builder.Services.AddSingleton<ICalculateDynamicVariables, CalculateDynamicVariables>();
        builder.Services.AddSingleton<ISensors, Sensors>();
        builder.Services.AddSingleton<IActuators, Actuators>();
        builder.Services.AddSingleton<IHostedService, ChangeReferenceCodeTimer>();
        builder.Services.AddSingleton<IHostedService, CalculateDynamicVariablesTimer>();


        builder.Services.AddSingleton<IAuthCookiesManager, AuthCookiesManager>();
        builder.Services.AddSingleton<ITraductionManager, TraductionManager>();
        builder.Services.AddSingleton<ICookiesManager, CookiesManager>();

        builder.Services.AddSingleton<IJwtTokenBuilder, JwtTokenBuilder>();

        builder.Services.AddSingleton<IManageSignalRConnection, ManageSignalRConnection>();

        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.MapHub<SignalRHub>("/Hub");

        #region Event subscribers
        GeneralVariables.OnVariableCalculated += variableid => app.Services.GetRequiredService<ICalculateDynamicVariables>().CalculateDynamicVariablesThatDependsOn(variableid);
        app.Services.GetRequiredService<IManageSignalRConnection>().GetISensor(app.Services.GetRequiredService<ISensors>());
        app.Services.GetRequiredService<IManageSignalRConnection>().GetIActuator(app.Services.GetRequiredService<IActuators>());
        #endregion

#if DEBUG
        //        app.Services.GetRequiredService<ISensors>().SaveInformation("ddgsdfhfgs", 25.5f);
#endif


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapRazorPages();

        app.Run();
    }
}