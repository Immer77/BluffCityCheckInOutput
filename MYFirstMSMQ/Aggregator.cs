using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;


namespace MYFirstMSMQ
{
    /// <summary>
    /// Aggregator class to receieve and gather messages with the same correlation ID
    /// </summary>
    internal class Aggregator
    {
        public MessageQueue inputQueue;
        public MessageQueue outputQueue;
        // ActiveCheckin for the correlation ID key and Aggregate value
        private IDictionary activeCheckin = new Hashtable();
        public Aggregator(MessageQueue inputQueue, MessageQueue outputQueue)
        {
            this.inputQueue = inputQueue;
            inputQueue.MessageReadPropertyFilter.AppSpecific = true;
            inputQueue.MessageReadPropertyFilter.Body = true;
            inputQueue.MessageReadPropertyFilter.CorrelationId = true;
            inputQueue.MessageReadPropertyFilter.Id = true;
            inputQueue.MessageReadPropertyFilter.Label = true;
            this.outputQueue = outputQueue;

            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inputQueue.BeginReceive();
        }

        /// <summary>
        /// On message we receive we first check whether the aggregate already exists if it doesn't we create a new aggregate with the correlation ID as key
        /// if it already exists we add it to the list of messages located on the aggregate.
        /// We could also have made the PassengerAggregate to a nested class since this is the only place we use it, however for simplicity sake I've decided to separete it into another class.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="asyncResult"></param>
        public void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult) 
        {
            
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            Console.WriteLine("Received message");

            PassengerAggregate aggregate = (PassengerAggregate)activeCheckin[message.CorrelationId];
            if(aggregate == null )
            {                
                int size = Convert.ToInt32(message.AppSpecific);
                aggregate = new PassengerAggregate(size);
                activeCheckin.Add(message.CorrelationId, aggregate);
            }

            if(!aggregate.IsComplete())
            {
                aggregate.AddMessage(message);
                if(aggregate.IsComplete())
                {
                    outputQueue.Send(aggregate.GetResultMessage());
                }
            }
            mq.BeginReceive();
        }
    }
}
