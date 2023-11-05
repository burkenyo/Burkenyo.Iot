using Burkenyo.Iot.Shared;
using HueApi;
using HueApi.Models;
using HueApi.Models.Requests;
namespace Burkenyo.Iot.Server;

class ButtonPressProcessor(
    ButtonPressEventProducer eventProducer,
    LocalHueApi hueClient,
    IConfiguration configuration
) : IHostedService
{
    public static readonly On s_on = new() { IsOn = true };
    public static readonly On s_off = new();

    // The values work with the limited “B” gamut of early Hue bulbs
    public static readonly Color s_red    = new() { Xy = new() { X = 0.69, Y = 0.31 } };
    public static readonly Color s_yellow = new() { Xy = new() { X = 0.47, Y = 0.48 } };
    public static readonly Color s_green  = new() { Xy = new() { X = 0.41, Y = 0.52 } };
    public static readonly Color s_blue   = new() { Xy = new() { X = 0.19, Y = 0.09 } };

    Light _light = null!;
    ButtonInfo _lastButtons;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(eventProducer);

        var lights = EnsureSuccess(await hueClient.GetLightsAsync());
        _light = lights.Single(l => l.Metadata!.Name == configuration["PhilipsHue:LightName"]);

        eventProducer.ButtonsPressed += ListenForButtonsAsync;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        eventProducer.ButtonsPressed -= ListenForButtonsAsync;

        EnsureSuccess(await hueClient.UpdateLightAsync(_light.Id, new() { On = s_off }));
    }

    async Task ListenForButtonsAsync(ButtonInfo buttons)
    {
        var newButtons = _lastButtons.NewSince(buttons);
        _lastButtons = buttons;
        if (!newButtons.Any)
        {
            return;
        }

        _light.Color = newButtons switch
        {
            { Red: true } => s_red,
            { Yellow: true } => s_yellow,
            { Green: true } => s_green,
            { Blue: true } => s_blue,
            _ => _light.Color
        };

        _light.Dimming!.Brightness = newButtons switch
        {
            { Up: true } => Math.Min(_light.Dimming.Brightness + 50, 100),
            { Down: true } => Math.Max(_light.Dimming.Brightness - 50, 0),
            _ => _light.Dimming.Brightness
        };

        // Only include properties in the upload payload that can be modified.
        UpdateLight update = new()
        {
            On = s_on,
            Color = new() { Xy = _light.Color!.Xy },
            Dimming = new() { Brightness = _light.Dimming.Brightness },
        };

        EnsureSuccess(await hueClient.UpdateLightAsync(_light.Id, update));
    }

    static List<T> EnsureSuccess<T>(HueResponse<T> response)
    {
        if (response.HasErrors)
        {
            throw new InvalidOperationException(String.Join(' ', response.Errors.Select(e => e.Description)));
        }

        return response.Data;
    }
}
