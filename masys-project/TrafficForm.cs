using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class TrafficForm : Form
    {
        private TrafficAgent _ownerAgent;
        private Bitmap _doubleBufferImage;
        private object _locker = new object();
        public TrafficForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        public void SetOwner(TrafficAgent a)
        {
            _ownerAgent = a;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            DrawPlanet();
        }

        public void UpdatePlanetGUI()
        {
            DrawPlanet();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawPlanet();
        }

        private void DrawPlanet()
        {
            int w = pictureBox.Width;
            int h = pictureBox.Height;

            if (_doubleBufferImage != null)
            {
                _doubleBufferImage.Dispose();
                GC.Collect(); // prevents memory leaks
            }

            _doubleBufferImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(_doubleBufferImage);
            g.Clear(Color.LightGreen);

            int minXY = Math.Min(w, h);
            int cellSize = (minXY) / Utils.gridLength;
            Font textFont = new Font("Arial", 12);
            Font arrowFont = new Font("Arial", 40);

            foreach (int pos in Utils.interestPointsX)
            {
                g.FillRectangle(Brushes.DarkGray, pos * cellSize, 1 * cellSize, cellSize, cellSize * (Utils.gridLength - 2));

                g.FillRectangle(Brushes.Brown, pos * cellSize, 0 * cellSize, cellSize, cellSize);
                g.FillRectangle(Brushes.LightBlue, pos * cellSize, (Utils.gridLength - 1) * cellSize, cellSize, cellSize);

                g.DrawString("↑", arrowFont, Brushes.White, new PointF(pos * cellSize, (Utils.gridLength - 3) * cellSize));
            }

            bool toggle = true;

            foreach (int pos in Utils.interestPointsY)
            {
                g.FillRectangle(Brushes.DarkGray, 1 * cellSize, pos * cellSize, cellSize * (Utils.gridLength - 1), cellSize);

                g.DrawString(toggle ? "←" : "→", arrowFont, Brushes.White, new PointF(2.5f * cellSize, pos * cellSize - 12));
                g.DrawString(toggle ? "←" : "→", arrowFont, Brushes.White, new PointF(8.5f * cellSize, pos * cellSize - 12));
                g.DrawString(toggle ? "←" : "→", arrowFont, Brushes.White, new PointF(14.5f * cellSize, pos * cellSize - 12));
                toggle = !toggle;
            }

            for (int i = 0; i <= Utils.gridLength; i++)
            {
                g.DrawLine(Pens.Black, 0, i * cellSize,  Utils.gridLength * cellSize, i * cellSize);
                g.DrawLine(Pens.Black, i * cellSize, 0, i * cellSize, Utils.gridLength * cellSize);

                //draw x and Y map coordinates
                g.DrawString(i.ToString(), new Font("Arial", 11), Brushes.Black, new PointF(5, i * cellSize + 5));
                g.DrawString(i.ToString(), new Font("Arial", 11), Brushes.Black, new PointF(i * cellSize + 5, 5));
            }

            
            if (_ownerAgent != null)
            {
                //draw cars
                foreach (KeyValuePair<string, string> entry in Utils.CarPositions)
                {
                    string[] t = entry.Value.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);
                    g.FillEllipse(Brushes.Blue, x * cellSize+4, y * cellSize+4, cellSize-8, cellSize-8); //4 and 8 to make the car smaller and put it in the center
                    g.DrawString(entry.Key.Substring(3), textFont, Brushes.White, new PointF(x * cellSize + 10, y * cellSize + 10));
                }

                //inverted dictionary
                Dictionary<string, List<string>> carPositionsInverted = new Dictionary<string, List<String>>();

                foreach (var keyValuePair in Utils.CarDestinations)
                {
                    string name = keyValuePair.Value;
                    string key = keyValuePair.Key;

                    if (!carPositionsInverted.ContainsKey(name))
                    {
                        carPositionsInverted[name] = new List<String>();
                    }

                    carPositionsInverted[name].Add(key);
                }

                //destinations labels
                foreach (KeyValuePair<string, List<string>> entry in carPositionsInverted)
                {
                    string[] t = entry.Key.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);

                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        g.DrawString(entry.Value[i].Substring(3), new Font("Arial", 7), Brushes.Black, new PointF(x * cellSize + 25, y * cellSize + 8 * i));
                    }

                }
            }

            Color transparentOrange = Color.FromArgb(128, Color.Orange); // Adjust 128 for the desired transparency level (0 to 255)
            SolidBrush transparentOrangeBrush = new SolidBrush(transparentOrange);

            //draw traffic lights
            foreach (KeyValuePair<string, Dictionary<string,string>> entry in Utils.TrafficLightPositions)
            {
                string[] t = entry.Key.Split();
                int x = Convert.ToInt32(t[0]);
                int y = Convert.ToInt32(t[1]);
                int padding = 0;
                foreach(var tf in entry.Value)
                {
                    Brush brush = null;
                    if (tf.Value == "Green") brush = Brushes.Green;
                    else if (tf.Value == "Red") brush = Brushes.Red;
                    else if (tf.Value == "IntermitentGreen") brush = Brushes.GreenYellow;

                    g.FillEllipse(brush, (x) * cellSize + 1 + padding, (y) * cellSize + 1, cellSize - 26, cellSize - 26);
                    g.DrawString(tf.Key.Substring(0,1), new Font("Arial", 8), Brushes.Black, new PointF((x) * cellSize + 4 + padding, y * cellSize + 1));
                    padding += 15;
                }

                //mark on the map the trafficlights that are in alert mode
                if (Utils.TrafficLightAlertMode[$"{x} {y}"] == true)
                {
                    g.FillEllipse(transparentOrangeBrush, (x) * cellSize + 1, (y) * cellSize + 1, cellSize, cellSize);
                }
            }

            lock (_locker)
            {
                Graphics pbg = pictureBox.CreateGraphics();
                pbg.DrawImage(_doubleBufferImage, 0, 0);
            }
        }
    }
}
