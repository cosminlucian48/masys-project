using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using static System.Windows.Forms.AxHost;
using System.Linq;

namespace Project
{
    public class Utils
    {
        public static string TrafficLightIntelligence = ConfigurationManager.AppSettings.Get("TrafficLightIntelligence");
        public static int CarGenerationRate = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CarGenerationRate"));
        public static string CarPrioritization = ConfigurationManager.AppSettings.Get("CarPrioritization");
        public static int Delay = Convert.ToInt32(ConfigurationManager.AppSettings.Get("TurnDelay"));
        public static int gridLength = 19;
        public static int[] interestPointsX = { 0, 6, 12, 18 };
        //keep in mind that the index of the elemnt is used in order to get the left/right direction
        public static int[] interestPointsY = { 5, 6, 12, 13 };
        //5 12 L --- 6 13 R
        public static List<int[]> trafficLightsPos = new List<int[]>();

        public static Dictionary<string, string> CarPositions = new Dictionary<string, string>();
        public static Dictionary<string, string> CarDestinations = new Dictionary<string, string>();
        public static Dictionary<string, string> TrafficLightPositions = new Dictionary<string, string>();

        public static Random RandNoGen = new Random();
        public static int noAgents = 0;
        public static int carsToGenerate = 3; // [0,4]
        public static string logFocus = "car";

        public static void initializeTrafficLights()
        {
            int totalCombinations = interestPointsX.Length * interestPointsY.Length;
            int[,] trafficLights = new int[totalCombinations, 2];

            int index = 0;

            for (int i = 0; i < interestPointsX.Length; i++)
            {
                for (int j = 0; j < interestPointsY.Length; j++)
                {
                    trafficLights[index, 0] = interestPointsX[i];
                    trafficLights[index, 1] = interestPointsY[j];
                    index++;
                }
            }


            for (int i = 0; i < totalCombinations; i++)
            {
                if (Array.IndexOf(Utils.interestPointsY, trafficLights[i, 1]) % 2 == 0)
                {
                    if (trafficLights[i, 0] + 1 < gridLength)
                    {
                        int[] combination = { trafficLights[i, 0] + 1, trafficLights[i, 1] };
                        trafficLightsPos.Add(combination);
                    }

                }
                else
                {
                    if (trafficLights[i, 0] - 1 > 0)
                    {
                        int[] combination1 = { trafficLights[i, 0] - 1, trafficLights[i, 1] };
                        trafficLightsPos.Add(combination1);
                    }

                    int[] combination2 = { trafficLights[i, 0], trafficLights[i, 1] + 1 };
                    trafficLightsPos.Add(combination2);
                }
            }

        }
        public static void ParseMessage(string content, out string action, out string parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = "";

            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    parameters += t[i] + " ";
                parameters += t[t.Length - 1];
            }
        }

        public static string Str(object p1, object p2)
        {
            return string.Format("{0} {1}", p1, p2);
        }

        public static string Str(object p1, object p2, object p3)
        {
            return string.Format("{0} {1} {2}", p1, p2, p3);
        }

       /*public static void getUpperSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsX.Contains(coordsX)) yDifference = 3;
            else if (Utils.interestPointsX.Contains(coordsX + 1)) { yDifference = 2; xDifference = -1; }
            else if (Utils.interestPointsX.Contains(coordsX - 1)) { yDifference = 1; xDifference = 1; }


            for (int y = coordsY - yDifference; y > Utils.interestPointsY[1]; y -= 1)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(coordsX - xDifference, y)))
                {
                    noCars++;
                }
            }
            Console.WriteLine($"[{coordsX}, {coordsY}] UP checking y [{coordsY - yDifference}, {Utils.interestPointsY[1]}) on x {coordsX - xDifference}");
            *//*return (State.Up, 25 / (5 - noCars));*//*
        }

        public static void getLeftSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsY.Contains(coordsY - 2)) { yDifference = 2; }
            else if (Utils.interestPointsY.Contains(coordsY)) { xDifference = 1; }

            int leftXStreet = Array.IndexOf(Utils.interestPointsX, coordsX - xDifference) - 1;

            for (int x = coordsX - xDifference - 1; x > Utils.interestPointsX[leftXStreet]; x--)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, coordsY - yDifference)))
                {
                    noCars++;
                }
            }

            Console.WriteLine($"[{coordsX}, {coordsY}] LEFT checking x [{coordsX - xDifference - 1}, {Utils.interestPointsX[leftXStreet]}) on y {coordsY - yDifference}");
            *//*return (State.Left, 25 / 5 - noCars);*//*
        }

        public static void getRightSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsY.Contains(coordsY)) { xDifference = -1; }
            else if (Utils.interestPointsY.Contains(coordsY - 1)) { yDifference = 1; }

            int rightXStreet = Array.IndexOf(Utils.interestPointsX, coordsX - xDifference) + 1;
            for (int x = coordsX - xDifference + 1; x < Utils.interestPointsX[rightXStreet]; x++)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, coordsY - yDifference)))
                {
                    noCars++;
                }
            }

            Console.WriteLine($"[{coordsX}, {coordsY}] RIGHT checking x [{coordsX - xDifference + 1}, {Utils.interestPointsX[rightXStreet]}) on y {coordsY - yDifference}");

            *//*return (State.Left, 25 / 5 - noCars);*//*
        }*/
    }
}