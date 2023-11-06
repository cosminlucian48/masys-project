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

            for (int i = 1; i <= Utils.NoBidders; i++)
            {
                int agentValuation = Utils.MinPrice + Utils.RandNoGen.Next(Utils.MaxPrice - Utils.MinPrice);
                var bidderAgent = new CarAgent(agentValuation);
                env.Add(bidderAgent, string.Format("bidder{0:D2}", i));
            }

            var auctioneerAgent = new TrafficLightAgent();
            env.Add(auctioneerAgent, "auctioneer");

            env.Start();
        }
    }
}