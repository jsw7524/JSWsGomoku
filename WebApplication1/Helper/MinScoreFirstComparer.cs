using System.Collections.Generic;

namespace WebApplication1.Helper
{
    public partial class AI
    {
        public class MinScoreFirstComparer : IComparer<Move>
        {
            public int Compare(Move x, Move y)
            {
                return x.score - y.score;
            }
        }
    }
}