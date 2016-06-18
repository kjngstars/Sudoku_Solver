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
        #region properties define 
        
        public enum HouseType
        {
            ROW = 1,
            COLUMN,
            BLOCK
        }

        public enum HeuristicType
        {
            NAKEDSINGLE,
            NAKEDSUBSET,
            INTERSECT
        }

        #endregion
        
        private static SudokuSolver instance = null;

        private bool currentPuzzleSolved = false;
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

        public bool IsPuzzleSolved()
        {
            return currentPuzzleSolved;
        }
        public void Reset()
        {            
            checkedHeuristic.Clear();
            currentPuzzleSolved = false;
        }

        #region sudoku solver method
        public BacktrackingResult SolveByBacktracking(Puzzle puzzle, BackgroundWorker worker, DoWorkEventArgs e)
        {

            if (worker.CancellationPending) 
            {
                e.Cancel = true;
                var result = new BacktrackingResult { Stopped = true };
                worker.ReportProgress(0, result);
                Thread.Sleep(10);
                return result;
            }

            if (puzzle.IsPuzzleSolved())
            {
                currentPuzzleSolved = true;
                return new BacktrackingResult { ReSolved = true };
            }

            var startSquare = puzzle.GetSquareToStart();

            foreach (var value in startSquare.PosibleCandidate)
            {
                if (!puzzle.IsValidCandidate(startSquare.Row, startSquare.Column, value))
                {
                    continue;

                }
                puzzle[startSquare.Row, startSquare.Column] = value;
                worker.ReportProgress(0, new BacktrackingResult { Guess = true, Backtracking = false, HandlingSquare = startSquare });
                Thread.Sleep(10);

                var result = SolveByBacktracking(puzzle, worker, e);

                if (result.Stopped) 
                {
                    return new BacktrackingResult { Stopped = true };
                }
                else if (result.ReSolved)
                {
                    return new BacktrackingResult { ReSolved = true };
                }
                else
                {
                    worker.ReportProgress(0, new BacktrackingResult { Guess = false, Backtracking = true, HandlingSquare = startSquare });
                    Thread.Sleep(10);
                    puzzle[startSquare.Row, startSquare.Column] = 0;
                }
            }

            return new BacktrackingResult { ReSolved = false };
        }
        public Puzzle SolveByHeuristic(Puzzle puzzle, BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return puzzle;
            }

            while (true)
            {
                if (puzzle.IsPuzzleSolved())
                {
                    worker.ReportProgress(0, new HeuristicResult { CurrentState = puzzle, ListSquareRelevant = new List<Square>() });
                    Thread.Sleep(10);
                    currentPuzzleSolved = true;
                    return puzzle;
                }

                HeuristicResult result = null;

                result = ApplyNakedTuple(puzzle);
                if (result.ReSolved == true)
                {
                    worker.ReportProgress(0, result);
                    Thread.Sleep(10);
                    continue;
                }

                result = ApplyIntersection(puzzle);
                if (result.ReSolved == true)
                {
                    worker.ReportProgress(0, result);
                    Thread.Sleep(10);
                    continue;
                }

                break;
            }

            #region turn around to backtracking if heuristic failed

            var startSquare = puzzle.GetBestSquareToStart();

            for (int i = 0; i < startSquare.PosibleCandidate.Count; i++)
            {
                int value = startSquare.PosibleCandidate[i];

                //remove guess value from list posible candidate
                startSquare.PosibleCandidate.Remove(value);
                i--;
                var tryState = puzzle.Clone();
                tryState[startSquare.Row, startSquare.Column] = value;
                var trySquare = tryState.GetSquare(startSquare.Row, startSquare.Column);
                UpdateNakedSingle(tryState, trySquare);

                //report progress
                worker.ReportProgress(0, new HeuristicResult { CurrentState = tryState, ListSquareRelevant = new List<Square> { trySquare }, BacktrackResult = new BacktrackingResult { Guess = true, HandlingSquare = tryState.GetSquare(startSquare.Row, startSquare.Column) } });
                Thread.Sleep(10);

                //recursion
                var resultState = SolveByHeuristic(tryState, worker, e);
                if (resultState.IsPuzzleSolved())
                {
                    worker.ReportProgress(0, new HeuristicResult { CurrentState = resultState, ListSquareRelevant = new List<Square>() });
                    Thread.Sleep(10);
                    currentPuzzleSolved = true;
                    return resultState;
                }
                else
                {
                    //backtracking
                    worker.ReportProgress(0, new HeuristicResult { BacktrackResult = new BacktrackingResult { Backtracking = true, HandlingSquare = startSquare } });
                    Thread.Sleep(10);
                }
            }
            #endregion

            return puzzle;
        }
            
        #endregion

        #region implement heuristic solving method
        
        #region naked subset
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
                    var desc = GetDescriptionProcess(new List<Square> { nakedSingle }, HeuristicType.NAKEDSINGLE);
                    return new HeuristicResult { ReSolved = true, ListSquareRelevant = new List<Square> { nakedSingle }, CurrentState = puzzle, Description = desc };
                }
            }

            return new HeuristicResult { ReSolved = false };
        }

        void UpdateNakedSingle(Puzzle puzzle, Square square)
        {
            UpdateNakedSingleHouse(puzzle, square, HouseType.ROW);
            UpdateNakedSingleHouse(puzzle, square, HouseType.COLUMN);
            UpdateNakedSingleHouse(puzzle, square, HouseType.BLOCK);
        }

        void UpdateNakedSingleHouse(Puzzle puzzle, Square square, HouseType type)
        {
            List<Square> listCandidate = null;
            if (type == HouseType.ROW)
            {
                listCandidate = puzzle.GetListCandidateInRow(square.Row);
            }
            else if (type == HouseType.COLUMN)
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
            if (result.ReSolved == true)
            {
                return result;
            }

            //naked pair
            result = NakedTuple(puzzle, 2);
            if (result.ReSolved == true)
            {
                return result;
            }

            //naked triple
            result = NakedTuple(puzzle, 3);
            if (result.ReSolved == true)
            {
                return result;
            }

            //naked quad
            result = NakedTuple(puzzle, 4);
            if (result.ReSolved == true)
            {
                return result;
            }

            //naked quint
            result = NakedTuple(puzzle, 5);
            if (result.ReSolved == true)
            {
                return result;
            }

            return (result = new HeuristicResult { ReSolved = false });
        }
        public HeuristicResult NakedTuple(Puzzle puzzle, int tuple)
        {
            //check in row
            for (int i = 0; i < 9; i++)
            {
                var result = NakedTupeByHouse(puzzle, tuple, i, HouseType.ROW);
                if (result.ReSolved == true)
                {
                    return result;
                }
            }

            //check in column
            for (int i = 0; i < 9; i++)
            {
                var result = NakedTupeByHouse(puzzle, tuple, i, HouseType.COLUMN);
                if (result.ReSolved == true)
                {
                    return result;
                }
            }

            //check in block
            for (int i = 0; i < 9; i++)
            {
                var result = NakedTupeByHouse(puzzle, tuple, i, HouseType.BLOCK);
                if (result.ReSolved == true)
                {
                    return result;
                }
            }

            return new HeuristicResult { ReSolved = false };
        }

        public void UpdateNakedTupe(Puzzle puzzle, List<Square> listNakedTuple)
        {
            if (listNakedTuple.All(square => square.Row == listNakedTuple[0].Row))
            {
                UpdateNakedTupeByHouse(puzzle, listNakedTuple, HouseType.ROW);
            }

            if (listNakedTuple.All(square => square.Column == listNakedTuple[0].Column))
            {
                UpdateNakedTupeByHouse(puzzle, listNakedTuple, HouseType.COLUMN);
            }

            if (listNakedTuple.All(square => square.Block == listNakedTuple[0].Block))
            {
                UpdateNakedTupeByHouse(puzzle, listNakedTuple, HouseType.BLOCK);
            }
        }

        HeuristicResult NakedTupeByHouse(Puzzle puzzle, int tuple, int index, HouseType type)
        {
            List<Square> listCandidate = null;
            if (type == HouseType.ROW)
            {
                listCandidate = puzzle.GetListCandidateInRow(index);
            }
            else if (type == HouseType.COLUMN)
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
                        var hash = GetHashHeuristic(nakedSquare, HeuristicType.NAKEDSUBSET);
                        if (checkedHeuristic.Contains(hash))
                        {
                            continue;
                        }
                        else
                        {
                            checkedHeuristic.Add(hash);
                        }
                        UpdateNakedTupe(puzzle, nakedSquare);
                        var desc = GetDescriptionProcess(nakedSquare, HeuristicType.NAKEDSUBSET);
                        return new HeuristicResult { ReSolved = true, ListSquareRelevant = nakedSquare, CurrentState = puzzle,Description = desc };
                    }
                }
            }
            return new HeuristicResult { ReSolved = false };
        }

        void UpdateNakedTupeByHouse(Puzzle puzzle, List<Square> listNakedTupe, HouseType type)
        {
            List<Square> listSquareToUpdate = null;

            if (type == HouseType.ROW)
            {
                var row = listNakedTupe[0].Row;
                listSquareToUpdate = puzzle.GetListCandidateInRow(row);
            }
            else if (type == HouseType.COLUMN)
            {
                var column = listNakedTupe[0].Column;
                listSquareToUpdate = puzzle.GetListCandidateInColumn(column);
            }
            else if (type == HouseType.BLOCK)
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

        #endregion

        #region Intersections heuristic

        public HeuristicResult ApplyIntersection(Puzzle puzzle)
        {
            HeuristicResult result = null;

            //check in block
            for (int i = 0; i < 9; i++)
            {
                result = Intersection(puzzle, HouseType.BLOCK, i);
                if (result.ReSolved == true)
                {
                    return result;
                }
            }

            //check in row
            for (int i = 0; i < 9; i++)
            {
                result = Intersection(puzzle, HouseType.ROW, i);
                if (result.ReSolved == true)
                {
                    return result;
                }
            }

            //check in column
            for (int i = 0; i < 9; i++)
            {
                result = Intersection(puzzle, HouseType.COLUMN, i);
                if (result.ReSolved == true)
                {
                    return result;
                }
            }

            return new HeuristicResult { ReSolved = false };
        }
        public HeuristicResult Intersection(Puzzle puzzle, HouseType houseType, int index)
        {
            List<Square> remainSquares = null;
            List<int> remainValues = null;

            if (houseType == HouseType.ROW)
            {
                remainSquares = puzzle.GetListCandidateInRow(index);
                remainValues = puzzle.GetRemainNumberInRow(index);
            }
            else if (houseType == HouseType.COLUMN)
            {
                remainSquares = puzzle.GetListCandidateInColumn(index);
                remainValues = puzzle.GetRemainNumberInColumn(index);
            }
            else
            {
                remainSquares = puzzle.GetListCandidateInBlock(index);
                remainValues = puzzle.GetRemainNumberInBlock(index);
            }

            for (int k = 0; k < remainValues.Count; k++)
            {
                int value = remainValues[k];
                var intersectionSquare = remainSquares.FindAll(square => square.PosibleCandidate.Contains(value));

                if (intersectionSquare.Count <= 3 && intersectionSquare.Count >= 2) 
                {
                    var heuristicHash = GetHashHeuristic(intersectionSquare, HeuristicType.INTERSECT);

                    if (houseType == HouseType.BLOCK) 
                    {
                        if (intersectionSquare.All(sq => sq.Row == intersectionSquare[0].Row))
                        {                            
                            if (checkedHeuristic.Contains(heuristicHash))
                            {
                                continue;
                            }
                            else
                            {
                                checkedHeuristic.Add(heuristicHash);
                            }

                            UpdateIntersection(puzzle, value, intersectionSquare, HouseType.ROW);
                            var desc = GetDescriptionProcess(intersectionSquare, HeuristicType.INTERSECT);
                            return new HeuristicResult { CurrentState = puzzle, ReSolved = true, ListSquareRelevant = intersectionSquare, Description = desc };
                        }
                        else if (intersectionSquare.All(sq => sq.Column == intersectionSquare[0].Column))
                        {
                            if (checkedHeuristic.Contains(heuristicHash))
                            {
                                continue;
                            }
                            else
                            {
                                checkedHeuristic.Add(heuristicHash);
                            }

                            UpdateIntersection(puzzle, value, intersectionSquare, HouseType.COLUMN);
                            var desc = GetDescriptionProcess(intersectionSquare, HeuristicType.INTERSECT);
                            return new HeuristicResult { CurrentState = puzzle, ReSolved = true, ListSquareRelevant = intersectionSquare, Description = desc };
                        }
                    }
                    else
                    {
                        if (intersectionSquare.All(sq => sq.Block == intersectionSquare[0].Block))
                        {
                            if (checkedHeuristic.Contains(heuristicHash))
                            {
                                continue;
                            }
                            else
                            {
                                checkedHeuristic.Add(heuristicHash);
                            }

                            UpdateIntersection(puzzle, value, intersectionSquare, HouseType.BLOCK);
                            var desc = GetDescriptionProcess(intersectionSquare, HeuristicType.INTERSECT);
                            return new HeuristicResult { CurrentState = puzzle, ReSolved = true, ListSquareRelevant = intersectionSquare, Description = desc };
                        }
                    }
                }
            }

            return new HeuristicResult { ReSolved = false };
        }

        void UpdateIntersection(Puzzle puzzle, int value, List<Square> intersection, HouseType houseType)
        {
            List<Square> listRemainSquare = null;

            if (houseType == HouseType.BLOCK)
            {
                listRemainSquare = puzzle.GetListCandidateInBlock(intersection[0].Block);
                listRemainSquare.RemoveAll(sq => intersection.Contains(sq));
                
            }
            else if (houseType == HouseType.ROW)
            {
                listRemainSquare = puzzle.GetListCandidateInRow(intersection[0].Row);
                listRemainSquare.RemoveAll(sq => intersection.Contains(sq));                
            }
            else
            {
                listRemainSquare = puzzle.GetListCandidateInColumn(intersection[0].Column);
                listRemainSquare.RemoveAll(sq => intersection.Contains(sq)); 
            }

            for (int i = 0; i < listRemainSquare.Count; i++)
            {
                listRemainSquare[i].PosibleCandidate.Remove(value);
            }
        }        

        #endregion

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

        string GetHashHeuristic(List<Square> listPuzzle, HeuristicType heuristic)
        {
            string hash = heuristic.ToString();
            foreach (var item in listPuzzle)
            {
                hash += item.Row.ToString() + item.Column.ToString();
            }

            return hash;
        }

        string GetDescriptionProcess(List<Square> listPuzzle, HeuristicType heuristic)
        {
            string square = "r{0}c{1}";
            string description = heuristic.ToString() + ": ";

            foreach (var item in listPuzzle)
            {
                var str = string.Format(square, item.Row, item.Column);
                description += str + ", ";
            }
            description = description.Remove(description.Length - 2);
            description += "\n";
            return description;
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
