using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace Life
{
    public partial class Form1 : Form
    {
        private Bitmap bmp;
        private Life l;
        /// <summary>
        /// Form constructor. It creates a random initial state and a new Life object with Conway's rules (B3/S23).
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            bmp = new Bitmap(panel1.Width, panel1.Height);
            List<BitArray> state = new List<BitArray>(panel1.Width);
            Random r = new Random();
            for (int i = 0; i < panel1.Width; i++)
            {
                state.Add(new BitArray(panel1.Height));
                for (int j = 0; j < panel1.Height; j++)
                    state[i][j] = (r.Next(12) < 5);
                    // state[i][j] = false;
            }
            
            //Glider:
         /*   state[99][98] = true;
            state[100][99] = true;
            state[98][100] = true;
            state[99][100] = true;
            state[100][100] = true;
         */
         
            l = new Life(bmp, state, "B3/S23");
        }

        /// <summary>
        /// Paint event handler. Simply copies the bitmap drawn by the Life object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

            e.Graphics.DrawImage(l.Bmp, 0, 0);

        }

        /// <summary>
        /// Timer's tick event handler. Updates the Life object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.l.Update();
            panel1.Invalidate();
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            this.l.Width = panel1.Width;
            this.l.Height = panel1.Height;
        }
    }
}
