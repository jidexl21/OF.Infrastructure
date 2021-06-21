using System;
using System.Collections.Generic;
using System.Text;

namespace OF.Infrastructure.Messaging
{
    public class Utility
    {
        public static  IQueueMessage CreateMessage(string Topic, string Message)
        {
            return new QueueMessage
            {
                Message = Message,
                Topic = Topic
            };
        }
    }
}
