using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public static class SudokuSolver
    {
        public static void SolveByBacktracking(Puzzle puzzle, BackgroundWorker worker)
        {
            if (puzzle.IsPuzzleSolved())
            {
                return;
            }

            var startSquare = puzzle.GetSquareToStart();

            foreach (var value in startSquare.PosibleCandidate)
            {
                if (!puzzle.IsValidCandidate(startSquare.Row, startSquare.Column, value)) 
                {
                    continue;

                }
                puzzle[startSquare.Row, startSquare.Column] = value;
                worker.ReportProgress(0, startSquare);
                Thread.Sleep(10);

                SolveByBacktracking(puzzle, worker);

                if (puzzle.IsPuzzleSolved()) 
                {
                    return;
                }
                else
                {
                    puzzle[startSquare.Row, startSquare.Column] = 0;
                }
            }

            return;
        }

    }
}
