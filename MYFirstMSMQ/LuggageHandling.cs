using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MYFirstMSMQ
{
    internal class LuggageHandling
    {
        protected MessageQueue inputQueue;
        protected MessageQueue outputQueue;

        public LuggageHandling(MessageQueue inputQueue, MessageQueue outputQueue) 
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;

            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inputQueue.BeginReceive();
        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult) 
        {
            MessageQueue mq = (MessageQueue)source;
            mq.Formatter = new ActiveXMessageFormatter();
            Message message = mq.EndReceive(asyncResult.AsyncResult);

            if(message != null)
            {
                outputQueue.Send(message);
            }

            mq.BeginReceive();
        }

    }
}
