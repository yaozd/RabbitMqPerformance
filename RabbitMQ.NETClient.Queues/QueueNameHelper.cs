using System;
using System.Collections;

namespace RabbitMQ.NETClient.Queues
{
    public sealed class QueueNameHelper
    {
        private static readonly Lazy<QueueNameHelper> lazy =
          new Lazy<QueueNameHelper>(() => new QueueNameHelper());
        public static QueueNameHelper Instance
        {
            get { return lazy.Value; }
        }
        private QueueNameHelper()
        {
            InitQueueNames();
        }

        private void InitQueueNames()
        {
            _ht = EnumToHashtable(typeof(QueueName));
        }

        private Hashtable _ht = new Hashtable();
        public string Name(QueueName name)
        {
            var val = _ht[name];
            if (val == null)
            {
                val = name.ToString("G");
                _ht[name] = val;
            }
            return val.ToString();
        }
        private Hashtable EnumToHashtable(Type enumType)
        {
            Hashtable ht = new Hashtable();
            string[] enumArr = Enum.GetNames(enumType);
            foreach (string name in enumArr)
            {
                ht.Add(Enum.Parse(enumType, name), name);
            }
            return ht;
        }
    }
}