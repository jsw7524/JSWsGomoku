using System.Collections.Generic;
using JSW.Gomoku.Domain;

namespace JSW.Gomoku.Helper
{
    public class MinScoreFirstComparer : IComparer<Move>
    {
        public int Compare(Move x, Move y)
        {
            return x.score - y.score;
        }
    }
}