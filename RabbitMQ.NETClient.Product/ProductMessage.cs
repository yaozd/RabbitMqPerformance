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
        public static void SendMessage(IModel channel, QueueName queueName, string message)
        {
            ProductQueueDeclare.Instance.Check(channel, queueName);
            var buffer = StringToBytes(message);
            channel.BasicPublish(exchange: "",
                routingKey: QueueNameHelper.Instance.Name(queueName),
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