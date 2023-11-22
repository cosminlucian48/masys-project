using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Project
{
    public class CarAgent : Agent
    {
        public Position currentPos, targetPos, intendedPosition;
        private enum State { Up, Left, Right };

        private State _direction, _intendedDirection;
        private State? _optimalDirection;
        public CarAgent(Position start, Position target)
        {
            this.currentPos = start;
            this.targetPos = target;
            this.intendedPosition = new Position(-1, -1);
        }

        public override void Setup()
        {
            _optimalDirection = null;
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
            //update current position with intended position
            if (this.intendedPosition.x != -1)
            {
                this.currentPos.x = this.intendedPosition.x;
                this.currentPos.y = this.intendedPosition.y;
            }
            _direction = State.Up;

            //check direction
            if (!(currentPos.x == targetPos.x) && Utils.interestPointsY.Contains(currentPos.y))
            {
                if (Array.IndexOf(Utils.interestPointsY, currentPos.y) % 2 == 0)
                {
                    //move left
                    if (_optimalDirection != null)
                    {
                        _direction = (State)_optimalDirection;
                        _optimalDirection = null;
                    }
                    else _direction = State.Left;
                }
                else
                {
                    if (currentPos.x < targetPos.x)
                    {
                        //TODO: finish here (dont delete optimal direction right away)
                        //move right
                        if (_optimalDirection != null)
                        {
                            if (_optimalDirection == State.Right)
                            {
                                _direction = (State)_optimalDirection;
                                _optimalDirection = null;
                            }
                            else
                            {

                            }
                        }
                        else _direction = State.Right;
                    }
                }
            }

            //check car direction intention
            int xAxisDifference = currentPos.x - targetPos.x;
            if (xAxisDifference > 1)
            {
                _intendedDirection = State.Left;
            }
            else if (xAxisDifference < -1)
            {
                _intendedDirection = State.Right;
            }
            else
            {
                _intendedDirection = State.Up;
            }

            //TODO: here we should choose the direction based on the equation

            //check if car is on a traffic light and if the light is -red-
            if (Utils.TrafficLightPositions.TryGetValue(this.currentPos.ToString(), out string color))
            {
                if (Utils.interestPointsY[3] == currentPos.y || Utils.interestPointsY[2] == currentPos.y || Utils.interestPointsY[3] + 1 == currentPos.y)
                {
                    _optimalDirection = chooseFavorableSegment();
                }

                if (color == "Red")
                {
                    //check if car has intermitent right green
                    if (!((this._direction == State.Up && _intendedDirection == State.Right)
                        || (this._direction == State.Left && _intendedDirection == State.Up)))
                    {
                        //to wait an entire turn
                        Send("traffic", "carwait");

                        return;
                    }
                    else
                    {
                        Console.WriteLine($"[{this.Name}] has intermitent green!");
                    }
                }
            }

            //update intended position based on direction
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

            /*if (Utils.CarPositions.Values.Contains(intendedPosition.ToString())) //check also if car direction matches semaphor direction
            {
                //ask traffic monitor to send -wait- to this car
                //if we send dirrecttly wait to this.name it will go in infinite loop
                Send("traffic", "carwait");
                return;
            }*/

            //try sending car to intended position
            Send("traffic", Utils.Str("change", intendedPosition.ToString()));
        }

        public void HandleWait()
        {
            //reset intendedPosition if the car has to wait, so that the actual position is not updated in HandleMove
            this.intendedPosition.x = -1;
            this.intendedPosition.y = -1;
            Send(this.Name, "move");
        }

        public void HandleFinish()
        {
            Console.WriteLine($"[{Name}] Reached destination! I am going home.. zZz..");
            this.Stop();
        }

        private (State, int) getUpperSegmentCost()
        {
            int noCars = 0;
            for (int y = currentPos.y - 3; y >= currentPos.y - 2 - 5; y--)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(currentPos.x, y)))
                {
                    noCars++;
                }
            }
            return (State.Up, 25 / 5 - noCars);
        }

        private (State, int) getLeftSegmentCost()
        {
            int noCars = 0;
            for (int x = currentPos.x - 1; x >= currentPos.x - 1 - 5; x--)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, currentPos.y - 2)))
                {
                    noCars++;
                }
            }
            return (State.Left, 25 / 5 - noCars);
        }

        private (State, int) getRightSegmentCost()
        {
            int noCars = 0;
            for (int x = currentPos.x + 1; x <= currentPos.x + 1 + 5; x++)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, currentPos.y - 1)))
                {
                    noCars++;
                }
            }
            return (State.Right, 25 / 5 - noCars);
        }

        private State chooseFavorableSegment()
        {
            //choose the segment to follow with the lowest cost if on first intersection
            List<(State, int)> segmentCosts = new List<(State, int)>
            {
                getUpperSegmentCost()
            };
            if (_intendedDirection == State.Left) { segmentCosts.Add(getLeftSegmentCost()); }
            if (_intendedDirection == State.Right) { segmentCosts.Add(getRightSegmentCost()); }
            segmentCosts.Sort((a, b) => -1 * a.Item2.CompareTo(b.Item2));
            Console.WriteLine($"[{Name}] chose as my optimal direction the following segment: {segmentCosts[0].Item1}");
            for (int i = 0; i < segmentCosts.Count; i++)
            {
                Console.WriteLine($"[{segmentCosts[i].Item1}] with cost: {segmentCosts[i].Item2}");
            }
            return segmentCosts[0].Item1;
        }

    }
}