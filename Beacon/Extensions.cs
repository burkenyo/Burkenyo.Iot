using nanoFramework.M2Mqtt.Messages;
using System.Device.Wifi;
using System.Text.RegularExpressions;

namespace Blinky;

static class EnumExtensions
{
    public static string GetName(this WifiConnectionStatus status) =>
        status switch
        {
            WifiConnectionStatus.AccessRevoked => nameof(WifiConnectionStatus.AccessRevoked),
            WifiConnectionStatus.InvalidCredential => nameof(WifiConnectionStatus.InvalidCredential),
            WifiConnectionStatus.NetworkNotAvailable => nameof(WifiConnectionStatus.NetworkNotAvailable),
            WifiConnectionStatus.Success => nameof(WifiConnectionStatus.Success),
            WifiConnectionStatus.Timeout => nameof(WifiConnectionStatus.Timeout),
            WifiConnectionStatus.UnspecifiedFailure => nameof(WifiConnectionStatus.UnspecifiedFailure),
            WifiConnectionStatus.UnsupportedAuthenticationProtocol => nameof(WifiConnectionStatus.UnsupportedAuthenticationProtocol),
            _ => status.ToString()
        };

    public static string GetName(this MqttReasonCode code) =>
        code switch
        {
            MqttReasonCode.Success => nameof(MqttReasonCode.Success),
            // Connect errors (but note that these overlap some legitimate "OK" codes)
            MqttReasonCode.ConnectionRefusedUnacceptableProtocolVersion => nameof(MqttReasonCode.ConnectionRefusedUnacceptableProtocolVersion),
            MqttReasonCode.ConnectionRefusedIdentifierRejected => nameof(MqttReasonCode.ConnectionRefusedIdentifierRejected),
            MqttReasonCode.ConnectionRefusedServerUnavailable => nameof(MqttReasonCode.ConnectionRefusedServerUnavailable),
            MqttReasonCode.ConnectionRefusedBadUserNameOrPassword => nameof(MqttReasonCode.ConnectionRefusedBadUserNameOrPassword),
            MqttReasonCode.ConnectionRefusedNotAuthorized => nameof(MqttReasonCode.ConnectionRefusedNotAuthorized),
            // Information
            MqttReasonCode.NoMatchingSubscribers => nameof(MqttReasonCode.NoMatchingSubscribers),
            MqttReasonCode.NoSubscriptionExisted => nameof(MqttReasonCode.NoSubscriptionExisted),
            MqttReasonCode.ContinueAuthentication => nameof(MqttReasonCode.ContinueAuthentication),
            MqttReasonCode.ReAuthenticate => nameof(MqttReasonCode.ReAuthenticate),
            // Errors
            MqttReasonCode.UnspecifiedError => nameof(MqttReasonCode.UnspecifiedError),
            MqttReasonCode.MalformedPacket => nameof(MqttReasonCode.MalformedPacket),
            MqttReasonCode.ProtocolError => nameof(MqttReasonCode.ProtocolError),
            MqttReasonCode.ImplementationSpecificError => nameof(MqttReasonCode.ImplementationSpecificError),
            MqttReasonCode.UnsupportedProtocolVersion => nameof(MqttReasonCode.UnsupportedProtocolVersion),
            MqttReasonCode.ClientIdentifierNotValid => nameof(MqttReasonCode.ClientIdentifierNotValid),
            MqttReasonCode.BadUserNameOrPassword => nameof(MqttReasonCode.BadUserNameOrPassword),
            MqttReasonCode.NotAuthorized => nameof(MqttReasonCode.NotAuthorized),
            MqttReasonCode.ServerUnavailable => nameof(MqttReasonCode.ServerUnavailable),
            MqttReasonCode.ServerBusy => nameof(MqttReasonCode.ServerBusy),
            MqttReasonCode.Banned => nameof(MqttReasonCode.Banned),
            MqttReasonCode.ServeShuttingDown => nameof(MqttReasonCode.ServeShuttingDown),
            MqttReasonCode.BadAuthenticationMethod => nameof(MqttReasonCode.BadAuthenticationMethod),
            MqttReasonCode.KeepAliveTimeout => nameof(MqttReasonCode.KeepAliveTimeout),
            MqttReasonCode.SessionTakenOver => nameof(MqttReasonCode.SessionTakenOver),
            MqttReasonCode.TopicFilterInvalid => nameof(MqttReasonCode.TopicFilterInvalid),
            MqttReasonCode.TopicNameInvalid => nameof(MqttReasonCode.TopicNameInvalid),
            MqttReasonCode.PacketIdentifierInUse => nameof(MqttReasonCode.PacketIdentifierInUse),
            MqttReasonCode.PacketIdentifierNotFound => nameof(MqttReasonCode.PacketIdentifierNotFound),
            MqttReasonCode.ReceiveMaximumExceeded => nameof(MqttReasonCode.ReceiveMaximumExceeded),
            MqttReasonCode.TopicAliasInvalid => nameof(MqttReasonCode.TopicAliasInvalid),
            MqttReasonCode.PacketTooLarge => nameof(MqttReasonCode.PacketTooLarge),
            MqttReasonCode.MessageRateTooHigh => nameof(MqttReasonCode.MessageRateTooHigh),
            MqttReasonCode.QuotaExceeded => nameof(MqttReasonCode.QuotaExceeded),
            MqttReasonCode.AdministrativeAction => nameof(MqttReasonCode.AdministrativeAction),
            MqttReasonCode.PayloadFormatInvalid => nameof(MqttReasonCode.PayloadFormatInvalid),
            MqttReasonCode.RetainNotSupported => nameof(MqttReasonCode.RetainNotSupported),
            MqttReasonCode.QoSNotSupported => nameof(MqttReasonCode.QoSNotSupported),
            MqttReasonCode.UseAnotherServer => nameof(MqttReasonCode.UseAnotherServer),
            MqttReasonCode.ServerMoved => nameof(MqttReasonCode.ServerMoved),
            MqttReasonCode.SharedSubscriptionsNotSupported => nameof(MqttReasonCode.SharedSubscriptionsNotSupported),
            MqttReasonCode.ConnectionRateExceeded => nameof(MqttReasonCode.ConnectionRateExceeded),
            MqttReasonCode.MaximumConnectTime => nameof(MqttReasonCode.MaximumConnectTime),
            MqttReasonCode.SubscriptionIdentifiersNotSupported => nameof(MqttReasonCode.SubscriptionIdentifiersNotSupported),
            MqttReasonCode.WildcardSubscriptionsNotSupported => nameof(MqttReasonCode.WildcardSubscriptionsNotSupported),
            _ => code.ToString()
        };
}

static class StringExtensions
{
    readonly static Regex s_pascalCase = new("([a-z])([A-Z])");

    public static string Humanize(this string value) =>
        s_pascalCase.Replace(value, "$1 $2");
}
