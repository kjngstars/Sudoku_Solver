using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public class Puzzle
    {
        private List<Square> sudokuBoard = new List<Square>();

        public Puzzle(int[,] board)
        {
            
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var square = new Square
                    {
                        Value = board[i, j],
                        Row = i,
                        Column = j,
                        Block = SudokuInfo.Instance.GetBlock(i, j),                        
                    };

                    if (board[i, j] == 0)  
                    {
                        square.PosibleCandidate = GetPosibleCandidateInSquare(i, j);
                    }

                    sudokuBoard.Add(square);
                }
            }
        }
        
        public int this[int row, int column]
        {
            get
            {
                return sudokuBoard.Find(square => square.Row == row && square.Column == column).Value;
            }

            set
            {
                var sq = sudokuBoard.Find(square => square.Row == row && square.Column == column);
                sq.Value = value;
            }
        }

        #region get method
        public List<Square> GetBoard()
        {
            return sudokuBoard;
        }
        public List<int> GetProvidedNumberInRow(int row)
        {
            var listSquare = sudokuBoard.FindAll(square => square.Value != 0 && square.Row == row);
            var result = listSquare.Select(square => square.Value).ToList();
            return result;
        }
        public List<int> GetProvidedNumberInColumn(int column)
        {
            var listSquare = sudokuBoard.FindAll(square => square.Value != 0 && square.Column == column);
            var result = listSquare.Select(square => square.Value).ToList();
            return result;
        }
        public List<int> GetProvidedNumberInBlock(int block)
        {
            var listSquare = sudokuBoard.FindAll(square => square.Value != 0 && square.Block == block);
            var result = listSquare.Select(square => square.Value).ToList();
            return result;
        }
        public List<int> GetPosibleCandidateInSquare(int row,int column)
        {
            var preRowNumber = GetProvidedNumberInRow(row);
            var preColumnNumber = GetProvidedNumberInColumn(column);

            var block = SudokuInfo.Instance.GetBlock(row, column);
            var preBlockNumber = GetProvidedNumberInBlock(block);

            var existNumberOnSquare = (preRowNumber.Union(preColumnNumber)).Union(preBlockNumber).ToList();

            var remainCandidate = SudokuInfo.Instance.ListValidCandidate().Except(existNumberOnSquare).ToList();

            return remainCandidate;
        }
        public Square GetBestSquareToStart()
        {
            var listSquareNeedToFind = sudokuBoard.FindAll(square => square.Value == 0).ToList();

            var minPosibleCandidate = listSquareNeedToFind.Min(square => square.PosibleCandidate.Count);
            //var minPosibleCandidate = sudokuBoard.Min(square => square.PosibleCandidate.Count);
            var result = listSquareNeedToFind.Find(square => square.PosibleCandidate.Count == minPosibleCandidate);
            
            return result;
        }

        public Square GetSquareToStart()
        {
            for (int i = 0; i < 9; i++)
            {
                var listSquareInRow = sudokuBoard.FindAll(square => square.Row == i);
                var start = listSquareInRow.Find(sq => sq.Value == 0);
                if (start != null) 
                {
                    return start;
                }
                
            }
            return null;
        }

        #endregion

        #region helper method
        public bool IsPuzzleSolved()
        {
            return sudokuBoard.All(square => square.Value != 0);
        }

        public bool IsValidCandidate(int row, int column, int value)
        {

            var numberAlreadyInRow = GetProvidedNumberInRow(row);
            var numberAlreadyInColumn = GetProvidedNumberInColumn(column);
            var block = SudokuInfo.Instance.GetBlock(row, column);
            var numberAlreadyInBlock = GetProvidedNumberInBlock(block);

            if (numberAlreadyInRow.Contains(value) ||
                numberAlreadyInColumn.Contains(value) ||
                numberAlreadyInBlock.Contains(value))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
