using System;
using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Customer
{
    /// <summary>
    /// 消费者
    /// 同一个线程只做一次队列声明
    /// 只有做了队列声明以后才可以发送队列消息（调用CustomerMessage.ReceiveMessage）
    /// </summary>
    public class CustomerQueueDeclare
    {
        //锁--创建队列声明
        private static readonly object LockObject = new object();

        private static readonly Lazy<CustomerQueueDeclare> lazy =
            new Lazy<CustomerQueueDeclare>(() => new CustomerQueueDeclare());

        public static CustomerQueueDeclare Instance
        {
            get { return lazy.Value; }
        }

        private readonly Hashtable _ht = new Hashtable();

        /// <summary>
        /// 检查队列声明
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        public void Check(IModel channel, QueueName queueName)
        {
            var val = _ht[queueName];
            if (val == null)
            {
                lock (LockObject)
                {
                    val = _ht[queueName];
                    if (val == null)
                    {
                        QueueDeclare(channel, queueName);
                        _ht[queueName] = true;
                    }
                }
            }
        }

        /// <summary>
        /// 所有的队列都做了相同的声明
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        public void QueueDeclare(IModel channel, QueueName queueName)
        {
            var queue = QueueNameHelper.Instance.Name(queueName);
            channel.QueueDeclare(queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
    }
}