using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Messaging;

namespace MYFirstMSMQ
{
    class Program
    {
        static void Main(string[] args)
        {

            MessageQueue messageQueue = null;
            MessageQueue luggageQueue = new MessageQueue(@".\Private$\luggagehandling");
            MessageQueue personQueue = new MessageQueue(@".\Private$\personqueue");
            MessageQueue resequencer = new MessageQueue(@".\Private$\resequencer");
            MessageQueue SasAirlineCompany = new MessageQueue(@".\Private$\sas");
            if (MessageQueue.Exists(@".\Private$\AirportCheckInOutput"))
            {
                messageQueue = new MessageQueue(@".\Private$\AirportCheckInOutput");
                messageQueue.Label = "CheckIn Queue";
            }
            else
            {
                // Create the Queue
                MessageQueue.Create(@".\Private$\AirportCheckInOutput");
                messageQueue = new MessageQueue(@".\Private$\AirportCheckInOutput");
                messageQueue.Label = "Newly Created Queue";
            }

            XElement CheckInFile = XElement.Load(@"CheckedInPassenger.xml");

            string AirlineCompany = "SAS";

            // Now we need to split the message
            CheckInSplitter splitter = new CheckInSplitter(messageQueue, luggageQueue, personQueue);
            // We receive the message in the luggagehandling
            LuggageHandling luggageHandling = new LuggageHandling(luggageQueue, resequencer);
            // After processing the message in luggagehandling we send the message to the resequencer to make sure it is in the right order
            Resequencer sequencer = new Resequencer(resequencer, SasAirlineCompany);
            
            messageQueue.Send(CheckInFile.ToString(), AirlineCompany);
            Console.ReadLine();
        }
    }
}

