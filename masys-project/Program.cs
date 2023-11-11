using System.Configuration;
using System.Collections.Specialized;
using System;

namespace Project
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ActressMas.EnvironmentMas(0, Utils.Delay);
            /*Utils.RandNoGen.Next(4)*/
            for (int i = 0; i < 4; i++)
            {
                var carAgent = new CarAgent(new Position(Utils.interestPointsX[i], Utils.gridLength-1), new Position(Utils.interestPointsX[i], 0));
                env.Add(carAgent, string.Format("car{0:D2}", i));
               
                Utils.noAgents += 1;
            }

            /*var trafficLightAgent = new TrafficLightAgent(new Position(Utils.RandNoGen.Next(4), Utils.RandNoGen.Next(2) + 1));
            env.Add(trafficLightAgent, "trafficlight");*/

            var trafficAgent = new TrafficAgent();
            env.Add(trafficAgent, "traffic");

            env.Start();
        }
    }
}