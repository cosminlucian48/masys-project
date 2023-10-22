using System;

namespace Project
{
    public class Utils
    {
        public static int NoBidders = 5;
        public static int ReservePrice = 100;
        public static int MinPrice = 50;
        public static int MaxPrice = 500;
        public static int Increment = 10;

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