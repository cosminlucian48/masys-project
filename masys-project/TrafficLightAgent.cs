using ActressMas;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Project
{
    public class TrafficLightAgent : Agent
    {
        private Timer _timer;
        public Position p;
        public TrafficLightAgent(Position p)
        {
            _timer = new Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = Utils.Delay;
            this.p = p;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "wake-up");
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: hello from traffic light setup.", this.Name);
            _timer.Start();
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "hello":
                    Console.WriteLine("hello from traffic ligth.");
                    break;

                case "wake-up":
                    Console.WriteLine("Waking up!");
                    break;

                default:
                    break;
            }
        }

    }
}