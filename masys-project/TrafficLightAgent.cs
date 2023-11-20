using ActressMas;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Project
{
    public class TrafficLightAgent : Agent
    {
        private Timer _timer;
        public Position pos;
        /*private enum State { Up, Left, Right };*/
        private enum State { Red, Green};
        private State _state;
        public TrafficLightAgent(Position p)
        {
            _timer = new Timer();
            _timer.Elapsed += t_Elapsed;
            int homeManyDelays = Utils.RandNoGen.Next(5,30);
            _timer.Interval = homeManyDelays * Utils.Delay;
            this.pos = p;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "lightchange");
            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: Traffic light at p = {1} {2} is ready!", this.Name, this.pos.x, this.pos.y);
            Send("traffic", Utils.Str("lightposition", pos.ToString()));
            this._state = State.Green;
            _timer.Start();
        }

        public override void Act(Message message)
        {
            if(Utils.logFocus.Length>0 && this.Name.Contains(Utils.logFocus))
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
            }

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "lightchange":
                    if (_state == State.Red)
                    {
                        _state = State.Green;
                    }
                    else
                    {
                        _state = State.Red;
                    }
                    Send("traffic", Utils.Str("lightchange",Utils.Str(pos.ToString(),_state)));
                    break;

                default:
                    break;
            }
        }

    }
}