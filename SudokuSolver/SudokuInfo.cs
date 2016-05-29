using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    struct Blocks
    {
        public List<int> rows { get; set; }
        public List<int> columns { get; set; }
    }
    public class SudokuInfo
    {
        private static SudokuInfo instance;

        List<int> validCandidate = null;

        List<Blocks> block = new List<Blocks>();
       
        private SudokuInfo()
        {
            validCandidate = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            //block 0
            block.Add(new Blocks { rows = new List<int> { 0, 1, 2 }, columns = new List<int> { 0, 1, 2 } });
            //block 1
            block.Add(new Blocks { rows = new List<int> { 0, 1, 2 }, columns = new List<int> { 3, 4, 5 } });
            //block 3
            block.Add(new Blocks { rows = new List<int> { 0, 1, 2 }, columns = new List<int> { 6, 7, 8 } });
            //block 4
            block.Add(new Blocks { rows = new List<int> { 3, 4, 5 }, columns = new List<int> { 0, 1, 2 } });
            //block 5
            block.Add(new Blocks { rows = new List<int> { 3, 4, 5 }, columns = new List<int> { 3, 4, 5 } });
            //block 6
            block.Add(new Blocks { rows = new List<int> { 3, 4, 5 }, columns = new List<int> { 6, 7, 8 } });
            //block 7
            block.Add(new Blocks { rows = new List<int> { 6, 7, 8 }, columns = new List<int> { 0, 1, 2 } });
            //block 8
            block.Add(new Blocks { rows = new List<int> { 6, 7, 8 }, columns = new List<int> { 3, 4, 5 } });
            //block 9
            block.Add(new Blocks { rows = new List<int> { 6, 7, 8 }, columns = new List<int> { 6, 7, 8 } });

            
        }

        public static SudokuInfo Instance
        {
            get
            {
                if (instance == null) 
                {
                    instance = new SudokuInfo();
                }
                return instance;
            }
        }

        public List<int> ListValidCandidate()
        {
            return validCandidate;
        }

        public int GetBlock(int row, int column)
        {
            var index = block.FindIndex(b => b.rows.Contains(row) && b.columns.Contains(column));

            return index;
        }
        
    }

    //This class used for return value for update UI
    public class HeuristicResult
    {
        public bool Resolved { get; set; }
        public Puzzle CurrentState { get; set; }        
        public List<Square> ListSquareRelevant { get; set; }
    }
}
