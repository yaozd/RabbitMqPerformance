using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Customer
{
    public class CustomerMessage : ICustomerMessage
    {
        private readonly string _listentChannelGuid;
        private readonly QueueName _queueName;
        public CustomerMessage(QueueName queueName)
        {
            _queueName = queueName;
            _listentChannelGuid = Guid.NewGuid().ToString();
        }

        private IModel GetChannel()
        {
            return Singleton.Instance.MqCustomer().ListenChannel(_listentChannelGuid);
        }
        public Func<string, bool> ReceiveMessageCallback { get; set; }
        public uint MessageCount { get; }
        #region  开始监听消息
        /// <summary>
        /// 开始监听消息
        /// </summary>
        public void StartListening()
        {
            Task.Factory.StartNew(BasicConsume);
        }
        #endregion

        #region 消费消息
        /// <summary>
        /// 消费信息
        /// </summary>
        private void BasicConsume()
        {
            var queue = QueueNameHelper.Instance.Name(_queueName);
            var listenChannel = GetChannel();
            CustomerQueueDeclare.Instance.Check(listenChannel, _queueName);
            //公平分发,不要同一时间给一个工作者发送多于一个消息
            listenChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); ;
            //创建事件驱动的消费者类型，不要用下边的死循环来消费消息
            var consumer = new EventingBasicConsumer(listenChannel);
            consumer.Received += Consumer_Received;
            //消费消息是否自动删除
            const bool autoDeleteMessage = false;
            //接收消息
            listenChannel.BasicConsume(queue, autoDeleteMessage, consumer);
        }
        /// <summary>
        /// 接收到消息触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Consumer_Received(object sender, BasicDeliverEventArgs args)
        {
            try
            {
                var listenChannel = GetChannel();
                var body = args.Body;
                var message = Encoding.UTF8.GetString(body);
                //将消息业务处理交给外部业务
                bool result = Execute(() => ReceiveMessageCallback(message));
                //bool result = ReceiveMessageCallback(message);
                if (result)
                {
                    if (listenChannel != null && !listenChannel.IsClosed)
                    {
                        //确认机制--ACK机制
                        listenChannel.BasicAck(deliveryTag: args.DeliveryTag, multiple:false);
                    }
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
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
    }
}