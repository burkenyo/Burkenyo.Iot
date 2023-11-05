using MQTTnet.Protocol;

namespace Burkenyo.Iot.Server;

class MqttClientAuthenticator(
    IConfiguration configuration,
    ILogger<MqttClientAuthenticator> logger
) : IMqttClientAuthenticator
{
    public Task<MqttConnectReasonCode> AuthenticateUserAsync(string clientId, string userName, string password)
    {
        logger.LogInformation("Connection request from {}", clientId);

        if (configuration["Client:UserName"] == userName && configuration["Client:Password"] == password)
        {
            return Task.FromResult(MqttConnectReasonCode.Success);
        }

        logger.LogWarning("Authentication failed for user {}", userName);

        return Task.FromResult(MqttConnectReasonCode.BadUserNameOrPassword);
    }
}