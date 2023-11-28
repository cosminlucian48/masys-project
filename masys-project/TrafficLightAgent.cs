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
        private enum Color { Red, Green };
        private enum Direction { Up, Left, Right };
        private string whereAmI = "";
        private Dictionary<Direction, TrafficLight> trafficLights = new Dictionary<Direction, TrafficLight>();
        private Color _color;
        private readonly int trafficLightInterval = 12 * Utils.Delay;
        private int _lightConfigState = 0;
        // private int trafficLightInterval = Utils.RandNoGen.Next(5, 30) * Utils.Delay;
        public TrafficLightAgent(Position p)
        {
            TrafficLight.Color defaultColor = TrafficLight.Color.Red;
            //on UP
            if (Utils.interestPointsX.Contains(p.x))
            {
                whereAmI = "Up";
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, TrafficLight.Color.Green, TrafficLight.Direction.Up));
                if (p.x - 1 > 0)
                {
                    trafficLights.Add(Direction.Left, new TrafficLight(p.x, p.y, TrafficLight.Color.Green, TrafficLight.Direction.Left));
                }
                if (p.x + 1 < Utils.gridLength)
                {
                    trafficLights.Add(Direction.Right, new TrafficLight(p.x, p.y, TrafficLight.Color.IntermitentGreen, TrafficLight.Direction.Right));
                }
            }
            //on LEFT
            else if (Array.IndexOf(Utils.interestPointsY, p.y) % 2 == 0)
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

            //add in if block, for nonintelligent traffic light
            _timer = new Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = trafficLightInterval;
            this.pos = p;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._lightConfigState++;
            Send(this.Name, "lightchange");
            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: Traffic light at p = {1} {2} is ready!", this.Name, this.pos.x, this.pos.y);
            foreach (var trafficLight in trafficLights)
            {
                Send("traffic", Utils.Str("lightposition", Utils.Str(pos.x, pos.y, trafficLight.Key, trafficLight.Value.color)));
            }
            _timer.Start();

        }

        public override void Act(Message message)
        {
            if (Utils.logFocus.Length > 0 && this.Name.Contains(Utils.logFocus))
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
            }

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "lightchange":
                    //TODO: keep in mind that for inteligence>=1 we dont need timer
                    HandleLightChange();
                    break;

                default:
                    break;
            }
        }

        public void HandleLightChange()
        {
            // foreach (var trafficLight in trafficLights.Values)
            // {
            //     Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLight.direction, trafficLight.lightChange())));
            // }
            var middleCurrentStep = this._lightConfigState % 3;

            Console.WriteLine($"[{this.Name}]: middleCurrentStep = {middleCurrentStep} {whereAmI}");

            //if traffic light agent is on the side of the map, it has only 2 states
            // each state alternates from the previous one
            if (pos.x % (Utils.gridLength - 1) == 0)
            {
                Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Up].direction, trafficLights[Direction.Up].lightChange())));
                if (trafficLights.ContainsKey(Direction.Left)) Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Left].direction, trafficLights[Direction.Left].lightChange())));
            }
            //else -> traffic lights will have alternating states over 3 repeating iterations
            /*
                UPWARD TRAFFFIC LIGHT -> intermitent green [right] on all cases
                case 0: green [up, left]
                case 1 and 2: red [up, left]
            */
            /*
                RIGHT TRAFFFIC LIGHT
                case 0: red [up, right]
                case 1: green [right] ; red [up]
                case 2: green [up] ; red [right]
            */
            /*
                LEFT TRAFFFIC LIGHT -> intermitent green [up] on all cases
                case 0: red [left]
                case 1: green [left]
                case 2: red [left]
            */
            else switch (whereAmI)
                {
                    case "Up":
                        if (middleCurrentStep == 0)
                        {
                            trafficLights[Direction.Up].color = TrafficLight.Color.Green;
                            trafficLights[Direction.Left].color = TrafficLight.Color.Green;
                        }
                        else
                        {
                            trafficLights[Direction.Up].color = TrafficLight.Color.Red;
                            trafficLights[Direction.Left].color = TrafficLight.Color.Red;
                        }
                        Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Up].direction, trafficLights[Direction.Up].color)));
                        Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Left].direction, trafficLights[Direction.Left].color)));
                        break;
                    case "Right":
                        switch (middleCurrentStep)
                        {
                            case 1:
                                trafficLights[Direction.Up].color = TrafficLight.Color.Red;
                                if (trafficLights.ContainsKey(Direction.Right)) trafficLights[Direction.Right].color = TrafficLight.Color.Green;
                                break;
                            case 2:
                                trafficLights[Direction.Up].color = TrafficLight.Color.Green;
                                if (trafficLights.ContainsKey(Direction.Right)) trafficLights[Direction.Right].color = TrafficLight.Color.Red;
                                break;
                            default:
                                trafficLights[Direction.Up].color = TrafficLight.Color.Red;
                                if (trafficLights.ContainsKey(Direction.Right)) trafficLights[Direction.Right].color = TrafficLight.Color.Red;
                                break;
                        }
                        Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Up].direction, trafficLights[Direction.Up].color)));
                        if (trafficLights.ContainsKey(Direction.Right)) Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Right].direction, trafficLights[Direction.Right].color)));
                        break;
                    case "Left":
                        if (middleCurrentStep == 1)
                        {
                            if (trafficLights.ContainsKey(Direction.Left)) trafficLights[Direction.Left].color = TrafficLight.Color.Green;
                        }
                        else
                        {
                            if (trafficLights.ContainsKey(Direction.Left)) trafficLights[Direction.Left].color = TrafficLight.Color.Red;
                        }
                        if (trafficLights.ContainsKey(Direction.Left)) Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[Direction.Left].direction, trafficLights[Direction.Left].color)));
                        break;
                    default:
                        break;
                }
        }

    }
}