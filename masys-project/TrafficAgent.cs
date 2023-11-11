using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public class TrafficAgent : Agent
    {
        private TrafficForm _formGui;
        public TrafficAgent()
        {
            Console.WriteLine("hello from constructor {0}", this.Name);
            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }

        private void GUIThread()
        {
            _formGui = new TrafficForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Thread.Sleep(500);
            while(true)
            {
                _formGui.UpdatePlanetGUI();
            }
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                //TODO: cases

                default:
                    break;
            }
            _formGui.UpdatePlanetGUI();
        }
    }
}
