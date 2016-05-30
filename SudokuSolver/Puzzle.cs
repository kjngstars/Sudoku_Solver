using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    [Serializable()]
    public class Puzzle : ISerializable
    {
        private List<Square> sudokuBoard = new List<Square>();
        public Puzzle(){ }
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

                    sudokuBoard.Add(square);
                }
            }

            foreach (Square square in sudokuBoard)
            {
                if (square.Value == 0) 
                {
                    square.PosibleCandidate = GetPosibleCandidateInSquare(square.Row, square.Column);
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
        public Square GetSquare(int row, int column)
        {
            return sudokuBoard.Find(square => square.Row == row && square.Column == column);
        }
        public List<int> GetProvidedNumberInRow(int row)
        {
            var listSquare = sudokuBoard.FindAll(square => square.Value != 0 && square.Row == row);
            var result = listSquare.Select(square => square.Value).ToList();
            return result;
        }
        public List<int> GetRemainNumberInRow(int row)
        {
            var providedNumberInRow = GetProvidedNumberInRow(row);
            var remainNumberInrow = SudokuInfo.Instance.ListValidCandidate().Except(providedNumberInRow).ToList();
            return remainNumberInrow;
        }
        public List<int> GetProvidedNumberInColumn(int column)
        {
            var listSquare = sudokuBoard.FindAll(square => square.Value != 0 && square.Column == column);
            var result = listSquare.Select(square => square.Value).ToList();
            return result;
        }
        public List<int> GetRemainNumberInColumn(int column)
        {
            var providedNumberInColumn = GetProvidedNumberInColumn(column);
            var remainNumberInColumn = SudokuInfo.Instance.ListValidCandidate().Except(providedNumberInColumn).ToList();
            return remainNumberInColumn;
        }
        public List<int> GetProvidedNumberInBlock(int block)
        {
            var listSquare = sudokuBoard.FindAll(square => square.Value != 0 && square.Block == block);
            var result = listSquare.Select(square => square.Value).ToList();
            return result;
        }
        public List<int> GetRemainNumberInBlock(int block)
        {
            var providedNumberInBlock= GetProvidedNumberInBlock(block);
            var remainNumberInBlock = SudokuInfo.Instance.ListValidCandidate().Except(providedNumberInBlock).ToList();
            return remainNumberInBlock;
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
        public List<Square> GetListCandidateInRow(int row)
        {
            return sudokuBoard.FindAll
                (
                    square =>
                    square.Row == row &&
                    square.Value == 0
                );
        }
        public List<Square> GetListCandidateInColumn(int column)
        {
            return sudokuBoard.FindAll(square => square.Column == column && square.Value == 0);
        }
        public List<Square> GetListCandidateInBlock(int block)
        {
            return sudokuBoard.FindAll
                    (
                        square =>
                        square.Block == block &&
                        square.Value == 0
                    );
       } 
        public Square GetBestSquareToStart()
        {
            var listSquareNeedToFind = sudokuBoard.FindAll(square => square.Value == 0).ToList();
            var minPosibleCandidate = listSquareNeedToFind.Min(square => square.PosibleCandidate.Count);
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

        #region update method

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

        #region serialize

        public Puzzle(SerializationInfo info, StreamingContext context)
        {
            sudokuBoard = (List<Square>)info.GetValue("sudokuBoard", typeof(List<Square>));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sudokuBoard", sudokuBoard);
        }

        #endregion
    }
}
