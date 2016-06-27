using System;
using System.ComponentModel;
using RabbitMQ.Client;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Product
{
    public class RabbitMqProduct : IDisposable
    {
        /// <summary>
        /// 发送
        /// </summary>
        IConnection _sendConnection;

        /// <summary>
        /// 发送
        /// </summary>
        IModel _sendChannel;

        public RabbitMqProduct(string hostName)
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
            _sendConnection = factory.CreateConnection();
            _sendChannel = _sendConnection.CreateModel();
        }
        /// <summary>
        /// 消息持久化，防止丢失
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void SendMessagePersistent(QueueName queueName, string message)
        {
            SendMessage(queueName,message,true);
        }
        /// <summary>
        /// 消息非持久化，RabbitMq宕机后数据则会丢失
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void SendMessageNoPersistent(QueueName queueName, string message)
        {
            SendMessage(queueName, message, false);
        }
        /// <summary>
        /// 发送消息
        /// 使用消息确认机制和BasicQos 就可以建立工作队列，用持久化选项可以保证即使RabbitMQ重启也不会丢失任务。
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="isDurable">默认为true-消息持久化，防止丢失</param>
        public void SendMessage(QueueName queueName, string message,bool isDurable=true)
        {
            //消息持久化，防止丢失
            //var isDurable = true;
            Execute(() => ProductMessage.SendMessage(_sendChannel, queueName, message, isDurable));
        }

        #region Execute

        /// <summary>
        /// Executes the specified expression. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private T Execute<T>(Func<T> action)
        {

            DateTime before = DateTime.Now;
            //Log.DebugFormat("Executing action '{0}'", action.Method.Name);

            try
            {
                T result = action();
                TimeSpan timeTaken = DateTime.Now - before;
                //if (Log.IsDebugEnabled)
                //    Log.DebugFormat("Action '{0}' executed. Took {1} ms.", action.Method.Name, timeTaken.TotalMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                Display("There was an error executing Action '{0}'. Message: {1}", action.Method.Name, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Executes the specified action (for void methods).
        /// </summary>
        /// <param name="action">The action.</param>
        private void Execute(Action action)
        {
            DateTime before = DateTime.Now;
            //Log.DebugFormat("Executing action '{0}'", action.Method.Name);

            try
            {
                action();
                TimeSpan timeTaken = DateTime.Now - before;
                //if (Log.IsDebugEnabled)
                //    Log.DebugFormat("Action '{0}' executed. Took {1} ms.", action.Method.Name, timeTaken.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Display("There was an error executing Action '{0}'. Message: {1}", action.Method.Name, ex.Message);
                throw;
            }
        }

        void Display(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        #endregion

        public void Dispose()
        {
            if (_sendChannel != null)
            {
                _sendChannel.Dispose();
                _sendChannel = null;
            }
            if (_sendConnection != null)
            {
                _sendConnection.Dispose();
                _sendConnection = null;
            }
        }
    }
}