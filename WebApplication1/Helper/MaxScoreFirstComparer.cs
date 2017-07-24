using System.Collections.Generic;

namespace WebApplication1.Helper
{
    public partial class AI
    {
        public class MaxScoreFirstComparer : IComparer<Move>
        {
            public int Compare(Move x, Move y)
            {
                return y.score - x.score;
            }
        }
    }
}