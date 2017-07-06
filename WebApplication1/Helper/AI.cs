using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebApplication1.Helper
{
    public class AI
    {

        private class MinScoreFirstComparer : IComparer<Move>
        {
            public int Compare(Move x, Move y)
            {
                return x.score - y.score;
            }
        }
        private class MaxScoreFirstComparer : IComparer<Move>
        {
            public int Compare(Move x, Move y)
            {
                return y.score - x.score;
            }
        }
        internal struct Move
        {
            internal int x;
            internal int y;
            internal int score;
        };

        public int[,] myTable;

        private Dictionary<string, string[]> weightTable;

        private const int depthLimit = 4;

        public void LoadWeightTable()
        {
            var patterns = File.ReadAllLines(HttpContext.Current.Server.MapPath("~/Helper/JSW.dll"));
            weightTable = patterns.Select(p => p.Split(' ')).ToDictionary(a => a[0]);
        }


        private int Evaluate(int y, int x, int c, int[,] testBoard)
        {
            int[,] Direct = new int[4, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 } }; //YX
            int sum = 0;
            char[] pattern = new char[9];
            for (int I = 0; I < 4; I++)
            {
                for (int K = -4; K < 5; K++)
                {
                    int j = K + 4;
                    if ((y + K * Direct[I, 0] >= 0) && (y + K * Direct[I, 0] < 15) && (x + K * Direct[I, 1] >= 0) && (x + K * Direct[I, 1] < 15))
                    {
                        switch (testBoard[(y + K * Direct[I, 0]), (x + K * Direct[I, 1])])
                        {
                            case 0:
                                pattern[j] = '_';
                                break;
                            case 1:
                                pattern[j] = 'w';
                                break;
                            case -1:
                                pattern[j] = 'b';
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        pattern[j] = ((c == 1) ? 'b' : 'w');
                    }

                }
                sum += int.Parse(weightTable[new string(pattern)][1]);
            }
            return sum;
        }

        private int MinMax(int[,] testBoard, int side, int depth, int alpha, int beta)
        {
            if (depth < depthLimit)
            {
                Move[] MyMove = new Move[256];
                int index = 0;
                for (int I = 0; I < 15; I++)
                {
                    for (int J = 0; J < 15; J++)
                    {
                        if (testBoard[J, I] == 0)
                        {
                            int selfScore = 0;
                            int enemyScore = 0;
                            testBoard[J, I] = side;
                            selfScore = Evaluate(J, I, side, testBoard);  //yx
                            testBoard[J, I] = (side == 1) ? -1 : 1;
                            enemyScore = Evaluate(J, I, (side == 1) ? -1 : 1, testBoard);
                            testBoard[J, I] = 0;
                            MyMove[index].x = I;
                            MyMove[index].y = J;
                            MyMove[index].score = Math.Abs(selfScore) + Math.Abs(enemyScore);
                            /**/
                            if (Math.Abs(selfScore) >= Math.Abs(int.Parse(weightTable["____wwwww"][1])))
                            {
                                return (side == 1) ? 10000000 : -10000000;
                            }
                            /**/
                            index++;
                        }
                    }
                }
                Array.Sort(MyMove, new MaxScoreFirstComparer());
                int max = int.MinValue;
                int min = int.MaxValue;
                for (int I = 0; I < index && I < 10; I++)
                {
                    int selfScore = 0;
                    testBoard[(MyMove[I].y), (MyMove[I].x)] = side;
                    selfScore = MinMax(testBoard, (side == 1 ? -1 : 1), depth + 1, alpha, beta);
                    testBoard[(MyMove[I].y), (MyMove[I].x)] = 0;
                    if (side == 1)
                    {
                        if (selfScore > max)
                        {
                            max = selfScore;
                            if (alpha < max)
                            {
                                alpha = max;
                            }
                        }
                    }
                    else
                    {
                        if (selfScore < min)
                        {
                            min = selfScore;
                            if (beta > min)
                            {
                                beta = min;
                            }
                        }
                    }
                    if (alpha >= beta)
                    {
                        return (side == 1 ? max : min);
                    }
                }
                return (side == 1 ? max : min);
            }
            else
            {
                int sumOfScores = 0;
                for (int I = 0; I < 15; I++)
                {
                    for (int J = 0; J < 15; J++)
                    {
                        if (testBoard[J, I] == 0)
                        {
                            int selfScore = 0;
                            int enemyScore = 0;
                            testBoard[J, I] = (side == 1) ? -1 : 1;
                            enemyScore = Evaluate(J, I, (side == 1) ? -1 : 1, testBoard);
                            sumOfScores += enemyScore;
                            testBoard[J, I] = side;
                            selfScore = Evaluate(J, I, side, testBoard);
                            sumOfScores += selfScore;
                            testBoard[J, I] = 0;
                            /**/
                            if (Math.Abs(selfScore) >= Math.Abs(int.Parse(weightTable["____wwwww"][1])))
                            {
                                return (side == 1) ? 10000000 : -10000000;
                            }
                            /**/
                        }
                    }
                }
                return sumOfScores;
            }
        }

        private delegate int MinMaxFunctionDelegate(int[,] testBoard, int side, int depth, int alpha, int beta);

        public Tuple<int, int> NextMove(int side)
        {
            Move[] myMove = new Move[256];
            int index = 0;
            for (int I = 0; I < 15; I++)
            {
                for (int J = 0; J < 15; J++)
                {
                    if (myTable[J, I] == 0)
                    {
                        int selfScore = 0;
                        int enemyScore = 0;
                        myTable[J, I] = side;
                        selfScore = Evaluate(J, I, side, myTable);  //yx
                        myTable[J, I] = (side == 1) ? -1 : 1;
                        enemyScore = Evaluate(J, I, (side == 1) ? -1 : 1, myTable);
                        myTable[J, I] = 0;
                        myMove[index].x = I;
                        myMove[index].y = J;
                        myMove[index].score = Math.Abs(selfScore) + Math.Abs(enemyScore);
                        if (Math.Abs(selfScore) >= int.Parse(weightTable["____wwwww"][1]))
                        {
                            return new Tuple<int, int>(myMove[index].y, myMove[index].x);
                        }
                        index++;
                    }
                }
            }
            Array.Sort(myMove, (side == 1) ? (IComparer<Move>)(new MaxScoreFirstComparer()) : (IComparer<Move>)(new MinScoreFirstComparer()));
            int max = int.MinValue;
            int min = int.MaxValue;
            MinMaxFunctionDelegate minMaxFunctionDelegate = new MinMaxFunctionDelegate(MinMax);
            List<KeyValuePair<int,IAsyncResult>> asyncResultList = new List<KeyValuePair<int,IAsyncResult>>();
            for (int I = 0; I < index && I < 10; I++)
            {
                int[,] NextBoard = (int[,])myTable.Clone();
                NextBoard[(myMove[I].y), (myMove[I].x)] = side;
                asyncResultList.Add(new KeyValuePair<int, IAsyncResult>(I, minMaxFunctionDelegate.BeginInvoke(NextBoard, (side == 1 ? -1 : 1), 0, max, min, null, null)));
                if ((I+1) % (Environment.ProcessorCount) == 0)
                {
                    WaitAllThreads(asyncResultList, minMaxFunctionDelegate, ref myMove, side, ref max, ref min);
                }
            }
            if (asyncResultList.Any())
            {
                WaitAllThreads(asyncResultList, minMaxFunctionDelegate, ref myMove, side, ref max, ref min);
            }
            Array.Sort(myMove, 0, 10, (side == 1) ? (IComparer<Move>)(new MaxScoreFirstComparer()) : (IComparer<Move>)(new MinScoreFirstComparer()));
            return new Tuple<int, int>(myMove[0].y, myMove[0].x);
        }

        private void WaitAllThreads(List<KeyValuePair<int, IAsyncResult>> asyncResultList, MinMaxFunctionDelegate minMaxFunctionDelegate, ref Move[] myMove, int side, ref int max, ref int min)
        {
            WaitHandle.WaitAll(asyncResultList.Select(r => r.Value.AsyncWaitHandle).ToArray());
            foreach (var result in asyncResultList)
            {
                int selfScore = minMaxFunctionDelegate.EndInvoke(result.Value);
                myMove[result.Key].score = selfScore;
                if (side == 1)
                {
                    if (selfScore > max)
                    {
                        max = selfScore;
                    }
                }
                else
                {
                    if (selfScore < min)
                    {
                        min = selfScore;
                    }
                }
            }
            asyncResultList.Clear();
        }
    }
}