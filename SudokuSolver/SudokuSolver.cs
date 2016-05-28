using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public class SudokuSolver
    {
        private static SudokuSolver instance = null;
        public static SudokuSolver Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SudokuSolver();
                    return instance;
                }
                return instance;
            }
        }

        private List<String> checkedHeuristic = new List<String>();

        //prevent create new instance
        private SudokuSolver() { }
        public enum Type
        {
            ROW = 1,
            COLUMN,
            BLOCK
        }

        #region sudoku solver method
        public void SolveByBacktracking(Puzzle puzzle, BackgroundWorker worker)
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
        public Puzzle SolveByHeuristic(Puzzle puzzle, BackgroundWorker worker)
        {
            while (true)
            {
                if (puzzle.IsPuzzleSolved())
                {
                    return puzzle;
                }

                HeuristicResult result = null;

                result = ApplyNakedTuple(puzzle);
                if (result.Resolved == true)
                {
                    worker.ReportProgress(0, puzzle);
                    Thread.Sleep(10);
                    continue;
                }
                break;
            }

            #region turn around to backtracking
            var startSquare = puzzle.GetBestSquareToStart();

            for (int i = 0; i < startSquare.PosibleCandidate.Count; i++)
            {
                int value = startSquare.PosibleCandidate[i];

                var tryState = puzzle.Clone();
                tryState[startSquare.Row, startSquare.Column] = value;
                var trySquare = tryState.GetSquare(startSquare.Row, startSquare.Column);
                UpdateNakedSingle(tryState, trySquare);
                //report progress
                //worker.ReportProgress(0, trySquare);
                //Thread.Sleep(10);
                var resultState = SolveByHeuristic(tryState, worker);
                if (resultState.IsPuzzleSolved())
                {
                    return resultState;
                }
            }
            #endregion

            return puzzle;
        }

        #endregion

        #region implement heuristic

        //naked subset
        public HeuristicResult NakedSingle(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                var listCandidateInRow = puzzle.GetListCandidateInRow(i);
                var nakedSingle = listCandidateInRow.Find(square => square.PosibleCandidate.Count == 1);
                if (nakedSingle != null)
                {
                    nakedSingle.Value = nakedSingle.PosibleCandidate[0];
                    UpdateNakedSingle(puzzle, nakedSingle);
                    return new HeuristicResult { Resolved = true, ListSquare = new List<Square> { nakedSingle } };
                }
            }

            return new HeuristicResult { Resolved = false };
        }

        void UpdateNakedSingle(Puzzle puzzle, Square square)
        {
            UpdateNakedSingleHouse(puzzle, square, Type.ROW);
            UpdateNakedSingleHouse(puzzle, square, Type.COLUMN);
            UpdateNakedSingleHouse(puzzle, square, Type.BLOCK);
        }

        void UpdateNakedSingleHouse(Puzzle puzzle, Square square, Type type)
        {
            List<Square> listCandidate = null;
            if (type == Type.ROW)
            {
                listCandidate = puzzle.GetListCandidateInRow(square.Row);
            }
            else if (type == Type.COLUMN)
            {
                listCandidate = puzzle.GetListCandidateInColumn(square.Column);
            }
            else
            {
                listCandidate = puzzle.GetListCandidateInBlock(square.Block);
            }

            for (int i = 0; i < listCandidate.Count; i++)
            {
                listCandidate[i].PosibleCandidate.Remove(square.Value);
            }
        }

        public HeuristicResult ApplyNakedTuple(Puzzle puzzle)
        {
            HeuristicResult result = null;

            //naked single
            result = NakedSingle(puzzle);
            if (result.Resolved == true)
            {
                return result;
            }

            //naked pair
            result = NakedTuple(puzzle, 2);
            if (result.Resolved == true)
            {
                return result;
            }

            //naked triple
            result = NakedTuple(puzzle, 3);
            if (result.Resolved == true)
            {
                return result;
            }

            //naked quad
            result = NakedTuple(puzzle, 4);
            if (result.Resolved == true)
            {
                return result;
            }

            //naked quint
            result = NakedTuple(puzzle, 5);
            if (result.Resolved == true)
            {
                return result;
            }

            return (result = new HeuristicResult { Resolved = false });
        }
        public HeuristicResult NakedTuple(Puzzle puzzle, int tuple)
        {
            //check in row
            for (int i = 0; i < 9; i++)
            {
                var result = NakedTupeByHouse(puzzle, tuple, i, Type.ROW);
                if (result.Resolved == true)
                {
                    return result;
                }
            }

            //check in column
            for (int i = 0; i < 9; i++)
            {
                var result = NakedTupeByHouse(puzzle, tuple, i, Type.COLUMN);
                if (result.Resolved == true)
                {
                    return result;
                }
            }

            //check in block
            for (int i = 0; i < 9; i++)
            {
                var result = NakedTupeByHouse(puzzle, tuple, i, Type.BLOCK);
                if (result.Resolved == true)
                {
                    return result;
                }
            }

            return new HeuristicResult { Resolved = false };
        }

        public void UpdateNakedTupe(Puzzle puzzle, List<Square> listNakedTuple)
        {
            if (listNakedTuple.All(square => square.Row == listNakedTuple[0].Row))
            {
                UpdateNakedTupeByHouse(puzzle, listNakedTuple, Type.ROW);
            }

            if (listNakedTuple.All(square => square.Column == listNakedTuple[0].Column))
            {
                UpdateNakedTupeByHouse(puzzle, listNakedTuple, Type.COLUMN);
            }

            if (listNakedTuple.All(square => square.Block == listNakedTuple[0].Block))
            {
                UpdateNakedTupeByHouse(puzzle, listNakedTuple, Type.BLOCK);
            }
        }

        #endregion

        #region helper method

        List<int> GetUnionElement(List<Square> listSquare)
        {
            List<int> result = new List<int>();
            foreach (var item in listSquare)
            {
                result = result.Union(item.PosibleCandidate).ToList();
            }
            return result;
        }

        void UpdateNakedTupeByHouse(Puzzle puzzle, List<Square> listNakedTupe, Type type)
        {
            List<Square> listSquareToUpdate = null;

            if (type == Type.ROW)
            {
                var row = listNakedTupe[0].Row;
                listSquareToUpdate = puzzle.GetListCandidateInRow(row);
            }
            else if (type == Type.COLUMN)
            {
                var column = listNakedTupe[0].Column;
                listSquareToUpdate = puzzle.GetListCandidateInColumn(column);
            }
            else if (type == Type.BLOCK)
            {
                var block = listNakedTupe[0].Block;
                listSquareToUpdate = puzzle.GetListCandidateInBlock(block);
            }

            listSquareToUpdate.RemoveAll(square => listNakedTupe.Contains(square));
            var listNumberToRemove = GetUnionElement(listNakedTupe);

            for (int i = 0; i < listSquareToUpdate.Count; i++)
            {
                listSquareToUpdate[i].PosibleCandidate.RemoveAll(value => listNumberToRemove.Contains(value));
            }
        }
        HeuristicResult NakedTupeByHouse(Puzzle puzzle, int tuple, int index, Type type)
        {
            List<Square> listCandidate = null;
            if (type == Type.ROW)
            {
                listCandidate = puzzle.GetListCandidateInRow(index);
            }
            else if (type == Type.COLUMN)
            {
                listCandidate = puzzle.GetListCandidateInColumn(index);
            }
            else
            {
                listCandidate = puzzle.GetListCandidateInBlock(index);
            }

            var listPosibleNakedTupe = listCandidate.FindAll
                (
                    square => square.PosibleCandidate.Count <= tuple &&
                    square.PosibleCandidate.Count > 1
                );

            if (listPosibleNakedTupe.Count >= tuple)
            {

                var union = GetUnionElement(listPosibleNakedTupe);
                var posibleNakedNumbers = GetKCombs(union, tuple).ToList();

                foreach (var combination in posibleNakedNumbers)
                {
                    var nakedSquare = listPosibleNakedTupe.FindAll(square => ContainsAllItems(combination.ToList(), square.PosibleCandidate));

                    if (nakedSquare.Count == tuple)
                    {
                        var hash = GetHashHeuristic(nakedSquare);
                        if (checkedHeuristic.Contains(hash))
                        {
                            continue;
                        }
                        else
                        {
                            checkedHeuristic.Add(hash);
                        }
                        UpdateNakedTupe(puzzle, nakedSquare);
                        return new HeuristicResult { Resolved = true, ListSquare = nakedSquare };
                    }
                }
            }
            return new HeuristicResult { Resolved = false };
        }

        string GetHashHeuristic(List<Square> listPuzzle)
        {
            string hash = "";
            foreach (var item in listPuzzle)
            {
                hash += item.Row.ToString() + item.Column.ToString();
            }

            return hash;
        }

        public static bool ContainsAllItems<T>(List<T> a, List<T> b)
        {
            return !b.Except(a).Any();
        }

        //get all combinations: nCk
        IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        #endregion
    }
}
