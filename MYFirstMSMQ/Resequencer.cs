using System;
using System.Collections;
using System.Messaging;

namespace MYFirstMSMQ
{
    /// <summary>
    /// The resenquencer class functionality is to take the packages that it has receieved and order them correctly
    /// Example: We receive luggage in an order where we get the 2nd package first and the 1st package last. We need to make sure that the 1st package is sent first
    /// </summary>
    internal class Resequencer : Processor
    {

        /// <summary>
        /// This is the setup of our buffer
        /// </summary>
        private int startIndex = 1;
        private IDictionary buffer = new Hashtable();
        private int endIndex = -1;

        /// <summary>
        /// The parameters should follow the base class
        /// </summary>
        /// <param name="inputQueue">Inputqueue in this setting is Resequencer queue</param>
        /// <param name="outputQueue">output queue is the aggregator</param>

        public Resequencer(MessageQueue inputQueue, MessageQueue outputQueue) : base(inputQueue,outputQueue) { }

        /// <summary>
        /// Overriden from the processer class
        /// Controls the flow.
        /// </summary>
        /// <param name="message"></param>
        protected override void ProcessMessage(Message message) 
        {
            AddToBuffer(message);
            SendConsecutiveMessages();
        }

        /// <summary>
        /// Adding to the buffer by the label, if the label is 
        /// </summary>
        /// <param name="message"></param>
        private void AddToBuffer(Message message) 
        {
            // Checking that we can read the correlation ID, a debugging step to make sure all packages despite being splitted, still have the same correlationId
            Console.WriteLine(message.CorrelationId);
            // I Then split the label to get the Message index
            string[] label = message.Label.Split('-');

            // Converting it to an integer so to compare to end-start index
            Int32 msgIndex = Convert.ToInt32(label[0]);
            
            Console.WriteLine("Received Message index {0}", msgIndex);
            if(msgIndex < startIndex)
            {
                Console.WriteLine("Out of range message index! Current start is: {0}", startIndex);
            }
            else
            {
                // Adding message to buffer
                Console.WriteLine("Adding message {0} to buffer", label[0]);
                buffer.Add(msgIndex, message);
                if(msgIndex >= endIndex)
                {
                    endIndex = msgIndex;
                }
            }
            Console.WriteLine("Buffer Range: {0} -> {1}", startIndex,endIndex);
        }

        /// <summary>
        /// This method send the consecutive messages that are present in the buffer
        /// </summary>
        private void SendConsecutiveMessages() 
        {
            // While the buffer contains startindex which gets incremented for each time to send consecutive messages
            while(buffer.Contains(startIndex))
            {
                // It takes the message from the buffer and sends it to the outqueue
                Message m = (Message)buffer[startIndex];
                Console.WriteLine("Sending luggage to Aggregator with id {0}", startIndex);
                outputQueue.Send(m);
                buffer.Remove(startIndex);
                startIndex++;
            }
        }
    }
}
