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
                var carAgent = new CarAgent();
                env.Add(carAgent, string.Format("car{0:D2}", i));
            }

            var trafficLightAgent = new TrafficLightAgent();
            env.Add(trafficLightAgent, "trafficlight");

            env.Start();
        }
    }
}