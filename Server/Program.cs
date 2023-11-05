using Burkenyo.Iot.Server;
using HueApi;
using MQTTnet.AspNetCore;
using MQTTnet.Server;

const ushort MqttPort = 1883;
const ushort DefaultTestPagePort = 8080;

var builder = WebApplication.CreateSlimBuilder(args);

// configure TCP listeners
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on the MQTT port delegate TCP processing to the MQTT server
    options.ListenAnyIP(MqttPort, listener => listener.UseMqtt());
    // Listen on the HTTP port and use the normal HTTP request pipeline
    options.ListenAnyIP(builder.Configuration.GetValue<ushort?>("TestPagePort") ?? DefaultTestPagePort);
});

// add services for the MQTT server and MQTT event processors
builder.Services.AddMqttServer();
builder.Services.AddSingleton<MqttClientAuthenticator>();
builder.Services.AddSingleton<ButtonPressEventProducer>();

// add service to talk to the smart light
builder.Services.AddHostedService<ButtonPressProcessor>();
builder.Services.AddHttpClient(nameof(LocalHueApi))
    .AddTypedClient(httpClient =>
        new LocalHueApi(builder.Configuration["PhilipsHue:Host"]!, builder.Configuration["PhilipsHue:ApiKey"], httpClient));

// add services for the Blazor testing page
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

// configure MQTT server event handlers
app.UseMqttClientAuthenticator<MqttClientAuthenticator>();
app.UseMqttMessageProcessor<ButtonPressEventProducer>();

app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
    app.Services.GetRequiredService<MqttServer>().StopAsync());

// add Blazor UI for the testing page
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();