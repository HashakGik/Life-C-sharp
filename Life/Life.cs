using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Drawing.Imaging;

namespace Life
{
    /// <summary>
    /// Life object. Allows to define the rule set to use in standard notation (S.../B...).
    /// </summary>
    class Life
    {
        /// <summary>
        /// Bitmask for the rule set: the first 8 bits define the Survive rule, the last 8 bits define the Born rule (e.g. B3/S23 -> 00000110 00000100).
        /// </summary>
        private UInt16 rule;
        /// <summary>
        /// Current state of the playing field. Each point is a single bit (0: dead, 1: alive).
        /// </summary>
        private List<BitArray> state;
        /// <summary>
        /// Number of live neighbors for each cell. This reduces the number of calculations required to update the state.
        /// </summary>
        private List<List<UInt16> > neighbors;
        /// <summary>
        /// Painting surface for the playing field.
        /// </summary>
        private Bitmap bmp;
        /// <summary>
        /// Width of the playing field.
        /// </summary>
        private int w;
        /// <summary>
        /// Height of the playing field.
        /// </summary>
        private int h;
        public Bitmap Bmp {
            get { return this.bmp; }
            set
            {
                // TO DO: update neighbors, state, w and h.
                // New rows/columns should be set to false (dead).
            }
        }
        public int Width {
            get { return this.w; }
            set
            {
                // Resizes neighbors, state and bmp.
                // New columns are set to false.
                if (this.w < value)
                {
                    BitArray b = new BitArray(this.h);
                    List<UInt16> l = new List<UInt16>(this.h);

                    // Increase width.
                    for (int i = 0; i < this.h; i++)
                        l.Add(0);

                    for (int i = 0; i < value - this.w; i++)
                    {
                        this.state.Add(b);
                        this.neighbors.Add(l);
                    }
                }
                else if (this.w > value)
                {
                    // Decrease width.
                    this.state.RemoveRange(value, this.w - value);
                    this.neighbors.RemoveRange(value, this.w - value);
                }
                this.w = value;
                this.bmp = new Bitmap(this.w, this.h);
            }
        }
        public int Height
        {
            get { return this.h; }
            set
            {
                // Resizes neighbors, state and bmp.
                // New rows are set to false. Less efficient than Width's setter because it has to update each neighbors[i], instead of just one.
                if (this.h < value)
                {
                    BitArray b = new BitArray(this.h);
                    List<UInt16> l = new List<UInt16>(this.h);

                    // Increase height.
                    for (int i = 0; i < this.w; i++)
                    {
                        this.state[i].Length = value;
                        for (int j = 0; j < value - this.h; j++)
                            this.neighbors[i].Add(0);
                    }
                }
                else if (this.h > value)
                {
                    // Decrease height.
                    for (int i = 0; i < this.w; i++)
                    {
                        this.neighbors[i].RemoveRange(value, this.h - value);
                        this.state[i].Length = value;
                    }
                  
                }
                this.h = value;
                this.bmp = new Bitmap(this.w, this.h);
            }
        }

        /// <summary>
        /// Converts rule to a human readable string.
        /// </summary>
        /// <returns>Rule set in the form B.../S...</returns>
        public string RuleToString()
        {
            string ret = "B";

            for (int i = 0; i < 8; i++)
                if ((rule & (0x1 << i)) != 0)
                    ret += (i+1).ToString();
            ret += "/S";
            for (int i = 8; i < 16; i++)
                if ((rule & (0x1 << i)) != 0)
                    ret += (i - 7).ToString();

                    return ret;
        }
        /// <summary>
        /// Converts a rule set from string to UInt16.
        /// </summary>
        /// <param name="str">Rule set to be converted.</param>
        /// <returns>Internal representation of the rule set.</returns>
        public UInt16 StringToRule(string str)
        {
            UInt16 ret = 1540; // B3/S23. Default value.
            string[] splitted = str.Split('/');

            try
            {
                if (splitted.Length == 2)
                {
                    if (splitted[0][0] == 'B' && splitted[1][0] == 'S')
                    {
                        ret = 0;
                        for (int i = 1; i < splitted[0].Length; i++)
                        {
                            ret += (UInt16)(1 << (UInt16.Parse(splitted[0][i].ToString())) - 1);
                        }
                        for (int i = 1; i < splitted[1].Length; i++)
                        {
                            ret += (UInt16)(1 << (UInt16.Parse(splitted[1][i].ToString())) + 7);
                        }

                    }
                    else if (splitted[0][0] == 'S' && splitted[1][0] == 'B')
                    {
                        ret = 0;
                        for (int i = 1; i < splitted[1].Length; i++)
                        {
                            ret += (UInt16)(1 << (UInt16.Parse(splitted[1][i].ToString())) - 1);
                        }
                        for (int i = 1; i < splitted[0].Length; i++)
                        {
                            ret += (UInt16)(1 << (UInt16.Parse(splitted[0][i].ToString())) + 7);
                        }

                    }
                }
            }
            catch (FormatException)
            {
                ret = 1540; // B3/S23
            }


            return ret;
        }
        /// <summary>
        /// Constructor with no initial state set. Creates an empty field (all cells dead).
        /// </summary>
        /// <param name="bmp">Painting surface in which the field will be drawn.</param>
        /// <param name="rule">Rule set in standard notation (how many neighbors are required for birth and survival. Default value is Conway's Game of Life.
        /// Some well known rules are:
        /// - Conway's Game of Life: B3/S23
        /// - Fredkin's Replicator: B1357/S1357
        /// - Seeds: B2/S
        /// - Life without death: B3/S12345678
        /// - 34 Life: B34/S34
        /// - Diamoeba: B35678/S5678
        /// - 2x2: B36/S125
        /// - HighLife: B36/S23
        /// - Day & Night: B3678/S34678
        /// - Morley's Move: B368/S245
        /// - Anneal: B4678/S35678
        /// </param>
        public Life(Bitmap bmp, string rule="B3/S23")
        {
            this.bmp = bmp;
            this.rule = this.StringToRule(rule);
            this.w = bmp.Width;
            this.h = bmp.Height;

            this.state = new List<BitArray>(bmp.Width);
            this.neighbors = new List<List<ushort>>(bmp.Width);

            for (int i = 0; i < bmp.Width; i++)
            {
                this.state.Add(new BitArray(bmp.Height));
                this.neighbors.Add(new List<ushort>(bmp.Height));
                for (int j = 0; j < bmp.Height; j++)
                {
                    this.state[i][j] = false;
                    this.neighbors[i].Add(0);
                    bmp.SetPixel(i, j, Color.FromArgb(0xff, 0xff, 0xff));
                }
            }
        }

