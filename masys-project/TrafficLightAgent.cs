using ActressMas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Project
{
    public class TrafficLightAgent : Agent
    {
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>
        {
            {"Up", null}, { "Left", null}, { "Right", null}
        };
        public Position pos;
        private enum Direction { Up, Left, Right };
        private Dictionary<Direction, TrafficLight> trafficLights = new Dictionary<Direction, TrafficLight>();
        private int[] trafficLightIntervals = {20 * Utils.Delay, 10 * Utils.Delay};
        private int initialInterval = 2 * Utils.Delay;
        private Dictionary<string, int> currentIntervalsIndex = new Dictionary<string, int> {
            {"Up", 0}, { "Left", 0}, { "Right", 0}
        };
        //on what type of segment is this traffic light situated
        private string whereAmI;

        public TrafficLightAgent(Position p, int trafficLightPosition)
        {
            if (trafficLightPosition == 0) whereAmI = "Up";
            else if (trafficLightPosition == 1) whereAmI = "Left";
            else if (trafficLightPosition == -1) whereAmI = "Right";
            else throw new Exception("bad argument");
            TrafficLight.Color defaultColor = TrafficLight.Color.Red;

            //on UP segment
            if(whereAmI=="Up")
            {
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
            //on LEFT segment
            else if (whereAmI == "Left")
            {
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, TrafficLight.Color.IntermitentGreen, TrafficLight.Direction.Up));
                if (p.x - 1 > 0)
                {
                    trafficLights.Add(Direction.Left, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Left));
                }
            }
            //on RIGHT segment
            else
            {
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Up));
                if (p.x + 2 < Utils.gridLength)
                {
                    trafficLights.Add(Direction.Right, new TrafficLight(p.x, p.y, defaultColor, TrafficLight.Direction.Right));
                }
            }

            //timers for UP LEFT RIGHT
            foreach (var val in trafficLights)
            {
                if ($"{val.Value.color}" != "IntermitentGreen")
                {
                    timers[$"{val.Key}"] = new Timer();
                    //use t_Elapsed from lambda in order to send an extra argument (the direction for whcih the traffic light has changed)
                    timers[$"{val.Key}"].Elapsed += (sender, e) => t_Elapsed(sender, e, $"{val.Key}");
                    //alterate intervales, as the traffic light is organise din 3 times (3 timpi)
                    //every traffic light will be green for N seconds, then red for 2*N seconds
                    timers[$"{val.Key}"].Interval = initialInterval;
                }
                
            }
            
            this.pos = p;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e, string direction)
        {
            Send(this.Name, Utils.Str("lightchange", direction));
            //change timer interval
            currentIntervalsIndex[$"{direction}"] = (currentIntervalsIndex[$"{direction}"] + 1) % trafficLightIntervals.Length;
            timers[$"{direction}"].Interval = trafficLightIntervals[currentIntervalsIndex[$"{direction}"]];
            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: Traffic light at p = {1} {2} is ready!", this.Name, this.pos.x, this.pos.y);

            //send position to traffic monitor agent
            foreach(var trafficLight in trafficLights)
            {
                Send("traffic", Utils.Str("lightposition", Utils.Str(pos.x, pos.y, trafficLight.Key, trafficLight.Value.color)));
            }

            //delay traffic lights based on position
            //the order implemented here is explained in the documentation
            if (whereAmI == "Up")
            {
                Task.Delay(0).ContinueWith(_ =>
                {
                    timers["Up"].Start();
                });
                if (timers["Left"] != null)
                {
                    Task.Delay(0).ContinueWith(_ =>
                    {
                        timers["Left"].Start();
                    });
                }
            }
            else if (whereAmI == "Left" && timers["Left"] != null)
            {
                Task.Delay(trafficLightIntervals[1]).ContinueWith(_ =>
                {
                    timers["Left"].Start();
                });
            }
            else if (whereAmI == "Right" )
            {
                if (timers["Right"] != null)
                {
                    Task.Delay(trafficLightIntervals[1]).ContinueWith(_ =>
                    {
                        timers["Right"].Start();
                    });
                }
                Task.Delay(trafficLightIntervals[0]).ContinueWith(_ =>
                {
                    timers["Up"].Start();
                });
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
                    HandleLightChange(parameters);
                    break;
                default:
                    break;
            }
        }

        public void HandleLightChange(string direction)
        {
            foreach (var trafficLight in trafficLights.Values)
            {
                if ($"{trafficLight.direction}" == direction)
                {
                    Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, direction, trafficLight.lightChange())));
                    break;
                }
                
            }
        }

    }
}