using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MYFirstMSMQ
{
    /// <summary>
    /// Passenger Aggregate to store the message for the aggregator
    /// since we are using the aggregation strategy 'Wait for All' due to it being important that we get all luggage on the plane for each passenger.
    /// </summary>
    internal class PassengerAggregate : Aggregate
    {
        private int size;
        public List<Message> messages;

        public PassengerAggregate(int size) 
        { 
            this.size = size;
            messages = new List<Message>();
        }

        /// <summary>
        /// Adding the message into the list of messages
        /// Making sure we dont get duplicates
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(Message message)
        {
            if(!messages.Contains(message)) 
            {
                messages.Add(message);
            }
        }

        /// <summary>
        /// This message is triggered when the IsComplete() function is true
        /// Gets all the messages and collects them into a single one to be sent to the airlinecompany
        /// </summary>
        /// <returns></returns>
        public Message GetResultMessage()
        {
            Message resultMessage = new Message();
            resultMessage.CorrelationId = messages[0].CorrelationId;

            XmlDocument schemaDoc = new XmlDocument();
            XmlElement root = schemaDoc.CreateElement("root");

            foreach (Message mes in messages)
            {
                XmlDocument messageDoc = new XmlDocument();
                mes.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                messageDoc.LoadXml(mes.Body.ToString());

                // Append the message's XML content to the schema
                XmlNode importedNode = schemaDoc.ImportNode(messageDoc.DocumentElement, true);
                root.AppendChild(importedNode);
            }

            schemaDoc.AppendChild(root);
            resultMessage.Body = schemaDoc.OuterXml;
            return resultMessage;
        }

        /// <summary>
        /// Since we have the wait for all aggregation strategy the condition is that the size of the total messages needs to match
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            return size == messages.Count;
        }
    }
}
