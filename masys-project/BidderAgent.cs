using ActressMas;
using System;
using System.Collections.Generic;

namespace Project
{
    public class BidderAgent : Agent
    {
        private int _valuation;

        public BidderAgent(int val)
        {
            _valuation = val;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: My valuation is {1}", this.Name, _valuation);
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action;
            string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "price":
                    HandlePrice(Convert.ToInt32(parameters));
                    break;

                case "winner":
                    HandleWinner(parameters);
                    break;

                default:
                    break;
            }
        }

        private void HandlePrice(int currentPrice)
        {
            if (currentPrice <= _valuation)
                Send("auctioneer", "bid");
        }

        private void HandleWinner(string winner)
        {
            if (winner == this.Name)
                Console.WriteLine("[{0}]: I have won.", this.Name);

            Stop();
        }
    }
}