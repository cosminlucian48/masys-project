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
            }


            if (_ownerAgent != null)
            {
                foreach (KeyValuePair<string, string> entry in _ownerAgent.CarPositions)
                {
                    string[] t = entry.Value.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);
                    g.FillEllipse(Brushes.Blue, x * cellSize+4, y * cellSize+4, cellSize-8, cellSize-8); //4 and 8 to make the car smaller and put it in the center
                    g.DrawString(entry.Key.Substring(3), textFont, Brushes.White, new PointF(x * cellSize + 10, y * cellSize + 10));
                }

            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }
    }
}
