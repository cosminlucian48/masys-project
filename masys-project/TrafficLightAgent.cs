using ActressMas;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Project
{
    public class TrafficLightAgent : Agent
    {
        private List<string> _bidders;
        private string _highestBidder;
        private int _currentPrice;
        private Timer _timer;

        public TrafficLightAgent()
        {
            _bidders = new List<string>();
            _timer = new Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = Utils.Delay;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "wake-up");
        }

        public override void Setup()
        {
            _currentPrice = Utils.ReservePrice;
            Broadcast(Utils.Str("price", _currentPrice));
            _timer.Start();
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "bid":
                    HandleBid(message.Sender);
                    break;

                case "wake-up":
                    HandleWakeUp();
                    break;

                default:
                    break;
            }
        }

        private void HandleBid(string sender)
        {
            _bidders.Add(sender);
        }

        private void HandleWakeUp()
        {
            if (_bidders.Count == 0) // no more bids
            {
                _currentPrice -= Utils.Increment;
                if (_currentPrice < Utils.ReservePrice)
                {
                    Console.WriteLine("[auctioneer]: Auction finished. No winner.");
                    Broadcast(Utils.Str("winner", "none"));
                }
                else
                {
                    Console.WriteLine("[auctioneer]: Auction finished. Sold to {0} for price {1}.", _highestBidder, _currentPrice);
                    Broadcast(Utils.Str("winner", _highestBidder));
                }
                _timer.Stop();
                Stop();
            }
            else if (_bidders.Count == 1)
            {
                _highestBidder = _bidders[0];
                Console.WriteLine("[auctioneer]: Auction finished. Sold to {0} for price {1}", _highestBidder, _currentPrice);
                Broadcast(Utils.Str("winner", _highestBidder));
                _timer.Stop();
                Stop();
            }
            else
            {
                _highestBidder = _bidders[0]; // first or random from the previous round, breaking ties
                _currentPrice += Utils.Increment;

                foreach (string a in _bidders)
                    Send(a, Utils.Str("price", _currentPrice));

                _bidders.Clear();
            }
        }
    }
}