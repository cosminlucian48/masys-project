using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace Project
{
    public class TrafficAgent : Agent
    {
        private TrafficForm _formGui;
        private System.Timers.Timer _timer;
        private readonly object positionsLock = new object();

        public TrafficAgent()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = 10*(Utils.Delay+100);

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "spawn");
        }

        private void GUIThread()
        {
            _formGui = new TrafficForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine($"[{this.Name}] Starting map!");
            _timer.Start();
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);
            

            switch (action)
            {
                //TODO: cases
                case "position":
                    HandlePosition(message.Sender, parameters);
                    break;
                case "change":
                    HandleChange(message.Sender, parameters);
                    break;
                case "spawn":
                    HandleSpawn();
                    break;
                case "lightposition":
                    HandleLightPosition(parameters);
                    break;
                case "lightchange":
                    HandleLightChange(parameters); 
                    break;
                case "carwait":
                    Send(message.Sender,"wait");
                    break;
                default:
                    break;
            }

            while (_formGui == null) Thread.Sleep(100);
            lock (positionsLock)
            {
                _formGui.UpdatePlanetGUI();
            }
        }

        private void HandleSpawn()
        {
               // TODO: check if there is space before spawning a new car
            int x, y;
            string[] t;
            int[] possibleX = { };

            foreach (string k in Utils.CarPositions.Values)
            {
                t = k.Split();
                x = Convert.ToInt32(t[0]);
                y = Convert.ToInt32(t[1]);
                possibleX = Utils.interestPointsX;
                if (y == Utils.gridLength - 1 && !(possibleX.Contains(x)))
                {
                    possibleX = possibleX.Where(val => val != x).ToArray();
                }
            }

            for(int i=0;i<Utils.CarGenerationRate;i++)
            {
                if (possibleX.Count() == 0)
                {
                    Console.WriteLine($"[{Name}] Can not spawn any more vehicles now. Will spawn again at next cycle.");
                }
                else
                {
                    x = possibleX[Utils.RandNoGen.Next(possibleX.Count())];
                    possibleX = possibleX.Where(val => val != x).ToArray();

                    var carAgent = new CarAgent(new Position(x, Utils.gridLength - 1), new Position(Utils.interestPointsX[Utils.RandNoGen.Next(4)], 0));

                    Utils.noAgents += 1;
                    this.Environment.Add(carAgent, string.Format("car{0:D2}", Utils.noAgents));
                }
                
            }
            
        }
        private void HandlePosition(string sender, string positions)
        {
            string[] t = positions.Split();

            Utils.CarPositions.Add(sender, $"{t[0]} {t[1]}");
            Utils.CarDestinations.Add(sender, $"{t[2]} {t[3]}");
            Send(sender, Utils.Str("move", "up"));
        }

        private void HandleLightPosition(string position)
        {
            string[] t = position.Split();
            Utils.TrafficLightPositions.Add($"{t[0]} {t[1]}", "Green");
        }

        private void HandleLightChange(string parameters)
        {
            string[] t = parameters.Split();
            Utils.TrafficLightPositions[$"{t[0]} {t[1]}"] = t[2];
        }
        

        private void HandleChange(string sender, string position)
        {
            Utils.CarPositions[sender] = position;

                //check if at finish
            string[] t = position.Split();
            int actualX = Convert.ToInt32(t[0]);
            int actualY = Convert.ToInt32(t[1]);

            t = Utils.CarDestinations[sender].Split();
            int targetX = Convert.ToInt32(t[0]);
            int targetY = Convert.ToInt32(t[1]);
            if (actualX == targetX && actualY == targetY)
            {
                Send(sender, "finish");
                Utils.CarPositions.Remove(sender);
                Utils.CarDestinations.Remove(sender);
            }
            else
            {
                if(actualX == targetX || !Utils.interestPointsY.Contains(actualY))
                {
                    Send(sender, Utils.Str("move", "Up"));
                }
                else
                //cost
                {
                    if(Array.IndexOf(Utils.interestPointsY, actualY) % 2 == 0)
                    {
                        Send(sender, Utils.Str("move", "Left"));
                    }
                    else
                    {
                        if(actualX < targetX)
                        {
                            Send(sender, Utils.Str("move", "Right"));
                        }
                        // so that it can get on the left direction road. It has to go one more square UP
                        else Send(sender, Utils.Str("move", "Up"));
                    }
                }
            }
            
        }

    }
}
