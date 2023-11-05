using System;
using System.Device.Adc;
using System.Device.Gpio;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Blinky;
using Burkenyo.Iot.Shared;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.Networking;

const int DPadLowCutoff = 2000;
const int DPadMedCutoff = 4400;
const int DPadHighCutoff = 6700;

GpioController gpioController = new();
var led = gpioController.OpenPin(13, PinMode.Output);

gpioController.OpenPin(21, PinMode.Output).Write(PinValue.High);
Ws2812c neoPixel = new(33, 1);

var boardButton = gpioController.OpenPin(0, PinMode.InputPullUp);

var redButton = gpioController.OpenPin(9, PinMode.InputPullUp);
var yellowButton = gpioController.OpenPin(12, PinMode.InputPullUp);
var greenButton = gpioController.OpenPin(11, PinMode.InputPullUp);
var blueButton = gpioController.OpenPin(10, PinMode.InputPullUp);

AdcController adcController = new();
var dPadX = adcController.OpenChannel(4);
var dPadY = adcController.OpenChannel(5);

led.Write(PinValue.High);
connectToWifi();
var client = connectToMqtt();
led.Write(PinValue.Low);

ButtonInfo oldButtons = default;
var currentBrightness = Brightness.Medium;
var currentColor = Color.None;

Timer buttonTimer = new(readButtons, null, 0, 20);

while (true)
{
    var @event = EventQueue.Dequeue();
    if(@event is ButtonInfo buttons)
    {
        sendButtons(buttons);
    }
}

void readButtons(object? _)
{
    var dPadXValue = dPadX.ReadValue();
    var dPadYValue = dPadY.ReadValue();

    ButtonInfo newButtons = new
    (
        left: dPadXValue is > DPadLowCutoff and < DPadMedCutoff,
        right: dPadXValue > DPadHighCutoff,
        up: dPadYValue is > DPadLowCutoff and < DPadMedCutoff,
        down: dPadYValue > DPadHighCutoff,
        red: !(bool)redButton.Read(),
        yellow: !(bool)yellowButton.Read(),
        green: !(bool)greenButton.Read(),
        blue: !(bool)blueButton.Read()
    );

    if (newButtons != oldButtons)
    {
        if (boardButton.Read() == PinValue.Low)
        {
            Console.WriteLine(newButtons.ToString());
        }
        else
        {
            EventQueue.Enqueue(newButtons);
        }

        updateNeoPixel(newButtons);
    }

    oldButtons = newButtons;
}

void updateNeoPixel(ButtonInfo buttons)
{
    currentColor = buttons switch
    {
        { Red: true } => Color.Red,
        { Yellow: true } => Color.Yellow,
        { Green: true } => Color.Green,
        { Blue: true } => Color.Blue,
        _ => currentColor
    };

    if (currentColor is Color.None)
    {
        return;
    }

    currentBrightness = buttons switch
    {
        { Up: true } when currentBrightness != Brightness.High => currentBrightness + 1,
        { Down: true } when currentBrightness != Brightness.Low => currentBrightness - 1,
        _ => currentBrightness
    };

    var color = currentColor switch
    {
        Color.Red => System.Drawing.Color.FromArgb(1 + 12 * (int)currentBrightness, 0, 0),
        Color.Yellow => System.Drawing.Color.FromArgb(1 + 6 * (int)currentBrightness, 1 + 6 * (int)currentBrightness, 0),
        Color.Green => System.Drawing.Color.FromArgb(0, 1 + 12 * (int)currentBrightness, 0),
        _ /* blue */ => System.Drawing.Color.FromArgb(0, 0, 1 + 12 * (int)currentBrightness)
    };

    neoPixel.Image.SetPixel(0, 0, color);
    neoPixel.Update();
}

void sendButtons(ButtonInfo buttons)
{
    Debug.WriteLine($"Sending buttons {buttons}");
    var code = new byte[8];
    for (var i = 0; i < 8; i++)
    {
        code[i] = (buttons.Bits >> (7 - i)) == 1 ? (byte)'1' : (byte)'0';
    }
    client.Publish(Defaults.MqttTopic, code);
}

void connectToWifi()
{
    if (!WifiNetworkHelper.ConnectDhcp(Secrets.Wifi.Ssid, Secrets.Wifi.PreSharedKey))
    {
        fatalError($"Could not connect to WiFi network {Secrets.Wifi.Ssid}!");
    }

    var iface = NetworkInterface.GetAllNetworkInterfaces()[WifiAdapter.FindAllAdapters()[0].NetworkInterface];
    Console.WriteLine($"Connected to Wifi network {Secrets.Wifi.Ssid} with IP address {iface.IPv4Address}.");
}

MqttClient connectToMqtt()
{
    try
    {
        MqttClient client = new(Secrets.Mqtt.Host);
        var code = client.Connect(typeof(Program).Assembly.FullName, Secrets.Mqtt.UserName, Secrets.Mqtt.Password);

        if (code is not MqttReasonCode.Success)
        {
            fatalError($"Could not connect to MQTT server {Secrets.Mqtt.Host}! {code.GetName().Humanize()}");
        }

        return client;
    }
    catch (Exception ex)
    {
        fatalError($"Could not connect to MQTT server {Secrets.Mqtt.Host}!, {ex}");

        // not reached
        return default!;
    }
}

void fatalError(string message)
{
    Console.WriteLine(message);
    while (true)
    {
        led.Toggle();
        Thread.Sleep(500);
    }
}

enum Color { None, Red, Yellow, Green, Blue };
enum Brightness { Low, Medium, High};
