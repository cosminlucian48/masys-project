using ActressMas;
using System;
using System.Linq;
using System.Threading;

namespace Project
{
    public class CarAgent : Agent
    {
        public Position currentPos, targetPos;
        private enum State { Up, Left, Right };
        
        private State _state;
        public CarAgent(Position start, Position target) {
            this.currentPos = start;
            this.targetPos = target;
        }

        public override void Setup()
        {
            _state = State.Up;
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
                    string direction = parameters;
                    switch (direction)
                    {
                        case "Up":
                            _state = State.Up;
                            break;
                        case "Left":
                            _state = State.Left;
                            break;
                        case "Right":
                            _state = State.Right;
                            break;
                        default:
                            _state = State.Up;
                            break;
                    }
                    HandleMove();
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

        public void HandleMove()
        {
            if (Utils.TrafficLightPositions.TryGetValue(this.currentPos.ToString(), out string color) && color == "Red")
            {
                Send("traffic", "carwait");
                return;
            }

            Position intendedPosition = new Position(currentPos.x, currentPos.y);
            switch (_state)
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

            if (Utils.CarPositions.Values.Contains(intendedPosition.ToString())) // or TrafficLights.contains(position) and check also car direction if matches semaphor direction
            {
                Send("traffic", "carwait");
                return;
            }

            this.currentPos.x = intendedPosition.x;
            this.currentPos.y = intendedPosition.y;

            Send("traffic", Utils.Str("change", currentPos.ToString()));
        }

        public void HandleWait()
        {
            Send(this.Name, Utils.Str("move",_state));
        }

        public void HandleFinish()
        {
            Console.WriteLine($"[{Name}] Reached destination! I am going home.. zZz..");
            this.Stop();
        }


    }
}