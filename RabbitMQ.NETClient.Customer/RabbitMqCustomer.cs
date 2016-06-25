using System;
using System.Collections;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Customer
{
    /// <summary>
    /// 官方的见意：
    /// 公用一个TCP
    /// 每个queue(队列)对应一个channel
    /// </summary>
    public class RabbitMqCustomer : IDisposable
    {
        /// <summary>
        /// 接收--监听
        /// </summary>
        private IConnection _listenConnection;

        //锁--创建Channel
        private static readonly object LockObject = new object();

        private readonly Hashtable _htChannel = new Hashtable();

        /// <summary>
        /// 接收--监听
        /// 共享连接
        /// 独享通道--为每一个消费者的队列创建一个Channel
        /// </summary>
        /// <param name="_listentChannelGuid"></param>
        /// <returns></returns>
        public IModel ListenChannel(string _listentChannelGuid)
        {
            var val = _htChannel[_listentChannelGuid];
            if (val == null)
            {
                lock (LockObject)
                {
                    val = _htChannel[_listentChannelGuid];
                    if (val == null)
                    {
                        val = _listenConnection.CreateModel();
                        _htChannel[_listentChannelGuid] = val;
                    }
                }
            }
            return (IModel) val;
        }

        public RabbitMqCustomer(string hostName)
        {
            InitRabbitMqClients(hostName);
        }

        private void InitRabbitMqClients(string hostName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                RequestedHeartbeat = 60,
                AutomaticRecoveryEnabled = true
            };
            _listenConnection = factory.CreateConnection();
            //连接关闭-记录日志
            _listenConnection.ConnectionShutdown += Connection_ConnectionShutdown;
            //
        }

        /// <summary>
        /// 监听连接关闭-记录日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            /// TODO 监听连接关闭-记录日志
            Console.WriteLine("Connection_ConnectionShutdown");
        }

        public void Dispose()
        {
            for (int i = 0; i < _htChannel.Count; i++)
            {
                try
                {
                    var listenChannel = (IModel) _htChannel[0];
                    if (listenChannel != null)
                    {
                        listenChannel.Dispose();
                        listenChannel = null;
                    }
                }
                catch (Exception)
                {
                }

            }
            if (_listenConnection != null)
            {
                _listenConnection.Dispose();
                _listenConnection = null;
            }
        }
    }
}