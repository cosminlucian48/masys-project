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

            //TODO, here if the car is on X traffic light, it wont get an optimal direction
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
                    _optimalDirection = chooseFavorableSegment(currentPos.x, currentPos.y);
                }

                if (color == "Red")
                {
                    //check if car has intermitent right green
                    if (!((this._direction == State.Up && _optimalDirection == State.Right)
                        || (this._direction == State.Left && _optimalDirection == State.Up)))
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

        private (State, int) getUpperSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsX.Contains(coordsX)) yDifference = 3;
            else if (Utils.interestPointsX.Contains(coordsX + 1)) { yDifference = 2; xDifference = -1; }
            else if (Utils.interestPointsX.Contains(coordsX - 1)) { yDifference = 1; xDifference = 1; }


            for (int y = coordsY - yDifference; y > Utils.interestPointsY[1]; y -= 1)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(coordsX - xDifference, y)))
                {
                    noCars++;
                }
            }
            Console.WriteLine($"[{coordsX}, {coordsY}] UP checking y [{coordsY - yDifference}, {Utils.interestPointsY[1]}) on x {coordsX - xDifference}");
            return (State.Up, 25 / (5 - noCars));
        }

        private (State, int) getLeftSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsY.Contains(coordsY - 2)) { yDifference = 2; }
            else if (Utils.interestPointsY.Contains(coordsY)) { xDifference = 1; }

            int leftXStreet = Array.IndexOf(Utils.interestPointsX, coordsX - xDifference) - 1;

            for (int x = coordsX - xDifference - 1; x > Utils.interestPointsX[leftXStreet]; x--)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, coordsY - yDifference)))
                {
                    noCars++;
                }
            }

            Console.WriteLine($"[{coordsX}, {coordsY}] LEFT checking x [{coordsX - xDifference - 1}, {Utils.interestPointsX[leftXStreet]}) on y {coordsY - yDifference}");
            return (State.Left, 25 / 5 - noCars);
        }

        private (State, int) getRightSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsY.Contains(coordsY)) { xDifference = -1; }
            else if  (Utils.interestPointsY.Contains(coordsY - 1)) { yDifference = 1; }

            int rightXStreet = Array.IndexOf(Utils.interestPointsX, coordsX - xDifference) + 1;
            for (int x = coordsX - xDifference + 1; x < Utils.interestPointsX[rightXStreet]; x++)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, coordsY - yDifference)))
                {
                    noCars++;
                }
            }

            Console.WriteLine($"[{coordsX}, {coordsY}] RIGHT checking x [{coordsX - xDifference + 1}, {Utils.interestPointsX[rightXStreet]}) on y {coordsY - yDifference}");

            return (State.Left, 25 / 5 - noCars);
        }

        private State chooseFavorableSegment(int coordsX, int coordsY)
        {
            //choose the segment to follow with the lowest cost if on first intersection
            List<(State, int)> segmentCosts = new List<(State, int)>
            {
                getUpperSegmentCost(coordsX, coordsY)
            };

            if (_intendedDirection == State.Left) { segmentCosts.Add(getLeftSegmentCost(coordsX, coordsY)); }
            if (_intendedDirection == State.Right) { segmentCosts.Add(getRightSegmentCost(coordsX, coordsY)); }

            segmentCosts.Sort((a, b) => -1 * a.Item2.CompareTo(b.Item2));

            Console.WriteLine($"[{Name}] chose as my optimal direction the following segment: {segmentCosts[0].Item1}");

            for (int i = 0; i < segmentCosts.Count; i++)
            {
                Console.WriteLine($"[{Name}][{segmentCosts[i].Item1}] with cost: {segmentCosts[i].Item2}");
            }
            return segmentCosts[0].Item1;
        }

    }
}