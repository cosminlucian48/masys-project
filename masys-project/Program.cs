using System.Configuration;
using System.Collections.Specialized;
using System;

namespace Project
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ActressMas.EnvironmentMas(0, 200);

            for (int i = 1; i <= 2; i++)
            {
                var carAgent = new CarAgent(new Position(Utils.RandNoGen.Next(4), 0), new Position(Utils.RandNoGen.Next(4), 3));
                env.Add(carAgent, string.Format("car{0:D2}", i));
            }


            var trafficLightAgent = new TrafficLightAgent(new Position(Utils.RandNoGen.Next(4), Utils.RandNoGen.Next(2) + 1));
            env.Add(trafficLightAgent, "trafficlight");

            env.Start();
        }
    }
}