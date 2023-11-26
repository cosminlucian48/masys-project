using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Project
{
    public class TrafficLightAgent : Agent
    {
        private Timer _timer;
        public Position pos;
        /*private enum State { Up, Left, Right };*/
        private enum Color { Red, Green};
        private enum Direction { Up, Left, Right};
        private string whereAmI = "";
        private Dictionary<Direction, TrafficLight> trafficLights = new Dictionary<Direction, TrafficLight>();
        private Color _color;
        private int trafficLightInterval = Utils.RandNoGen.Next(5, 30) * Utils.Delay;
        public TrafficLightAgent(Position p)
        {
            TrafficLight.Color defaultColor = TrafficLight.Color.Red;
            //on UP
            if(Utils.interestPointsX.Contains(p.x))
            {
                whereAmI = "Up";
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Up));
                if (p.x - 1 >0)
                {
                    trafficLights.Add(Direction.Left, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Left));
                }
                if(p.x + 1< Utils.gridLength)
                {
                    trafficLights.Add(Direction.Right, new TrafficLight(p.x, p.y, TrafficLight.Color.IntermitentGreen, TrafficLight.Direction.Right));
                }
            }
            //on LEFT
            else if(Array.IndexOf(Utils.interestPointsY, p.y) % 2 == 0)
            {
                whereAmI = "Left";
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, TrafficLight.Color.IntermitentGreen, TrafficLight.Direction.Up));
                if (p.x - 1 > 0)
                {
                    trafficLights.Add(Direction.Left, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Left));
                }
            }
            //on RIGHT
            else
            {
                whereAmI = "Right";
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Up));
                if (p.x + 2 < Utils.gridLength)
                {
                    trafficLights.Add(Direction.Right, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Right));
                }
            }

            Console.WriteLine($"{p.x} {p.y} , {trafficLights.Count}");
          
            _timer = new Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = trafficLightInterval;
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
            foreach(var trafficLight in trafficLights)
            {
                Send("traffic", Utils.Str("lightposition", Utils.Str(pos.x, pos.y, trafficLight.Key, trafficLight.Value.color)));
            }
            if(whereAmI == "Right")
            {
                Task.Delay(trafficLightInterval).ContinueWith(_ =>
                {
                    _timer.Start();
                });
            }
            else
            {
                _timer.Start();
            }
            
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
                    HandleLightChange();
                    break;

                default:
                    break;
            }
        }

        public void HandleLightChange()
        {
            foreach (var trafficLight in trafficLights.Values)
            {
                Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLight.direction, trafficLight.lightChange())));
            }
        }

    }
}