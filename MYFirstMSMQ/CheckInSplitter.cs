using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MYFirstMSMQ
{
    internal class CheckInSplitter
    {
        protected MessageQueue inQueue;
        protected MessageQueue luggageQueue;
        protected MessageQueue PersonQueue;

        public CheckInSplitter(MessageQueue inQueue, MessageQueue luggageQueue, MessageQueue PersonQueue)
        {
            this.inQueue = inQueue;
            this.luggageQueue = luggageQueue;
            this.PersonQueue = PersonQueue;

            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();

            inQueue.Formatter = new ActiveXMessageFormatter();
            luggageQueue.Formatter = new ActiveXMessageFormatter();
            PersonQueue.Formatter = new ActiveXMessageFormatter();
        }

        protected void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            mq.Formatter = new ActiveXMessageFormatter();
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

            // Added this for further use in the resequencer to see if all baggage has arrived
            XmlNode amountOfLuggage = doc.CreateNode("element","AmountOfLuggage","");
            amountOfLuggage.InnerText = luggageAmount.ToString();
            root.AppendChild(amountOfLuggage);

            foreach (XmlNode node in nodeList)
            {

                Message m = new Message();

                XmlDocument luggageDoc = new XmlDocument();
                luggageDoc.LoadXml("<luggageItem/>");
                XmlElement luggage = luggageDoc.DocumentElement;

                luggage.AppendChild(luggageDoc.ImportNode(reservationNumber, true));
                luggage.AppendChild(luggageDoc.ImportNode(amountOfLuggage, true));

                for (int i = 0; i < node.ChildNodes.Count; i++) 
                {
                    luggage.AppendChild(luggageDoc.ImportNode(node.ChildNodes[i],true));
                    m.AppSpecific = i;
                }
                m.Body = luggage.OuterXml;
                luggageQueue.Send(m,$"{m.AppSpecific}-{luggageAmount}");
            }
            PersonQueue.Send(passenger.OuterXml);
            mq.BeginReceive();

        }

    }
}
