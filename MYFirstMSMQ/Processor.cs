using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MYFirstMSMQ
{
    
    
    public class Processor
    {
        protected MessageQueue inputQueue;
        protected MessageQueue outputQueue;
        public Processor(MessageQueue inputQueue, MessageQueue outputQueue)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;

            inputQueue.Formatter = new ActiveXMessageFormatter();
            inputQueue.MessageReadPropertyFilter.ClearAll();
            inputQueue.MessageReadPropertyFilter.AppSpecific = true;
            inputQueue.MessageReadPropertyFilter.Body = true;
            inputQueue.MessageReadPropertyFilter.CorrelationId = true;
            inputQueue.MessageReadPropertyFilter.Id = true;
            Console.WriteLine("Processing Messages from " + inputQueue.Path + " to " + outputQueue.Path);

            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
            inputQueue.BeginReceive();
        }
        public void Process()
        {
            
        }
        private void OnReceiveCompleted(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;

            Message m = mq.EndReceive(asyncResult.AsyncResult);
            m.Formatter = new ActiveXMessageFormatter();
            ProcessMessage(m);
        }

        protected virtual void ProcessMessage(Message m)
        {
            string body = (string)m.Body;
            Console.WriteLine("Received Message: " + body);
            outputQueue.Send(body);
        }
    }
}
