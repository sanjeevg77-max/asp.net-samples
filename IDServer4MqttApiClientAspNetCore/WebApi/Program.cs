using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.AttributeRouting;
using MQTTnet.Server;


// Go To : https://localhost:7201/weatherforecast

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMqttControllers(
    /*
        By default, all controllers within the executing assembly are
        discovered (just pass nothing here). To provide a list of assemblies
        explicitly, pass an array of Assembly[] here.
    */
    );

builder.Services.AddSingleton<MqttHostedServer>();
builder.Services.AddSingleton<IMqttServerOptions>(new MqttServerOptionsBuilder().WithDefaultEndpoint().Build());
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttHostedServer>());
builder.Services.AddConnections();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7201, listenOptions => listenOptions.UseHttps());
    serverOptions.ListenAnyIP(1883, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.None;
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://192.168.2.3:7232";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// Root endpoint for MQTT - attribute routing picks up after this URL
app.MapMqtt("/mqtt");

app.MapControllers();

app.Run();