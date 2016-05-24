using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{   
    [Serializable()]
    public class Square : ISerializable
    {
        public int Value { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Block { get; set; }
        public List<int> PosibleCandidate { get; set; }
        public Square()
        {            
        }
        public Square(SerializationInfo info, StreamingContext context)
        {
            Value = (int)info.GetValue("Value", typeof(int));
            Row = (int)info.GetValue("Row", typeof(int));
            Column = (int)info.GetValue("Column", typeof(int));
            Block = (int)info.GetValue("Block", typeof(int));
            PosibleCandidate = (List<int>)info.GetValue("PosibleCandidate", typeof(List<int>));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value", Value);
            info.AddValue("Row", Row);
            info.AddValue("Column", Column);
            info.AddValue("Block", Block);
            info.AddValue("PosibleCandidate", PosibleCandidate);
        }
    }
}
