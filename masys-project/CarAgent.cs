using ActressMas;
using System;
using System.Linq;
using System.Threading;

namespace Project
{
    public class CarAgent : Agent
    {
        public Position currentPos, targetPos, intendedPosition;
        private enum State { Up, Left, Right };

        private State _intendedDirection;
        private State _direction;
        public CarAgent(Position start, Position target) {
            this.currentPos = start;
            this.targetPos = target;
            this.intendedPosition = new Position(-1, -1);
        }

        public override void Setup()
        {
            _direction = State.Up;
            Console.WriteLine($"[{Name}]: Starting!");
            Send("traffic", Utils.Str("position", currentPos.ToString(), targetPos.ToString()));
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action;
            string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "hello":
                    Send("trafficlight", "hello");
                    break;

                case "move":
                    HandleMove(parameters);
                    break;

                case "wait":
                    HandleWait();
                    break;

                case "finish":
                    HandleFinish();
                    break;
                default:
                    break;
            }
        }

        public void HandleMove(string direction)
        {
            //check direction
            switch (direction)
            {
                case "Up":
                    _direction = State.Up;
                    break;
                case "Left":
                    _direction = State.Left;
                    break;
                case "Right":
                    _direction = State.Right;
                    break;
                default:
                    _direction = State.Up;
                    break;
            }

            //update current position with intended position
            if (this.intendedPosition.x != -1)
            {
                this.currentPos.x = this.intendedPosition.x;
                this.currentPos.y = this.intendedPosition.y;
            }

            //check if car is on a traffic light and if the light is -red-
            if (Utils.TrafficLightPositions.TryGetValue(this.currentPos.ToString(), out string color) && color == "Red")
            {
                //check car direction intention
                //TODO: here we should choose the direction based on the equation
                int xAxisDifference = currentPos.x - targetPos.x;
                if (xAxisDifference>1)
                {
                    _intendedDirection = State.Left;
                }else if (xAxisDifference < -1)
                {
                    _intendedDirection = State.Right;
                }
                else
                {
                    _intendedDirection = State.Up;
                }

                //check if car has intermitent right green
                if(!((this._direction == State.Up && _intendedDirection == State.Right) 
                    || (this._direction == State.Left && _intendedDirection == State.Up))) 
                {
                    Send("traffic", "carwait");
                    return;
                }
                else
                {
                    Console.WriteLine($"[{this.Name}] has intermitent green!");
                }
            }

            //updated intended position based on direction
            intendedPosition = new Position(currentPos.x, currentPos.y);
            switch (_direction)
            {
                case State.Up:
                    intendedPosition.y -= 1;
                    break;
                case State.Left:
                    intendedPosition.x -= 1;
                    break;
                case State.Right:
                    intendedPosition.x += 1;
                    break;
                default:
                    break;
            }

            if (Utils.CarPositions.Values.Contains(intendedPosition.ToString())) //check also if car direction matches semaphor direction
            {
                //ask traffic monitor to send -wait- to this car
                //if we send dirrecttly wait to this.name it will go in infinite loop
                Send("traffic", "carwait");
                return;
            }

            //try sending car to intended position
            Send("traffic", Utils.Str("change", intendedPosition.ToString()));
        }

        public void HandleWait()
        {
            //reset intendedPosition if the car has to wait, so that the actual position is not updated in HandleMove
            this.intendedPosition.x = -1;
            this.intendedPosition.y = -1;
            Send(this.Name, Utils.Str("move",_direction));
        }

        public void HandleFinish()
        {
            Console.WriteLine($"[{Name}] Reached destination! I am going home.. zZz..");
            this.Stop();
        }

    }
}