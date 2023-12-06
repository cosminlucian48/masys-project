using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public struct Pebble
    {
        public Pebble(string direction, double nrOfCars, bool alertMode)
        {
            Direction = direction;
            NrOfCars = nrOfCars;
            AlertMode = alertMode;
        }

        public string Direction { get; }
        public double NrOfCars { get; }
        public bool AlertMode { get; }

        public override string ToString()
        {
            return $"{Direction} {NrOfCars} {AlertMode}";
        }
    }
}
