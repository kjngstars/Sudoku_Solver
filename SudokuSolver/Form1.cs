using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel2_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            Pen p = new Pen(SystemColors.ControlDark,2);

            if (e.Column == 0 || e.Column == 3 || e.Column == 6)
                e.Graphics.DrawLine(p, new Point(e.CellBounds.X, e.CellBounds.Y), new Point(e.CellBounds.X, e.CellBounds.Bottom));

            if (e.Row == 0 || e.Row == 3 || e.Row == 6)
                e.Graphics.DrawLine(p, new Point(e.CellBounds.X, e.CellBounds.Y), new Point(e.CellBounds.Right, e.CellBounds.Y));

            if (e.Row == 8) 
            {
                e.Graphics.DrawLine(p, new Point(e.CellBounds.X, e.CellBounds.Bottom), new Point(e.CellBounds.Right, e.CellBounds.Bottom));
            }

            if (e.Column == 8) 
            {
                e.Graphics.DrawLine(p, new Point(e.CellBounds.Right, e.CellBounds.Y), new Point(e.CellBounds.Right, e.CellBounds.Bottom));
            }

        }
    }
}
