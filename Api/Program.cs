using Services.GeneralFunctions.Settings;
using Services.GeneralFunctions.Logger;
using Services.GeneralFunctions;
using Api;
using Services.SensorsAndActuators;
using Services.Variables;
using Data;
using Services.Databases;
using System.Runtime;
using Microsoft.EntityFrameworkCore;
using Services.Web.Cookies;
using Services.Traductions;
using Web.Cookies;
using Services.SignalR.Server;
using Services.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(System.Net.IPAddress.Any, 5000); // Escucha en todas las interfaces en el puerto 5000
    // serverOptions.Listen(System.Net.IPAddress.Parse("192.168.0.203"), 5000); // Alternativamente, escucha en una IP específica
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();


builder.Services.AddSingleton<IClientSignalRConnetionManager, ClientSignalRConnetionManager>();
builder.Services.AddSingleton<IClientSignalRRequestManager, ClientSignalRRequestManager>();

builder.Services.AddSingleton<IFileFunctions, FileFunctions>();

builder.Services.AddSignalR();

var app = builder.Build();

#region Event subscribers
#endregion

app.Services.GetRequiredService<IClientSignalRConnetionManager>().CreateConnection("https://localhost:5000");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
