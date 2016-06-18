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
using System.Reflection;

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
            Pen p = new Pen(SystemColors.ControlDark, 2);

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
            richTextBoxHint.Clear();
            
            SudokuSolver.Instance.Reset();
            puzzle = new Puzzle(board);
            ShowFilledSquare(puzzle);

        }

        private void btnSolveBacktracking_Click(object sender, EventArgs e)
        {
            if (puzzle != null) 
            {
                bgBacktracking.RunWorkerAsync(puzzle);
            }
            else
            {
                MessageBox.Show("You must add Sudoku Puzzle first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSolveHeuristic_Click(object sender, EventArgs e)
        {
            if (puzzle != null) 
            {
                bgHeuristic.RunWorkerAsync(puzzle);  
            }
            else
            {
                MessageBox.Show("You must add Sudoku Puzzle first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void btnShowRemainNumber_Click(object sender, EventArgs e)
        {
            ShowRemainNumberCandidate(puzzle);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SudokuSolver.Instance.Reset();
            foreach (var control in tlpBoard.Controls)
            {
                TextBox tb = (TextBox)control;
                SetDefaultAppearance(tb);
                tb.Text = "";
            }

            richTextBoxHint.Clear();
            richTextBoxResult.Clear();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int[,] board = new int[9, 9];
            foreach (var control in tlpBoard.Controls)
            {
                TextBox tb = (TextBox)control;
                if (tb.Text != "")
                {
                    string name = tb.Name;
                    string index = name.Substring(name.Length - 2);
                    int r = int.Parse(index[0].ToString());
                    int c = int.Parse(index[1].ToString());
                    board[r, c] = int.Parse(tb.Text);
                }
            }

            puzzle = new Puzzle(board);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (bgBacktracking.IsBusy)
            {
                bgBacktracking.CancelAsync();
            }
            else if (bgHeuristic.IsBusy)
            {
                bgHeuristic.CancelAsync();
            }
        }
        private void richTextBoxHint_TextChanged(object sender, EventArgs e)
        {
            richTextBoxHint.SelectionStart = richTextBoxHint.Text.Length;
            richTextBoxHint.ScrollToCaret();
        }

        private void richTextBoxResult_TextChanged(object sender, EventArgs e)
        {
            richTextBoxResult.SelectionStart = richTextBoxResult.Text.Length;
            richTextBoxResult.ScrollToCaret();
        }

        #endregion

        #region helper method: update, check...
        void SetDefaultAppearance(Control textBox)
        {
            textBox.Font = new Font("Segoe UI", 18.0f, FontStyle.Bold);
            textBox.ForeColor = Color.Orange;
        }
        string GetNumberCandidateText(List<int> listNumber)
        {
            int count = 0;
            string result = "";
            foreach (var item in listNumber)
            {
                result += item.ToString() + " ";
                count++;
                if (count == 3)
                {
                    result += Environment.NewLine;
                    count = 0;
                }
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
                    textBox[0].Font = new Font(textBox[0].Font.FontFamily, 9.0f);
                    textBox[0].ForeColor = Color.DarkBlue;
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
                SetDefaultAppearance(textBox);
            }

            string prefixName = "textBox";
            var board = puzzle.GetBoard();

            foreach (var square in board)
            {
                prefixName += square.Row.ToString() + square.Column.ToString();
                var textBox = tlpBoard.Controls.Find(prefixName, true);

                if (square.Value != 0)
                {
                    SetDefaultAppearance(textBox[0]);
                    textBox[0].Text = square.Value.ToString();
                }

                prefixName = "textBox";
            }


        }
        void UpdateBacktracking(BacktrackingResult result)
        {
            if (result.Stopped)
            {
                richTextBoxHint.AppendText("Stop backtracking...!");
                return;
            }

            string textBoxName = "textBox" + result.HandlingSquare.Row.ToString() + result.HandlingSquare.Column.ToString();
            var textBox = tlpBoard.Controls.Find(textBoxName, true);
            textBox[0].Font = new Font(textBox[0].Font.FontFamily, 18.0f);
            textBox[0].ForeColor = Color.Black;

            if (result.Guess)
            {
                
                var value = result.HandlingSquare.Value;
                if (value != 0) 
                {
                    textBox[0].Text = value.ToString();
                }
                
                //update process
                string sqr = "r{0}c{1}";
                var str = string.Format(sqr, result.HandlingSquare.Row.ToString(), result.HandlingSquare.Column.ToString());
                string text = "Guess " + result.HandlingSquare.Value + " at square " + str + "\n";
                richTextBoxHint.AppendText(text);

            }
            else if (result.Backtracking) 
            {
                textBox[0].Text = "";
                string text = "Backtracking" + "\n";
                richTextBoxHint.AppendText(text);
            }
            
        }
        void UpdateSquareHeuristic(HeuristicResult result)
        {
            if (result.CurrentState != null)
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
                        textBox[0].Font = new Font(textBox[0].Font.FontFamily, 9.0f);
                        textBox[0].ForeColor = Color.DarkBlue;
                        textBox[0].Text = numbersCandidate;
                    }
                }
            }
        }
        void UpdateProcess(HeuristicResult result)
        {
            if (result.Description != null)
            {
                richTextBoxHint.AppendText(result.Description);
            }

        }

        #endregion

        #region backtracking background task

        private void bgBacktracking_DoWork(object sender, DoWorkEventArgs e)
        {
            watch = new Stopwatch();
            watch.Start();
            var puzzle = (Puzzle)e.Argument;
            SudokuSolver.Instance.SolveByBacktracking(puzzle, bgBacktracking, e);
            watch.Stop();
        }

        private void bgBacktracking_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var result = (BacktrackingResult)e.UserState;
            UpdateBacktracking(result);

        }

        private void bgBacktracking_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var timePass = (double)watch.ElapsedMilliseconds / 1000;
            var result = "✔ Time take: " + timePass.ToString() + " Seconds\n";
            richTextBoxResult.AppendText(result);
            if (SudokuSolver.Instance.IsPuzzleSolved())
            {          
                richTextBoxHint.AppendText("Puzzle solved...!");
                puzzle = null;
            }
        }

        #endregion

        #region heuristic background task

        private void bgHeuristic_DoWork(object sender, DoWorkEventArgs e)
        {
            watch = new Stopwatch();
            watch.Start();
            var puzzle = (Puzzle)e.Argument;
            var result = SudokuSolver.Instance.SolveByHeuristic(puzzle, bgHeuristic, e);
            watch.Stop();
        }

        private void bgHeuristic_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            HeuristicResult result = (HeuristicResult)e.UserState;
            UpdateSquareHeuristic(result);
            UpdateProcess(result);
            if (result.BacktrackResult != null)
            {
                UpdateBacktracking(result.BacktrackResult);
                puzzle = null;
            }

        }

        private void bgHeuristic_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var timePass = (double)watch.ElapsedMilliseconds / 1000;
            var result = "✔ Time take: " + timePass.ToString() + " Seconds\n";
            richTextBoxResult.AppendText(result);
            if (SudokuSolver.Instance.IsPuzzleSolved())
            {
                richTextBoxHint.AppendText("Puzzle solved...!");
                puzzle = null;
            }
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            typeof(TableLayoutPanel).InvokeMember("DoubleBuffered",
    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
    null, tlpBoard, new object[] { true });
        }
       
    }
}
