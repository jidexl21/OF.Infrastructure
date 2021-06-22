using OF.Infrastructure.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace OF.Infrastructure.Messaging.Commands
{
    public abstract class Command : Message
    {
        public DateTime TimeStamp { get; protected set; }
    }
}
