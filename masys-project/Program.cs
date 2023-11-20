namespace Project
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ActressMas.EnvironmentMas(0, Utils.Delay);
            for (int i = 0; i < 4; i++)
            {
                var carAgent = new CarAgent(new Position(Utils.interestPointsX[i], Utils.gridLength - 1), new Position(Utils.interestPointsX[Utils.RandNoGen.Next(4)], 0));
                env.Add(carAgent, string.Format("car{0:D2}", i));

                Utils.noAgents += 1;
            }

            Utils.initializeTrafficLights();
            //for el in trafficLights2, if x=0 numa 2 dir, if x=18 numa 2 dir
            for (int i = 0; i < Utils.trafficLightsPos.Count; i++)
            {
                var trafficLightAgent = new TrafficLightAgent(new Position(Utils.trafficLightsPos[i][0], Utils.trafficLightsPos[i][1]));
                env.Add(trafficLightAgent, string.Format("light{0:D2}", i));
            }

            var trafficAgent = new TrafficAgent();
            env.Add(trafficAgent, "traffic");

            env.Start();
        }
    }
}