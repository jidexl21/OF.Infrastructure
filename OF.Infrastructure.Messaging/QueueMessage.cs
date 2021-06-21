using System;
using System.Collections.Generic;
using System.Text;

namespace OF.Infrastructure.Messaging
{
    public class QueueMessage : IQueueMessage
    {
        public string EventID { get; set; } = Guid.NewGuid().ToString("N");
        public string Topic { get; set; }
        public string Message { get; set; }
        public PublishStatus Status { get; set; }
        public DateTime? DatePublished { get; set; }
    }
}
