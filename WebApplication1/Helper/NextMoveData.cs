namespace WebApplication1.Helper
{
    public class NextMoveData
    {
        public NextMoveData(AI.Move[] myMove, int side, int max, int min)
        {
            MyMove = myMove;
            Side = side;
            Max = max;
            Min = min;
        }

        public AI.Move[] MyMove { get; private set; }
        public int Side { get; private set; }
        public int Max { get; set; }
        public int Min { get; set; }
    }
}