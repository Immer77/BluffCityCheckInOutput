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
    /// Splitter class to split the message into Personinformation and luggagehandling
    /// </summary>
    internal class CheckInSplitter
    {
        /// <summary>
        /// The hardcoded queues we need to receive from and sent out to
        /// </summary>
        protected MessageQueue inQueue;
        protected MessageQueue luggageQueue;
        protected MessageQueue PersonQueue;

        public CheckInSplitter(MessageQueue inQueue, MessageQueue luggageQueue, MessageQueue PersonQueue)
        {
            this.inQueue = inQueue;
            inQueue.MessageReadPropertyFilter.AppSpecific = true;
            inQueue.MessageReadPropertyFilter.Body = true;
            inQueue.MessageReadPropertyFilter.CorrelationId = true;
            inQueue.MessageReadPropertyFilter.Id = true;
            inQueue.MessageReadPropertyFilter.Label = true;
            this.luggageQueue = luggageQueue;
            this.PersonQueue = PersonQueue;

            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();

        }

        /// <summary>
        /// Since we are dealing with an xml format we can alter the 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="asyncResult"></param>
        protected void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            
            // The xmldocument where we load the body that is being sent
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message.Body.ToString());

            // Listing all nodes that is in the xml document so that we can access them and split them to the right channels
            XmlNodeList nodeList;
            XmlElement root = doc.DocumentElement;

            // Since we want to have the reservation number on both the passenger and on the luggage
            XmlNode reservationNumber = root.SelectSingleNode("Passenger/ReservationNumber");
            XmlNode passenger = root.SelectSingleNode("Passenger");

            nodeList = root.SelectNodes("Luggage");
            int luggageAmount = nodeList.Count;

            // Doing this allows us to take the last digit(s) telling us how many packages are required for the aggregator to collect before
            // sending, since we are using the aggregation strategy 'Wait for all' due to the fact that we need all luggage before take off.
            // The + 1 is the person that we also need we are using the '-' to tell us where to split 
            int totalMessages = luggageAmount + 1;


            // Added this for further use in the resequencer to see if all baggage has arrived Using label instead
            XmlNode amountOfPackages = doc.CreateNode("element","AmountOfPackages","");
            amountOfPackages.InnerText = totalMessages.ToString();
            root.AppendChild(amountOfPackages);

            int j = 0;
            // Now looping through each node in the nodelist to apply specific information before sending it to the luggagehandling
            foreach (XmlNode node in nodeList)
            {
                j++;
                
                Message luggageMessage = new Message();

                XmlDocument luggageDoc = new XmlDocument();
                luggageDoc.LoadXml("<luggageitem/>");
                XmlElement luggage = luggageDoc.DocumentElement;

                luggage.AppendChild(luggageDoc.ImportNode(reservationNumber, true));
                luggage.AppendChild(luggageDoc.ImportNode(amountOfPackages, true));

                for (int i = 0; i < node.ChildNodes.Count; i++) 
                {
                    luggage.AppendChild(luggageDoc.ImportNode(node.ChildNodes[i],true));
                }
                luggageMessage.Body = luggage.OuterXml;
                luggageMessage.CorrelationId = message.Id;
                luggageMessage.AppSpecific = totalMessages;
                Console.WriteLine("Sending message to Luggage handling number {0}", j);
                luggageQueue.Send(luggageMessage,$"{j}-{luggageAmount}");
            }
            // Sending to personhandling aswell

            XmlDocument personDoc = new XmlDocument();
            personDoc.LoadXml("<personInfo/>");
            XmlElement personPackage = personDoc.DocumentElement;

            personPackage.AppendChild(personDoc.ImportNode(amountOfPackages, true));
            personPackage.AppendChild(personDoc.ImportNode(passenger, true));

            Message personMessage = new Message();
            personMessage.CorrelationId = message.Id;
            personMessage.Body = personPackage.OuterXml;
            personMessage.AppSpecific = totalMessages;
            PersonQueue.Send(personMessage, "Sending Person information to personhandling");
            mq.BeginReceive();

        }

    }
}
