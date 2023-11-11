using ActressMas;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Project
{
    public class CarAgent : Agent
    {
        public Position currentPos, targetPos;
        private State _state;
        public CarAgent(Position start, Position target) {
            this.currentPos = start;
            this.targetPos = target;
        }

        private enum State {Waiting, Up, Left, Right };

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
                    HandleMove();
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
            // TODO: change direction based on states
            // TODO: maybe, before updating position, check if target position is free or not
            //      if position is not free, maybe put car agent on Waiting state, and maybe using a timer get it started again later
            if (_state == State.Up)
            {
                this.currentPos.y -= 1;
            }
            Send("traffic", Utils.Str("change", currentPos.ToString()));
        }

        public void HandleFinish()
        {
            Console.WriteLine($"[{Name}] Reached destination! I am going home.. zZz..");
            this.Stop();
        }


    }
}