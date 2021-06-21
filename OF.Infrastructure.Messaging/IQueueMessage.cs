using System;
using System.Collections.Generic;
using System.Text;

namespace OF.Infrastructure.Messaging
{
    public interface IQueueMessage
    {
        string EventID { get; set; }
        string Topic { get; set; }
        string Message { get; set; }
        PublishStatus Status { get; set; }
        DateTime? DatePublished { get; set; }
    }
}
