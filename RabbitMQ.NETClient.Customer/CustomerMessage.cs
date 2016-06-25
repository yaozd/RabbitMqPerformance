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
        public Action<String> ReceiveMessageCallback { get; set; }
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
                bool result = ExecuteReciveFun(() => ReceiveMessageCallback(message));
                //bool result = ReceiveMessageCallback(message);
                if (result)
                {
                    if (listenChannel != null && !listenChannel.IsClosed)
                    {
                        //确认消息已经成做了处理机制--ACK机制
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

        #region 
        /// <summary>
        /// 所有的业务逻辑处理都返回结果为true
        /// 这样就不会造成消息处理的阻塞
        /// 参考：
        /// .Net下RabbitMQ的使用(3) -- 竞争的消费者
        /// http://www.cnblogs.com/haoxinyue/archive/2012/09/26/2703964.html
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool ExecuteReciveFun(Action action)
        {

            DateTime before = DateTime.Now;
            //Log.DebugFormat("Executing action '{0}'", action.Method.Name);

            try
            {
                action();
                TimeSpan timeTaken = DateTime.Now - before;
                //if (Log.IsDebugEnabled)
                //    Log.DebugFormat("Action '{0}' executed. Took {1} ms.", action.Method.Name, timeTaken.TotalMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                //TODO 记录错误日志
                Display("There was an error executing Action '{0}'. Message: {1}", action.Method.Name, ex.Message);
                //参考：.Net下RabbitMQ的使用(3) -- 竞争的消费者
                //http://www.cnblogs.com/haoxinyue/archive/2012/09/26/2703964.html
                //生产环境中
                //注意：不管业务处理逻辑是否正确
                //TODO 这个地方必须返回为true,如果不返回为True的话，当前的客户端无法再获取下一条消息,造成消息处理的阻塞
                return true;
            }
        }
        /// <summary>
        /// 开发环境下做显示输出
        /// 生产环境这里是用来记录错误日志。
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        void Display(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }
        #endregion       
    }
}