using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.NETClient.Customer;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Server
{
    /// <summary>
    /// 默认情况下生成的消息队列：
    /// 队列的持久化
    /// 消息的持久化
    /// RabbitMQ指南(C#)(二)工作队列
    /// http://www.cnblogs.com/lpush/p/5537289.html
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //测试方法
            DoWork();
            Console.Read();
            return;
            const int ThreadCount = 10;
            QueueName queueName = QueueName.HelloWorld;
            //Example(ThreadCount);

            ParalleTask(ThreadCount, queueName);
            Console.Read();
        }

        /// <summary>
        /// 并行任务
        /// </summary>
        /// <param name="threadCount"></param>
        /// <param name="queueName"></param>
        private static void ParalleTask(int threadCount, QueueName queueName)
        {
            Parallel.For(0, threadCount, i =>
            {
                ICustomerMessage customer = new CustomerMessage(queueName);
                customer.StartListening();
                customer.ReceiveMessageCallback = message =>
                {

                    //TODO 业务处理逻辑
                    try
                    {
                        //TODO 业务出现异常情况下的业务逻辑
                    }
                    catch (Exception)
                    {
                        //消息队列--这里是要吃掉异常的
                        //TODO 业务出现异常情况下的业务逻辑
                        //throw;
                    }
                    Console.WriteLine("接收到消息：" + message);
                    //参考：.Net下RabbitMQ的使用(3) -- 竞争的消费者
                    //http://www.cnblogs.com/haoxinyue/archive/2012/09/26/2703964.html
                    //生产环境中
                    //注意：不管业务处理逻辑是否正确
                    //TODO 这个地方必须返回为true,如果不返回为True的话，当前的客户端无法再获取下一条消息
                    //return true
                    //return true;
                };
            });
        }
        /// <summary>
        /// 例子
        /// </summary>
        /// <param name="ThreadCount"></param>
        private static void Example(int ThreadCount)
        {
            DoWork();
            Parallel.For(0, ThreadCount, i => { DoWork(); });
        }
        private static void DoWork()
        {
            ICustomerMessage customer = new CustomerMessage(QueueName.HelloWorld);
            customer.StartListening();
            customer.ReceiveMessageCallback = message =>
            {
                //TODO 业务处理逻辑
                try
                {
                    //TODO 业务出现异常情况下的业务逻辑
                }
                catch (Exception)
                {
                    //消息队列--这里是要吃掉异常的
                    //TODO 业务出现异常情况下的业务逻辑
                    //throw;
                }
                //return false;
                //Console.WriteLine("接收到消息：" + message);
                //return true;
            };
        }
    }
}
