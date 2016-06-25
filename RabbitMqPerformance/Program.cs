using System;
using System.Threading;
using RabbitMQ.NETClient.Product;
using RabbitMQ.NETClient.Queues;

namespace RabbitMqPerformance
{
    /// <summary>
    /// Product--测试生产者客户端
    /// </summary>
    internal class Program
    {
        //原子计数
        public static int Count;
        //错误原子计数
        public static int ErrorCount;
        //测试数据
        public static byte[] TestData;
        public static string TestData2;
        public static int TestDataSize = 1*1024;
        public static string LogPath = @"D:\temp-test";
        public static string LogName = "log-RabbitMq.txt";

        private static void Main(string[] args)
        {
            var message = "Hello World!" + Math.Round(Convert.ToDouble(DateTime.Now.Ticks));
            Init();
            //
            while (true)
            {
                try
                {
                    PerformanceTest.Time("Test_Set", 400, 1000, Test_Set);
                    //PerformanceTest.Time("Test_Failover", 1, 1000, (Test_Failover));
                }
                catch (Exception ex)
                {
                    //记录报错信息
                    var current = Interlocked.Increment(ref ErrorCount);
                    Console.WriteLine("2-Error-Count-{0}-{1}", current, DateTime.Now);
                    Console.WriteLine("2-Error-Count-{0}-{1}", current, ex.Message);
                }
            }
            //
            End();
        }

        private static void Test_Set()
        {
            //记录报错信息
            var current = Interlocked.Increment(ref Count);
            if (current%2 == 0)
            {
                //
                Singleton.Instance.MqProduct().SendMessage(QueueName.HelloWorld, TestData2 + "HelloWorld");
                Singleton.Instance.MqProduct().SendMessage(QueueName.LoveYou, TestData2 + "LoveYou");
            }
            else
            {
                //
                Singleton.Instance.MqProduct().SendMessage(QueueName.LoveYou, TestData2 + "LoveYou");
                Singleton.Instance.MqProduct().SendMessage(QueueName.HelloWorld, TestData2 + "HelloWorld");
            }
        }

        private static void Test_Get()
        {
        }

        private static void Test_Failover()
        {
            //
            try
            {
                Thread.Sleep(1000);
                Singleton.Instance.MqProduct().SendMessage(QueueName.HelloWorld, TestData2 + "HelloWorld");
            }
            catch (Exception ex)
            {
                //记录报错信息
                var current = Interlocked.Increment(ref ErrorCount);
                Console.WriteLine("2-Error-Count-{0}-{1}", current, DateTime.Now);
                Console.WriteLine("2-Error-Count-{0}-{1}", current, ex.Message);
            }
        }

        private static void Example()
        {
            var current = Interlocked.Increment(ref Count);
            Logger.Info(current.ToString());
        }

        private static void Example_Loop()
        {
            while (true)
            {
                try
                {
                    //
                    PerformanceTest.Time("Test_Set", 40, 5000, Test_Set);
                    PerformanceTest.Time("Test_Get", 40, 5000, Test_Get);
                    //
                }
                catch (Exception ex)
                {
                    //记录报错信息
                    var current = Interlocked.Increment(ref ErrorCount);
                    Logger.Info(string.Format("2-Error-Count-{0}-{1}", current, DateTime.Now));
                    Logger.Info(string.Format("2-Error-Count-{0}-{1}", current, ex.Message));
                }
            }
        }

        private static void Init()
        {
            Logger.Initialize(LogPath, LogName);
            PerformanceTest.Initialize();
            BuildTestData();
            BuildTestData2();
        }

        private static void End()
        {
            Console.WriteLine("Test End");
            Logger.Close();
            Console.Read();
        }

        private static void BuildTestData()
        {
            TestData = new byte[TestDataSize];
            for (var i = 0; i < TestDataSize; i++)
            {
                TestData[i] = 0x30;
            }
        }

        private static void BuildTestData2()
        {
            for (var i = 0; i < TestDataSize; i++)
            {
                TestData2 += "1";
            }
        }
    }
}