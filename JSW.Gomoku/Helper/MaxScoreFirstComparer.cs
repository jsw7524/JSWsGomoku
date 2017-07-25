using System.Collections.Generic;
using JSW.Gomoku.Domain;

namespace JSW.Gomoku.Helper
{
    public class MaxScoreFirstComparer : IComparer<Move>
    {
        public int Compare(Move x, Move y)
        {
            return y.score - x.score;
        }
    }
}