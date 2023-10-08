using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MYFirstMSMQ
{
    /// <summary>
    /// Person information class to simulate processing personinformation
    /// We could apply some extra information to the message before sending it again
    /// However I have not done that, I've just written a Console.WriteLine("Simulating Person Information alteration");
    /// </summary>
    public class PersonInformation
    {
        private MessageQueue inputQueue;
        private MessageQueue outputQueue;

        
        public PersonInformation(MessageQueue inputQueue, MessageQueue outputQueue)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            inputQueue.MessageReadPropertyFilter.AppSpecific = true;
            inputQueue.MessageReadPropertyFilter.CorrelationId = true;


            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inputQueue.BeginReceive();
        }
        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            Console.WriteLine("Person handling" + message.CorrelationId);
            Console.WriteLine("Simulating Person Information alteration");
            outputQueue.Send(message);
            

            mq.BeginReceive();
        }
    }
}
