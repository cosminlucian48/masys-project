using System;
using System.Configuration;
using System.Collections.Specialized;

namespace Project
{
    public class Utils
    {
        public static string TrafficLightIntelligence = ConfigurationManager.AppSettings.Get("TrafficLightIntelligence");
        public static string CarGenerationRate = ConfigurationManager.AppSettings.Get("CarGenerationRate");
        public static string CarPrioritization = ConfigurationManager.AppSettings.Get("CarPrioritization");
        public static int gridLength = 19;
        public static int gridBlockSize = 20;
        //public static int[] interestPointsX = { 1, 7, 13, 19 };
        public static int[] interestPointsX = { 0, 6, 12, 18 };
        //public static int[] interestPointsY = { 6, 7, 13, 14 };
        public static int[] interestPointsY = { 5, 6, 12, 13 };
        //5 12 r 6 13 l

        public static int Delay = 1000;
        public static Random RandNoGen = new Random();

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
    }
}