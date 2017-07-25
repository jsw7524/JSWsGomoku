namespace JSW.Gomoku.Helper
{
    public class MinMaxData
    {
        public MinMaxData(int y, int x, int[,] testBoard, int side, int depth, int alpha, int beta, int[,] whiteScores, int[,] blackScores)
        {
            Y = y;
            X = x;
            TestBoard = testBoard;
            Side = side;
            Depth = depth;
            Alpha = alpha;
            Beta = beta;
            WhiteScores = whiteScores;
            BlackScores = blackScores;
        }

        public int Y { get; private set; }
        public int X { get; private set; }
        public int[,] TestBoard { get; private set; }
        public int Side { get; private set; }
        public int Depth { get; private set; }
        public int Alpha { get; private set; }
        public int Beta { get; private set; }
        public int[,] WhiteScores { get; private set; }
        public int[,] BlackScores { get; private set; }
    }
}