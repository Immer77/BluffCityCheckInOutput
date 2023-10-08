using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MYFirstMSMQ
{
    /// <summary>
    /// Interface that aggregates should implement
    /// </summary>
    internal interface Aggregate
    {
        void AddMessage(Message message);
        bool IsComplete();
        Message GetResultMessage();
    }
}
