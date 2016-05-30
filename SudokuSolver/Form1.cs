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
using System.Diagnostics;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {
        #region properties define
        Stopwatch watch = null;

        Dictionary<int, int[,]> sudokuPuzzles = new Dictionary<int, int[,]>();
        Random rand = new Random();
        Puzzle puzzle = null;    

        #endregion
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
            var index = rand.Next(1, sudokuPuzzles.Count + 1);
            var board = sudokuPuzzles[index];

            SudokuSolver.Instance.Reset();
            puzzle = new Puzzle(board);
            ShowFilledSquare(puzzle);
            
        }

        private void btnSolveBacktracking_Click(object sender, EventArgs e)
        {
            bgBacktracking.RunWorkerAsync(puzzle);
        }

        private void btnSolveHeuristic_Click(object sender, EventArgs e)
        {
            bgHeuristic.RunWorkerAsync(puzzle);
            
        }
        private void btnShowRemainNumber_Click(object sender, EventArgs e)
        {
            ShowRemainNumberCandidate(puzzle);
        }

        #endregion

        #region helper method: update, check...
        void SetTextDefaultAppearance(Control textBox)
        {
            textBox.Font = new Font("Times New Roman",18.0f, FontStyle.Bold);
            textBox.ForeColor = Color.Orange;
        }
        string GetNumberCandidateText(List<int> listNumber)
        {
            string result = "";
            foreach (var item in listNumber)
            {
                result += item.ToString() + " ";
            }
            return result;
        }
        void ShowRemainNumberCandidate(Puzzle puzzle)
        {
            foreach (Square square in puzzle.GetBoard())
            {
                if (square.PosibleCandidate != null) 
                {
                    string name = "textBox" + square.Row.ToString() + square.Column.ToString();
                    var textBox = tlpBoard.Controls.Find(name, true);
                    textBox[0].Font = new Font(textBox[0].Font.FontFamily, 8.0f);
                    textBox[0].Text = GetNumberCandidateText(square.PosibleCandidate);
                }
            }
        }
        void ShowFilledSquare(Puzzle puzzle)
        {
            //clear all current spot
            foreach (var control in tlpBoard.Controls)
            {
                var textBox = (TextBox)control;
                textBox.Text = "";
            }

            string prefixName = "textBox";
            var board = puzzle.GetBoard();

            foreach (var square in board)
            {
                prefixName += square.Row.ToString() + square.Column.ToString();
                var textBox = tlpBoard.Controls.Find(prefixName, true);

                if (square.Value != 0) 
                {
                    SetTextDefaultAppearance(textBox[0]);
                    textBox[0].ForeColor = Color.Orange;
                    textBox[0].Text = square.Value.ToString();
                }
                
                prefixName = "textBox";
            }

            
        }
        void UpdateSquare(Square square)
        {
            string textBoxName = "textBox" + square.Row.ToString() + square.Column.ToString();
            var textBox = tlpBoard.Controls.Find(textBoxName, true);
            if (square.Value != 0) 
            {
                textBox[0].Text = square.Value.ToString();
            }
            else
            {
                textBox[0].Text = "";
            }
        }
        void UpdateSquareHeuristic(HeuristicResult result)
        {
            Puzzle currentState = result.CurrentState;
            var listSquareToUpdate = currentState.GetBoard().FindAll(square => square.PosibleCandidate != null);

            foreach (Square square in listSquareToUpdate)
            {
                if (square.Value != 0)
                {
                    var name = "textBox" + square.Row.ToString() + square.Column.ToString();
                    var textBox = tlpBoard.Controls.Find(name, true);
                    textBox[0].Font = new Font(textBox[0].Font.FontFamily, 18.0f);
                    textBox[0].ForeColor = Color.Black;
                    textBox[0].Text = square.Value.ToString();
                }
                else
                {
                    var name = "textBox" + square.Row.ToString() + square.Column.ToString();
                    var textBox = tlpBoard.Controls.Find(name, true);
                    var numbersCandidate = GetNumberCandidateText(square.PosibleCandidate);
                    textBox[0].Font = new Font(textBox[0].Font.FontFamily, 8.0f);
                    textBox[0].ForeColor = Color.Green;
                    textBox[0].Text = numbersCandidate;
                }
            }

        }

        #endregion

        #region backtracking background task

        private void bgBacktracking_DoWork(object sender, DoWorkEventArgs e)
        {
            watch = new Stopwatch();
            watch.Start();
            var puzzle = (Puzzle)e.Argument;
            SudokuSolver.Instance.SolveByBacktracking(puzzle, bgBacktracking);
            watch.Stop();
        }

        private void bgBacktracking_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var square = (Square)e.UserState;
            UpdateSquare(square);

        }

        private void bgBacktracking_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var timePass = (double)watch.ElapsedMilliseconds / 1000;
            var result = "✔ Time take: " + timePass.ToString() + " Seconds\n";
            richTextBoxResult.AppendText(result);
        }

        #endregion      

        #region heuristic background task

        private void bgHeuristic_DoWork(object sender, DoWorkEventArgs e)
        {
            watch = new Stopwatch();
            watch.Start();
            var puzzle = (Puzzle)e.Argument;
            var result = SudokuSolver.Instance.SolveByHeuristic(puzzle,bgHeuristic);
            watch.Stop();
        }

        private void bgHeuristic_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            HeuristicResult result =(HeuristicResult)e.UserState;
            UpdateSquareHeuristic(result);
                 
        }

        private void bgHeuristic_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var timePass = (double)watch.ElapsedMilliseconds / 1000;
            var result = "✔ Time take: " + timePass.ToString() + " Seconds\n";
            richTextBoxResult.AppendText(result);
        }

        #endregion

        
    }
}
