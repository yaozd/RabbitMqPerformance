﻿using System;
using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Product
{
    /// <summary>
    /// 生产者
    /// 同一个线程只做一次队列声明
    /// 只有做了队列声明以后才可以发送队列消息（调用ProductMessage.SendMessage）
    /// </summary>
    public class ProductQueueDeclare
    {
        //锁--创建队列声明
        private static readonly object LockObject = new object();

        private static readonly Lazy<ProductQueueDeclare> lazy =
            new Lazy<ProductQueueDeclare>(() => new ProductQueueDeclare());

        public static ProductQueueDeclare Instance
        {
            get { return lazy.Value; }
        }

        private readonly Hashtable _ht = new Hashtable();

        /// <summary>
        /// 检查队列声明
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="isDurable"></param>
        public void Check(IModel channel, QueueName queueName, bool isDurable)
        {
            var val = _ht[queueName];
            if (val == null)
            {
                lock (LockObject)
                {
                    val = _ht[queueName];
                    if (val == null)
                    {
                        QueueDeclare(channel, queueName, isDurable);
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
        /// <param name="isDurable"></param>
        public void QueueDeclare(IModel channel, QueueName queueName, bool isDurable)
        {
            var queue = QueueNameHelper.Instance.Name(queueName);
            //消息持久化，防止丢失
            if (isDurable)
            {
                channel.QueueDeclare(queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                return;
            }
            //消息非持久化
            channel.QueueDeclare(queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
    }
}