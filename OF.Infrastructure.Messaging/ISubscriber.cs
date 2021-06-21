using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Messaging
{
    public interface ISubscriber
    {
        Task ListenAsync();
    }
}
