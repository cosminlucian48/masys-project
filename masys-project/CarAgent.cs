using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Console.WriteLine($"[{Name}]: Starting! with target pos=[{this.targetPos.x} {this.targetPos.y}]");
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
            string color = "";
            //update current position with intended position
            if (this.intendedPosition.x != -1)
            {
                this.currentPos.x = this.intendedPosition.x;
                this.currentPos.y = this.intendedPosition.y;
            }

            //check if car is on traffic light
            if (Utils.TrafficLightPositions.ContainsKey(this.currentPos.ToString()))
            {
                //check car intention; basically if it is already in targetX/targetX-1/targetX+1 it will go UP, and if not it will consider also LEFT/RIGHT
                int xAxisDifference = currentPos.x - targetPos.x;
                if (xAxisDifference < -1)
                {
                    _intendedDirection = State.Right;
                }
                else if (xAxisDifference > 1)
                {
                    _intendedDirection = State.Left;
                }
                else
                {
                    _intendedDirection = State.Up;
                }

                //compute car optimal direction
                if (
                    (Utils.interestPointsY[3] == currentPos.y
                    || Utils.interestPointsY[2] == currentPos.y
                    || Utils.interestPointsY[3] + 1 == currentPos.y)
                )
                {
                    //set optimal direction, based on car prioritization
                    _optimalDirection = chooseFavorableSegment(currentPos.x, currentPos.y);
                }
                else
                {
                    _optimalDirection = _intendedDirection;
                }

                color = Utils.TrafficLightPositions[this.currentPos.ToString()][Convert.ToString(_optimalDirection)];
            }

            _direction = State.Up;

            //decide car direction
            if (currentPos.x != targetPos.x && Utils.interestPointsY.Contains(currentPos.y))
            {
                if (Array.IndexOf(Utils.interestPointsY, currentPos.y) % 2 == 0)
                {
                    _direction = Utils.interestPointsX.Contains(currentPos.x) ? (State)_optimalDirection : State.Left;
                }
                else
                {
                    if (currentPos.x < targetPos.x)
                    {
                        _direction = Utils.interestPointsX.Contains(currentPos.x) ? (State)_optimalDirection : State.Right;
                    }
                    // if currentPosX>targetPosX then car to go to the left road. And for this to happen it has to go one more square UP
                }
            }

            //if traffic light is red
            if (color == "Red") { Send("traffic", "carwait"); return; }
            //else if (color == "IntermitentGreen") Console.WriteLine($"[{this.Name}] has intermitent green!");


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

            if (intendedPosition.x < 0 || intendedPosition.y < 0 || (!Utils.interestPointsX.Contains(intendedPosition.x) && !Utils.interestPointsY.Contains(intendedPosition.y)))
            {
                Console.WriteLine("--------------------------------------------------------------------------");
                Console.WriteLine($"[{this.Name} out of bounds: [{intendedPosition.x},{intendedPosition.y}]]");
                throw new CarOutOfBounds($"Car '{this.Name}' is out of bounds at position: [{intendedPosition.x},{intendedPosition.y}]");
            }

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

        private (State, double) getUpperSegmentCost(int coordsX, int coordsY)
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
            //Console.WriteLine($"[{coordsX}, {coordsY}] UP checking y [{coordsY - yDifference}, {Utils.interestPointsY[1]}) on x {coordsX - xDifference}");
            return (State.Up, 25 / (5 - (double)noCars));
        }

        private (State, double) getLeftSegmentCost(int coordsX, int coordsY)
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

            //Console.WriteLine($"[{coordsX}, {coordsY}] LEFT checking x [{coordsX - xDifference - 1}, {Utils.interestPointsX[leftXStreet]}) on y {coordsY - yDifference}");
            return (State.Left, 25 / (5 - (double)noCars));
        }

        private (State, double) getRightSegmentCost(int coordsX, int coordsY)
        {
            int noCars = 0;
            int yDifference = 0, xDifference = 0;
            if (Utils.interestPointsY.Contains(coordsY)) { xDifference = -1; }
            else if (Utils.interestPointsY.Contains(coordsY - 1)) { yDifference = 1; }

            int rightXStreet = Array.IndexOf(Utils.interestPointsX, coordsX - xDifference) + 1;
            for (int x = coordsX - xDifference + 1; x < Utils.interestPointsX[rightXStreet]; x++)
            {
                if (Utils.CarPositions.Values.Contains(Utils.Str(x, coordsY - yDifference)))
                {
                    noCars++;
                }
            }

            //Console.WriteLine($"[{coordsX}, {coordsY}] RIGHT checking x [{coordsX - xDifference + 1}, {Utils.interestPointsX[rightXStreet]}) on y {coordsY - yDifference}");

            return (State.Right, 25 / (5 - (double)noCars));
        }

        private State chooseFavorableSegment(int coordsX, int coordsY)
        {
            //if car prioritisez traffic lights
            if (Utils.CarPrioritization == "trafficlights")
            {
                var trafficLightState = Utils.TrafficLightPositions[this.currentPos.ToString()];

                //if the car has the oportunity to chose between two directions. Basically this happends only when the intendedDirection is "Left" or "Right"
                //as in that moment the car can go either "Up" or "Left"/"Right", and it will chose the first Green trafficlight
                if ($"{_intendedDirection}" != "Up")
                {
                    string desiredDirection = trafficLightState.Where(x => (x.Key == "Up" || x.Key == $"{_intendedDirection}") && x.Value.Contains("Green"))
                        .FirstOrDefault(x => x.Value.Contains("Green")).Key;
                    switch (desiredDirection)
                    {
                        case "Up":
                            return State.Up;
                        case "Left":
                            return State.Left;
                        case "Right":
                            return State.Right;
                        default:
                            return _intendedDirection;
                    }
                }
                return _intendedDirection;
                
            }
            //if car prioritisez cars
            else if (Utils.CarPrioritization == "cars")
            {
                //choose the segment to follow with the lowest cost if on first intersection
                List<(State, double)> segmentCosts = new List<(State, double)>
                {
                    getUpperSegmentCost(coordsX, coordsY)
                };

                if (_intendedDirection == State.Left) { segmentCosts.Add(getLeftSegmentCost(coordsX, coordsY)); }
                if (_intendedDirection == State.Right) { segmentCosts.Add(getRightSegmentCost(coordsX, coordsY)); }

                segmentCosts.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                /*Console.WriteLine($"[{Name}] optimal direction: {segmentCosts[0].Item1} intendedDirection {this._intendedDirection}; based on: " +
                      string.Join("; ", segmentCosts.Select(s => $"[{s.Item1}] cost {s.Item2}")));*/
                return segmentCosts[0].Item1;
            }
            else
            {
                throw new Exception("CarPrioritization must be 'car' or 'traffic'!");
            }


        }
    }
}