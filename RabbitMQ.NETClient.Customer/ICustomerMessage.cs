using System;

namespace RabbitMQ.NETClient.Customer
{
    public interface ICustomerMessage
    {
        Func<string, bool> ReceiveMessageCallback { get; set; }
        uint MessageCount { get; }
        void StartListening();
    }
}