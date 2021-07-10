
using System;
using System.Collections.Generic;
using System.Text;

namespace OF.Infrastructure.Messaging.Events
{
    public abstract class Message 
    {
        public string MessageType { get; protected set; }
        protected Message() {
            MessageType = GetType().Name;
        }
    }
}
