using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Burkenyo.Iot.Server;

interface IMqttClientAuthenticator
{
    Task<MqttConnectReasonCode> AuthenticateUserAsync(string clientId, string userName, string password);
}

interface IMqttMessageProcessor
{
    Task<MqttPubAckReasonCode> ProcessMessageAsync(MqttApplicationMessage message);
}

static class MqttWebAppExtensions
{
    public static IApplicationBuilder UseMqttClientAuthenticator<TAuthenticator>(this IApplicationBuilder builder)
        where TAuthenticator : class, IMqttClientAuthenticator
    {
        var authenticator = builder.ApplicationServices.GetRequiredService<TAuthenticator>();
        var logger = builder.ApplicationServices.GetRequiredService<ILogger<MqttServer>>();

        builder.ApplicationServices.GetRequiredService<MqttServer>().ValidatingConnectionAsync += async args =>
        {
            try
            {
                args.ReasonCode = await authenticator.AuthenticateUserAsync(args.ClientId, args.UserName, args.Password);
            }
            catch (Exception ex)
            {
                args.ReasonCode = MqttConnectReasonCode.UnspecifiedError;
                logger.LogError(ex, "MQTT Client authentication failed!");
            }
        };

        return builder;
    }

    public static IApplicationBuilder UseMqttMessageProcessor<TProcessor>(this IApplicationBuilder builder)
        where TProcessor : class, IMqttMessageProcessor
    {
        var processor = builder.ApplicationServices.GetRequiredService<TProcessor>();
        var logger = builder.ApplicationServices.GetRequiredService<ILogger<MqttServer>>();

        builder.ApplicationServices.GetRequiredService<MqttServer>().InterceptingPublishAsync += async args =>
        {
            try
            {
                args.ProcessPublish = false;
                args.Response.ReasonCode = await processor.ProcessMessageAsync(args.ApplicationMessage);
            }
            catch (Exception ex)
            {
                args.Response.ReasonCode = MqttPubAckReasonCode.UnspecifiedError;
                logger.LogError(ex, "MQTT Message processing failed!");
            }
        };

        return builder;
    }
}
