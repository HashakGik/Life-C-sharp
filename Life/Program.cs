using System;
using System.Windows.Forms;

/// <summary>
/// This program implements Conway's Game of Life and other Life-Like automata.
/// </summary>
namespace Life
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
