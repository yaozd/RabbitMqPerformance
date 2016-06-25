using System;

namespace RabbitMQ.NETClient.Customer
{
    public interface ICustomerMessage
    {
        Action<String> ReceiveMessageCallback { get; set; }
        uint MessageCount { get; }
        void StartListening();
    }
}