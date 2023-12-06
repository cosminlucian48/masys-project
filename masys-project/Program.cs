using System;

namespace Project
{
    
    public class Program
    {
        private static void Main()
        {
            var env = new ActressMas.EnvironmentMas(0, Utils.Delay);
            for (int i = 0; i < Utils.InitialTurnCarsToGenerate; i++)
            {
                var carAgent = new CarAgent(new Position(Utils.interestPointsX[i], Utils.gridLength - 1), new Position(Utils.interestPointsX[Utils.RandNoGen.Next(4)], 0));
                env.Add(carAgent, string.Format("car{0:D2}", i));

                Utils.noAgents += 1;
            }

            Utils.initializeTrafficLights();
            
            for (int i = 0; i < Utils.trafficLightsPos.Count; i++)
            {
                var trafficLightAgent = new TrafficLightAgent(new Position(Convert.ToInt32(Utils.trafficLightsPos[i][0]), Convert.ToInt32(Utils.trafficLightsPos[i][1])));
                
                //keeping this for testing purposes
                /*if (Utils.trafficLightsPos[i][1] > 7)
                {
                    Utils.getUpperSegmentCost(Utils.trafficLightsPos[i][0], Utils.trafficLightsPos[i][1]);
                }*/

                /*if (Utils.trafficLightsPos[i][0] > 2 && Utils.trafficLightsPos[i][1] > 7 && Utils.trafficLightsPos[i][1] != Utils.interestPointsY[3])
                {
                    Utils.getLeftSegmentCost(Utils.trafficLightsPos[i][0], Utils.trafficLightsPos[i][1]);
                }*/

                /*if (Utils.trafficLightsPos[i][0] <17 && Utils.trafficLightsPos[i][1] > 7 && Utils.trafficLightsPos[i][1] != Utils.interestPointsY[2])
                {
                    Utils.getRightSegmentCost(Utils.trafficLightsPos[i][0], Utils.trafficLightsPos[i][1]);
                }*/


                env.Add(trafficLightAgent, $"light {Utils.trafficLightsPos[i][0]} {Utils.trafficLightsPos[i][1]}");
            }

            //keeping this for testing purposes
            /*Utils.CarPositions.Add("test1", "6 11");
            Utils.CarPositions.Add("test2", "6 10");
            Utils.CarPositions.Add("test3", "6 9");
            Utils.CarPositions.Add("test4", "6 8");
            Utils.CarPositions.Add("test5", "6 7");*/


            var trafficAgent = new TrafficAgent();
            env.Add(trafficAgent, "traffic");

            env.Start();
        }
    }
}