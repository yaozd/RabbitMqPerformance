using System.ComponentModel;

namespace RabbitMQ.NETClient.Product
{
    public enum DeliveryMode : byte
    {
        [Description("消息非持久")]
        NonPersistent = 1,
        [Description("消息持久")]
        Persistent = 2
    }
}