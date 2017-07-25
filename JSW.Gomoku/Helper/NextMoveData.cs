using JSW.Gomoku.Domain;

namespace JSW.Gomoku.Helper
{
    public class NextMoveData
    {
        public NextMoveData(Move[] myMove, int side, int max, int min)
        {
            MyMove = myMove;
            Side = side;
            Max = max;
            Min = min;
        }

        public Move[] MyMove { get; private set; }
        public int Side { get; private set; }
        public int Max { get; set; }
        public int Min { get; set; }
    }
}