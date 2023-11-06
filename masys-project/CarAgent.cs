using ActressMas;
using System;
using System.Collections.Generic;

namespace Project
{
    public class CarAgent : Agent
    {
        public CarAgent(){}

        public override void Setup()
        {
            Console.WriteLine("[{0}]: hello from car setup.", this.Name);
            Broadcast("hello");
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action;
            string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "hello":
                    Send("trafficlight", "hello");
                    break;

                case "winner":
                    break;

                default:
                    break;
            }
        }


    }
}