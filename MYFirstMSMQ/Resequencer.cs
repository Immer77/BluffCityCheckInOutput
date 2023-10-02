using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MYFirstMSMQ
{
    internal class Resequencer : Processor
    {
        // Where the buffer starts
        private int startIndex = 1;
        private IDictionary buffer = new Hashtable();
        private int endIndex = -1;

        // The parameters should follow the base class
        public Resequencer(MessageQueue inputQueue, MessageQueue outputQueue) : base(inputQueue,outputQueue) { }

        // Should be overrided
        protected override void ProcessMessage(Message message) 
        {
            AddToBuffer(message);
            SendConsecutiveMessages();
        }

        private void AddToBuffer(Message message) 
        {
            string[] label = message.Label.Split('-');

            Int32 msgIndex = Convert.ToInt32(label[0]);
            Console.WriteLine("Received Message index {0}", msgIndex);
            if(msgIndex < startIndex)
            {
                Console.WriteLine("Out of range message index! Current start is: {0}", startIndex);
            }
            else
            {
                buffer.Add(msgIndex, message);
                if(msgIndex >= endIndex)
                {
                    endIndex = msgIndex;
                }
            }
            Console.WriteLine("Buffer Range: {0} -> {1}", startIndex,endIndex);
        }

        private void SendConsecutiveMessages() 
        {
            while(buffer.Contains(startIndex))
            {
                Message m = (Message)buffer[startIndex];
                Console.WriteLine("Sending luggage with id {0}", startIndex);
                outputQueue.Send(m);
                buffer.Remove(startIndex);
                startIndex++;
            }
        }
    }
}
