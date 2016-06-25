using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.NETClient.Customer;
using RabbitMQ.NETClient.Queues;

namespace RabbitMQ.NETClient.Server
{
    class Program
    {
        static void Main(string[] args)
        {
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
                    //TODO 业务处程序
                    Console.WriteLine("接收到消息：" + message);
                    return true;
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
            ICustomerMessage customer = new CustomerMessage(QueueName.LoveYou);
            customer.StartListening();
            customer.ReceiveMessageCallback = message =>
            {
                //Console.WriteLine("接收到消息：" + message);
                return true;
            };
        }
    }
}
