using System;
using System.Messaging;

namespace MYFirstMSMQ
{
    /// <summary>
    /// Processer class
    /// Works as a template for the resequencer
    /// Following the 'L' in the SOLID principles
    /// </summary>
    public class Processor
    {
        protected MessageQueue inputQueue;
        protected MessageQueue outputQueue;
        public Processor(MessageQueue inputQueue, MessageQueue outputQueue)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            
            inputQueue.MessageReadPropertyFilter.CorrelationId = true;
            inputQueue.MessageReadPropertyFilter.Label = true;
            
            Console.WriteLine("Processing Messages from " + inputQueue.Path + " to " + outputQueue.Path);

            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
            inputQueue.BeginReceive();
        }
        
        /// <summary>
        /// Basic OnMessage/Onreceive method that almost all classes implement
        /// </summary>
        /// <param name="source"></param>
        /// <param name="asyncResult"></param>
        private void OnReceiveCompleted(Object source, ReceiveCompletedEventArgs asyncResult)
        {

            MessageQueue mq = (MessageQueue)source;
            Message m = mq.EndReceive(asyncResult.AsyncResult);
            ProcessMessage(m);

            mq.BeginReceive();
        }

        protected virtual void ProcessMessage(Message m)
        {
            string body = (string)m.Body;
            Console.WriteLine("Received Message: " + body);
            outputQueue.Send(body);
        }
    }
}
