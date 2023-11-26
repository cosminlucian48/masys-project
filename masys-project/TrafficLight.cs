using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class TrafficLight
    {
        public enum Color { Red, Green, IntermitentGreen };
        public enum Direction { Up, Left, Right };
        public int x { get; set; }
        public int y { get; set; }

        public Color color = Color.Green;
        public Direction direction = Direction.Up;
       
        
        public TrafficLight(int x, int y, Color color, Direction direction)
        {
            this.x = x;
            this.y = y;
            this.color = color;
            this.direction = direction;
        }

        public Color lightChange()
        {
            if (color == Color.Red) color = Color.Green;
            else if (color == Color.Green) color = Color.Red;
            else color = Color.IntermitentGreen;

            return color;
        }

        public override string ToString()
        {
            return Utils.Str(x, y);
        }
    }
}
