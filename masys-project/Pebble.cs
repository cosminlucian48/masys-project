﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public struct Pebble
    {
        public Pebble(string direction, double nrOfCars, bool alertMode, int distance)
        {
            Direction = direction;
            NrOfCars = nrOfCars;
            AlertMode = alertMode;
            Distance = distance;
        }

        public string Direction { get; set; }
        public double NrOfCars { get; set; }
        public bool AlertMode { get; }
        public int Distance { get; set; }

        public override string ToString()
        {
            return $"{Direction} {NrOfCars} {AlertMode} {Distance}";
        }
    }
}
