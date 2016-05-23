using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public struct Square
    {
        public int value { get; set; }
        public int row { get; set; }
        public int column { get; set; }
        public int block { get; set; }
        
    }
    public class Puzzle
    {
        private List<Square> sudokuBoard = new List<Square>();

        public Puzzle(int[,] board)
        {
            
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudokuBoard.Add(new Square { value = board[i, j], row = i, column = j, block = SudokuInfo.Instance.GetBlock(i, j) });
                }
            }
        }

        public List<Square> GetBoard()
        {
            return sudokuBoard;
        }

    }
}
