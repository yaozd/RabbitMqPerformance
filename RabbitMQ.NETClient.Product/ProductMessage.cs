using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Product
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductMessage
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="isDurable"></param>
        public static void SendMessage(IModel channel, QueueName queueName, string message,bool isDurable)
        {
            ProductQueueDeclare.Instance.Check(channel, queueName, isDurable);
            Publish(channel, queueName, message, isDurable);
        }

        private static void Publish(IModel channel, QueueName queueName, string message, bool isDurable)
        {
            var buffer = StringToBytes(message);
            var routingKey = QueueNameHelper.Instance.Name(queueName);
            //消息持久化，防止丢失
            if (isDurable)
            {
                //设置消息为持久化
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;
                channel.BasicPublish(exchange: "",
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: buffer);
                return;
            }
            //消息非持久化
            channel.BasicPublish(exchange: "",
                routingKey: routingKey,
                basicProperties: null,
                body: buffer);
        }

        private static byte[] StringToBytes(string message)
        {
            //字符串转化为Byte
            //参考Beetle.Redis.StringEncoding
            byte[] buffer = new byte[message.Length];
            Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
            return buffer;
        }
    }
}