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

        public void SendMessage(QueueName queueName, string message)
        {
            Execute(() => ProductMessage.SendMessage(_sendChannel, queueName, message));
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