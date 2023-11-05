using System.Globalization;
using Burkenyo.Iot.Shared;
using MQTTnet;
using MQTTnet.Protocol;

namespace Burkenyo.Iot.Server;

class ButtonPressEventProducer(
    ILogger<ButtonPressEventProducer> logger
) : IMqttMessageProcessor
{
    public event Func<ButtonInfo, Task>? ButtonsPressed;

    public Task<MqttPubAckReasonCode> ProcessMessageAsync(MqttApplicationMessage message)
    {
        if (message.Topic != Defaults.MqttTopic)
        {
            return Task.FromResult(MqttPubAckReasonCode.TopicNameInvalid);
        }

        if (!byte.TryParse(message.PayloadSegment, NumberStyles.AllowBinarySpecifier, CultureInfo.InvariantCulture, out var bits))
        {
            return Task.FromResult(MqttPubAckReasonCode.PayloadFormatInvalid);
        }

        var buttons = new ButtonInfo(bits);
        logger.LogDebug("Buttons received {:yyyy-MM-dd 'at' HH:mm:ss.fff}: {}", DateTime.Now, buttons);
        InvokeEvent(buttons);

        return Task.FromResult(MqttPubAckReasonCode.Success);
    }

    async void InvokeEvent(ButtonInfo buttons)
    {
        // Use async void to prevent event from blocking each other
        if (ButtonsPressed is not null)
        {
            try
            {
                await ButtonsPressed.InvokeMulti(buttons);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process button presses!");
            }
        }
    }
}