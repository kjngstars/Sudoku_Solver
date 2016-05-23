using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {

        Dictionary<int, int[,]> sudokuPuzzles = new Dictionary<int, int[,]>();
        Random rand = new Random();
        Puzzle puzzle = null;
        
        public Form1()
        {
            InitializeComponent();
        }


        #region form event
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

        private void Form1_Shown(object sender, EventArgs e)
        {
            using (StreamReader rd = new StreamReader("sudoku.txt"))
            {
                char emptyDigit = '-';
                string firstLine = rd.ReadLine();
                int index, count = 0;
                int[,] puzzle = null;

                while (firstLine != null) 
                {
                    if (firstLine.StartsWith("#")) 
                    {
                        index = count = 0;
                        puzzle = new int[9, 9];                        
                        var indexString = firstLine.Substring(1);
                        index = int.Parse(indexString);
                        sudokuPuzzles.Add(index, puzzle);
                    }

                    if (firstLine.StartsWith("-") || char.IsDigit(firstLine[0]))
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            if (firstLine[i] != emptyDigit) 
                            {
                                if (puzzle != null) 
                                {
                                    puzzle[count, i] = (int)char.GetNumericValue(firstLine[i]);
                                }
                                
                            }
                        }
                        count++;
                    }
                    firstLine = rd.ReadLine();
                }
            }
               
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var index = rand.Next(1, sudokuPuzzles.Count);
            var board = sudokuPuzzles[index];

            puzzle = new Puzzle(board);

            updateBoardSquare(puzzle);
            
        }

        private void btnSolveBacktracking_Click(object sender, EventArgs e)
        {

        }

        private void btnSolveHeuristic_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region helper function

        void updateBoardSquare(Puzzle puzzle)
        {
            string prefixName = "textBox";
            var board = puzzle.GetBoard();

            foreach (var square in board)
            {
                prefixName += square.row.ToString() + square.column.ToString();
                var textBox = tlpBoard.Controls.Find(prefixName, true);

                if (square.value != 0) 
                {
                    textBox[0].Text = square.value.ToString();
                }
                
                prefixName = "textBox";
            }


        }

        #endregion


    }
}
