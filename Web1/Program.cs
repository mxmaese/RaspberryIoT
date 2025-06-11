using Services.Databases;
using Services.GeneralFunctions.Settings;
using Services.GeneralFunctions;
using Services.SensorsAndActuators;
using Services.Variables;
using Data;
using Web.Cookies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Services.Traductions;
using Entities;
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorPages();




    var settings = new Settings(new FileFunctions()).ReadSettings();
    var connectionString = settings.DatabaseConection.GetConnectionString();
    var context = ContextFactory.CreateContext<DatabaseContext>(connectionString);

    builder.Services.AddHttpContextAccessor();

    // Configurar autenticación con cookies
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true; // Asegura que la cookie solo se pueda acceder desde el servidor
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Utiliza siempre HTTPS
        });

    builder.Services.AddSingleton<IDatabasesActions, DatabasesActions>(provider =>
    {
        return new DatabasesActions(context, provider.GetRequiredService<IGeneralVariables>(), provider.GetRequiredService<ISettings>());
    });

    builder.Services.AddSingleton<ISensors, Sensors>();
    builder.Services.AddSingleton<IActuators, Actuators>();

    builder.Services.AddSingleton<Services.Administrate.IActuator, Services.Administrate.Actuator>();
    builder.Services.AddSingleton<Services.Administrate.ILocation, Services.Administrate.Location>();
    builder.Services.AddSingleton<Services.Users.IUsers, Services.Users.Users>();
    builder.Services.AddSingleton<Services.Administrate.ISensor, Services.Administrate.Sensor>();
    builder.Services.AddSingleton<Services.Administrate.IUser, Services.Administrate.User>();
    builder.Services.AddSingleton<Services.Administrate.IVariable, Services.Administrate.Variable>();


    builder.Services.AddSingleton<ISettings, Settings>();
    builder.Services.AddSingleton<ICookiesManager, CookiesManager>();
    builder.Services.AddSingleton<Services.Web.Cookies.IAuthCookiesManager, Services.Web.Cookies.AuthCookiesManager>();
    builder.Services.AddSingleton<ITraductionManager, TraductionManager>();
    builder.Services.AddSingleton<IFileFunctions, FileFunctions>();
    builder.Services.AddSingleton<Services.GeneralFunctions.Logger.ILogger, Services.GeneralFunctions.Logger.Logger>();

    // Registro de ISensors con su implementación Sensors
    builder.Services.AddSingleton<IGeneralVariables, GeneralVariables>();


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
}