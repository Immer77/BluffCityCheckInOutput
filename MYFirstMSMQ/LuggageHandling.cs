using System;
using System.Collections.Generic;
using System.Messaging;

namespace MYFirstMSMQ
{
    /// <summary>
    /// Luggagehandling class to make sure we scramble the packages before sending it to the resequencer
    /// </summary>
    internal class LuggageHandling
    {
        protected MessageQueue inputQueue;
        protected MessageQueue outputQueue;
        // We use a stack since we want to put the message number 1 first and the 2nd last, that way, when we pop the message we get the 2nd first to send to the resequencer
        protected Stack<Message> messages;

        public LuggageHandling(MessageQueue inputQueue, MessageQueue outputQueue) 
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;
            inputQueue.MessageReadPropertyFilter.AppSpecific = true;
            inputQueue.MessageReadPropertyFilter.CorrelationId = true;
            inputQueue.MessageReadPropertyFilter.Label = true;
            messages = new Stack<Message>();
            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inputQueue.BeginReceive();

        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult) 
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);

            Console.WriteLine("Luggage handling: " + message.CorrelationId);
            int amountOfMessages = Convert.ToInt32(message.Label.Split('-')[1]);
            // Setting up to store the messages so that it can send them back in the wrong order to make sure our resequencer works
            messages.Push(message);


            if(messages.Count == amountOfMessages)
            {
                for(int i = messages.Count; i > 0; i--)
                {
                    Message sendMessage = messages.Pop();
                    Console.WriteLine("Furthering Message from luggage Handling message number: {0}", sendMessage.Label);
                    outputQueue.Send(sendMessage);
                }             
            }

            mq.BeginReceive();
        }

    }
}
