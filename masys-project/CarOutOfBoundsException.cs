using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class CarOutOfBounds : Exception
    {
        public CarOutOfBounds()
        {
        }

        public CarOutOfBounds(string message)
            : base(message)
        {
        }

        public CarOutOfBounds(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