        /// <summary>
        /// Constructor with initial state.
        /// </summary>
        /// <param name="bmp">Same as Life(Bitmap, string).</param>
        /// <param name="state">Initial state of the playing field. True: live cell, false: dead cell.</param>
        /// <param name="rule">Same as Life(Bitmap, string).</param>
        public Life(Bitmap bmp, List<BitArray> state, string rule = "B3/S23") :
            this(bmp, rule)
        {
            this.state = state;
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (state[i][j])
                        bmp.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    else
                        bmp.SetPixel(i, j, Color.FromArgb(0xff, 0xff, 0xff));
        }

        /// <summary>
        /// Updates the playing field's state. The naive implementation is slow (i.e. this is NOT an HashLife implementation), but allows the use of other rules than Conway's Game of Life.
        /// Tries to reduce the running time by parallelizing the count of neighbors for each cell and by drawing the state in a low level structure (BitmapData).
        /// </summary>
        public void Update()
        {
            Parallel.For(0, this.w, i =>
           {
               for (int j = 0; j < this.h; j++)
                   this.Notify(i, j);
           });

            Rectangle r = new Rectangle(0, 0, this.bmp.Width, this.bmp.Height);
            BitmapData bmpData = this.bmp.LockBits(r, ImageLockMode.WriteOnly, bmp.PixelFormat);
            IntPtr p = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * this.bmp.Height;
            byte[] rgb = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(p, rgb, 0, bytes);


            for (int i = 0; i < this.w; i++)
                for (int j = 0; j < this.h; j++)
                {
                    this.Update(i, j);
                    // Slow drawing:
                    // if (this.state[i][j])
                    //     this.bmp.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    // else
                    //     this.bmp.SetPixel(i, j, Color.FromArgb(0xff, 0xff, 0xff));
                    // Fast drawing:
                    if (this.state[i][j])
                    {
                        rgb[4 * (i + j * this.bmp.Width)] = 0x00; // B
                        rgb[4 * (i + j * this.bmp.Width) + 1] = 0x00; // G
                        rgb[4 * (i + j * this.bmp.Width) + 2] = 0x00; // R
                        rgb[4 * (i + j * this.bmp.Width) + 3] = 0xff; // A
                    }
                    else
                    {
                        rgb[4 * (i + j * this.bmp.Width)] = 0xff; // B
                        rgb[4 * (i + j * this.bmp.Width) + 1] = 0xff; // G
                        rgb[4 * (i + j * this.bmp.Width) + 2] = 0xff; // R
                        rgb[4 * (i + j * this.bmp.Width) + 3] = 0xff; // A
                    }
                    
                }
                

            System.Runtime.InteropServices.Marshal.Copy(rgb, 0, p, bytes);
            this.bmp.UnlockBits(bmpData);
                    
        }

        /// <summary>
        /// Updates the state of a single cell.
        /// </summary>
        /// <param name="i">Horizontal coordinate of the cell.</param>
        /// <param name="j">Vertical coordinate of the cell.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Update(int i, int j)
        {
            UInt16 workingSet;
             if (this.state[i][j]) // The cell was alive: follows the survive rules.
                 workingSet = (UInt16) ((this.rule >> 8) & 0xff);
             else // The cell was dead: follows the born rules.
                 workingSet = (UInt16) (this.rule & 0xff);

             this.state[i][j] = false;
             for (int k = 0; k < 8; k++)
                if (((int)workingSet & (1 << k)) != 0)
                    this.state[i][j] |= (this.neighbors[i][j] == k+1);

            this.neighbors[i][j] = 0;
        }
        /// <summary>
        /// Notifies each neighbor of a live cell. Faster than counting the number of neighbors for each cell (each cell is evaluated once, instead of eight times).
        /// </summary>
        /// <param name="i">Horizontal coordinate of the cell.</param>
        /// <param name="j">Vertical coordinate of the cell.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Notify(int i, int j)
        {
            if (this.state[i][j])
            {
                this.neighbors[(i + this.w - 1) % this.w][(j + this.h - 1) % this.h]++;
                this.neighbors[(i + this.w - 1) % this.w][j]++;
                this.neighbors[(i + this.w - 1) % this.w][(j + 1) % this.h]++;
                this.neighbors[i][(j + this.h - 1) % this.h]++;
                this.neighbors[i][(j + 1) % this.h]++;
                this.neighbors[(i + 1) % this.w][(j + this.h - 1) % this.h]++;
                this.neighbors[(i + 1) % this.w][j]++;
                this.neighbors[(i + 1) % this.w][(j + 1) % this.h]++;
            }
        }

    }
}
