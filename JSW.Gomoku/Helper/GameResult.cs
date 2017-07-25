namespace JSW.Gomoku.Helper
{
    public partial class GomokuController
    {
        public class GameResult
        {
            public int yAI { get; }
            public int xAI { get; }
            public int sideAI { get; }
            public int flag { get; }
            public string message { get; }

            public GameResult()
            {
                
            }
            public GameResult(int yAi, int xAi, int side, int flag, string message)
            {
                yAI = yAi;
                xAI = xAi;
                sideAI = side == 1 ? -1 : 1;
                this.flag = flag;
                this.message = message;
            }
        }
    }
}