using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MYFirstMSMQ
{
    public class AirlineCompany
    {
        private MessageQueue inputQueue = new MessageQueue();

        /// <summary>
        /// Airlinecompany class which should just receive information from the aggregator at last.
        /// </summary>
        /// <param name="inputQueue"></param>
        public AirlineCompany(MessageQueue inputQueue) 
        {
            this.inputQueue = inputQueue;

            inputQueue.Formatter = new ActiveXMessageFormatter();
            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inputQueue.BeginReceive();
        }


        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            mq.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            Console.WriteLine("Received response from aggregator {0} \n", message.Body.ToString());
            mq.BeginReceive();
        }
    }
}
