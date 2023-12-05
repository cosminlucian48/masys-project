using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Project
{
    public class TrafficLightAgent : Agent
    {
        private Timer _timer, _alertTimer;
        public Position pos;
        /*private enum State { Up, Left, Right };*/
        private enum Color { Red, Green };
        private enum Direction { Up, Left, Right };
        private string whereAmI = "";
        private bool alertMode = false;
        private int carsOnMe = 0;
        private Dictionary<Direction, TrafficLight> trafficLights = new Dictionary<Direction, TrafficLight>();
        private readonly int trafficLightInterval = 12 * Utils.Delay;
        private int _lightConfigState = 0;
        private List<string> neighboursToAlert = new List<string>();
        private Pebble alert;
        public TrafficLightAgent(Position p)
        {
            //on UP
            if (Utils.interestPointsX.Contains(p.x))
            {
                whereAmI = "Up";
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, Utils.TrafficLightIntelligence== 0 ? TrafficLight.Color.Green: TrafficLight.Color.Green, TrafficLight.Direction.Up));
                if (p.x - 1 > 0)
                {
                    trafficLights.Add(Direction.Left, new TrafficLight(p.x, p.y, Utils.TrafficLightIntelligence == 0 ? TrafficLight.Color.Green : TrafficLight.Color.Green, TrafficLight.Direction.Left));
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
                    trafficLights.Add(Direction.Left, new TrafficLight(p.x, p.y, Utils.TrafficLightIntelligence == 0? TrafficLight.Color.Red : TrafficLight.Color.Green, TrafficLight.Direction.Left));
                }
            }
            //on RIGHT
            else
            {
                whereAmI = "Right";
                trafficLights.Add(Direction.Up, new TrafficLight(p.x, p.y, Utils.TrafficLightIntelligence == 0 ? TrafficLight.Color.Red : TrafficLight.Color.Green, TrafficLight.Direction.Up));
                if (p.x + 2 < Utils.gridLength)
                {
                    trafficLights.Add(Direction.Right, new TrafficLight(p.x, p.y, Utils.TrafficLightIntelligence == 0 ? TrafficLight.Color.Red : TrafficLight.Color.Green, TrafficLight.Direction.Right));
                }
            }
            this.pos = p;
            //Console.WriteLine($"{p.x} {p.y} , {trafficLights.Count}");

            //check what neighbours this trafficlight should alert in case it has to many cars on its segment
            
            if(pos.y <= Utils.interestPointsY[3])
            {
                switch (whereAmI)
                {
                    case "Up":
                        neighboursToAlert.Add($"{pos.x} {pos.y + 7}");
                        if (pos.x - 1 >= 0)
                        {
                            neighboursToAlert.Add($"{pos.x - 1} {pos.y + 6}");
                        }
                        break;
                    case "Left":
                        if (pos.x + 6 < Utils.gridLength)
                        {
                            neighboursToAlert.Add($"{pos.x + 6} {pos.y}");
                        }
                        neighboursToAlert.Add($"{pos.x + 5} {pos.y + 2}");
                        break;
                    case "Right":
                        if (pos.x - 6 >= 0)
                        {
                            neighboursToAlert.Add($"{pos.x - 6} {pos.y}");
                        }
                        break;
                    default:
                        break;

                }
            }
           
            for(int i=0;i< neighboursToAlert.Count;i++)
            {
                Console.WriteLine($"[{pos.x} {pos.y}] has one neigh {neighboursToAlert[i]}");
            }
            //add in if block, for nonintelligent traffic light

            if (Utils.TrafficLightIntelligence == 0)
            {
                _timer = new Timer();
                _timer.Elapsed += t_Elapsed;
                _timer.Interval = trafficLightInterval;
            }
            else
            {
                _alertTimer = new Timer();
                _alertTimer.Elapsed += alert_Elapsed;
                _alertTimer.Interval = 2 * Utils.Delay;
            }
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._lightConfigState++;
            Send(this.Name, "lightchange");
            return;
        }

        private void alert_Elapsed(object sender, ElapsedEventArgs e)
        {
            int nrOfCars = 0;

            switch (whereAmI)
            {
                case "Up":
                    for(int y = pos.y; y < pos.y + 5; y++)
                    {
                        if(Utils.CarPositions.Values.Contains($"{pos.x} {y}"))
                        {
                            nrOfCars++;
                        }
                    }
                    break;
                case "Left":
                    for (int x = pos.x; x < pos.x + 5; x++)
                    {
                        if (Utils.CarPositions.Values.Contains($"{x} {pos.y}"))
                        {
                            nrOfCars++;
                        }
                    }
                    break;
                case "Right":
                    for (int x = pos.x; x > pos.x - 5; x--)
                    {
                        if (Utils.CarPositions.Values.Contains($"{x} {pos.y}"))
                        {
                            nrOfCars++;
                        }
                    }
                    break;
                default:
                    break;
            }

            carsOnMe = nrOfCars;
            if (carsOnMe >= Utils.AlertThreshold && alertMode== false)
            {
                foreach (string neigh in neighboursToAlert)
                {
                    if(Utils.TrafficLightPositions.ContainsKey($"{neigh}"))
                    {
                        alertMode = true;
                        alert = new Pebble(whereAmI, carsOnMe);
                        Send($"light {neigh}", Utils.Str("alert",alert.ToString()));
                    }
                }
            }
            else
            {
                if (carsOnMe < Utils.AlertThreshold && alertMode == true)
                {
                    foreach (string neigh in neighboursToAlert)
                    {
                        if (Utils.TrafficLightPositions.ContainsKey($"{neigh}"))
                        {
                            alertMode = false;
                            alert = new Pebble(whereAmI, carsOnMe);
                            Send($"light {neigh}", Utils.Str("revokealert", alert.ToString()));
                        }
                    }
                }
            }

            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: Traffic light at p = {1} {2} is ready!", this.Name, this.pos.x, this.pos.y);
            foreach (var trafficLight in trafficLights)
            {
                Send("traffic", Utils.Str("lightposition", Utils.Str(pos.x, pos.y, trafficLight.Key, trafficLight.Value.color)));
            }

            if (Utils.TrafficLightIntelligence == 0)
            {
                _timer.Start();
            }
            else
            {
                _alertTimer.Start();
            }
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "lightchange":
                    //TODO: keep in mind that for inteligence>=1 we dont need timer
                    HandleLightChange();
                    break;
                case "alert":
                    HandleLightChange(message.Sender, parameters);
                    break;
                case "revokealert":
                    HandleRevokeAlert(message.Sender, parameters);
                    break;
                default:
                    break;
            }
        }

        public void HandleRevokeAlert(string sender, string alertPebble)
        {
            alertMode = false;
            Pebble alert = Utils.returnPebbleFromAlertString(alertPebble);

            Direction dir = Direction.Up;

            switch (alert.Direction)
            {
                case "Up":
                    dir = Direction.Up;
                    break;
                case "Right":
                    dir = Direction.Right;
                    break;
                case "Left":
                    dir = Direction.Left;
                    break;
                default:
                    break;
            }

            if (trafficLights[dir].color == TrafficLight.Color.Red)
            {
                Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[dir].direction, trafficLights[dir].lightChange())));
            }
        }

        public void HandleLightChange(string sender, string alertPebble)
        {
            Pebble alert = Utils.returnPebbleFromAlertString(alertPebble);
            /*Console.WriteLine($"{this.Name} alert from {sender} = {alert.Direction}- {alert.NrOfCars}-------------------------------------");*/
            alertMode = true;
            Direction dir = Direction.Up;

            switch (alert.Direction)
            {
                case "Up":
                    dir = Direction.Up;
                    break;
                case "Right":
                    dir = Direction.Right;
                    break;
                case "Left":
                    dir = Direction.Left;
                    break;
                default:
                    break;
            }

            if (trafficLights[dir].color == TrafficLight.Color.Green)
            {
                Send("traffic", Utils.Str("lightchange", Utils.Str(pos.x, pos.y, trafficLights[dir].direction, trafficLights[dir].lightChange())));
            }
            
        }

        public void HandleLightChange()
        {
            Console.WriteLine("WHATEEEVEEEEEEEEEEEEEEEEEEERR!!!!!!!!!!!!!!!!!!!");
            var middleCurrentStep = this._lightConfigState % 3;

            //Console.WriteLine($"[{this.Name}]: middleCurrentStep = {middleCurrentStep} {whereAmI}");

            //if traffic light agent is on the side of the map, it has only 2 states
            // each state alternates from the previous one
            if (pos.x % (Utils.gridLength - 1) == 0 || pos.x == Utils.gridLength - 2)
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