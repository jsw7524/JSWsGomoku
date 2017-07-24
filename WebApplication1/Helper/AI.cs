using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public int[,] MyTable;

        private Dictionary<string, string[]> weightTable;

        public int DepthLimit = 6;
        public TimeSpan timeSpanTotal;
        public int roundCount;
        public void LoadWeightTable()
        {
            var patterns = File.ReadAllLines(HttpContext.Current.Server.MapPath("~/Helper/JSW.dll"));
            weightTable = patterns.Select(p => p.Split(' ')).ToDictionary(a => a[0]);
        }


        private int Evaluate(int y, int x, int c, int[,] testBoard)
        {
            int[,] direct = new int[4, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 } }; //YX
            int sum = 0;
            char[] pattern = new char[9];
            for (int I = 0; I < 4; I++)
            {
                for (int K = -4; K < 5; K++)
                {
                    int j = K + 4;
                    if ((y + K * direct[I, 0] >= 0) && (y + K * direct[I, 0] < 15) && (x + K * direct[I, 1] >= 0) && (x + K * direct[I, 1] < 15))
                    {
                        switch (testBoard[(y + K * direct[I, 0]), (x + K * direct[I, 1])])
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


        private int[,] RangeUpdate(int y, int x, int c, int[,] testBoard, int[,] scoreBoard)
        {
            int[,] direct = new int[4, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 } }; //YX
            int[,] newScoreBoard = scoreBoard.Clone() as int[,];
            for (int I = 0; I < 4; I++)
            {
                for (int K = -4; K < 5; K++)
                {
                    if ((y + K * direct[I, 0] >= 0) && (y + K * direct[I, 0] < 15) && (x + K * direct[I, 1] >= 0) && (x + K * direct[I, 1] < 15))
                    {
                        if (testBoard[y + K * direct[I, 0], x + K * direct[I, 1]] == 0)
                        {
                            testBoard[y + K * direct[I, 0], x + K * direct[I, 1]] = c;
                            newScoreBoard[y + K * direct[I, 0], x + K * direct[I, 1]] = Evaluate(y + K * direct[I, 0], x + K * direct[I, 1], c, testBoard);
                            testBoard[y + K * direct[I, 0], x + K * direct[I, 1]] = 0;
                        }
                    }
                }
            }
            return newScoreBoard;
        }

        private int MinMax(int y, int x, int[,] testBoard, int side, int depth, int alpha, int beta, int[,] whiteScores, int[,] blackScores)
        {
            int[,] newWhiteScores = RangeUpdate(y, x, 1, testBoard, whiteScores);
            int[,] newBlackScores = RangeUpdate(y, x, -1, testBoard, blackScores);
            if (depth < DepthLimit)
            {
                Move[] myMove = new Move[256];
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
                            selfScore = (side == 1) ? newWhiteScores[J, I] : newBlackScores[J, I];
                            testBoard[J, I] = (side == 1) ? -1 : 1;
                            enemyScore = (side == 1) ? newBlackScores[J, I] : newWhiteScores[J, I];
                            testBoard[J, I] = 0;
                            myMove[index].x = I;
                            myMove[index].y = J;
                            myMove[index].score = Math.Abs(selfScore) + Math.Abs(enemyScore);
                            /**/
                            if (Math.Abs(selfScore) >= Math.Abs(int.Parse(weightTable["____wwwww"][1])))
                            {
                                return (side == 1) ? 100000000 : -100000000;
                            }
                            /**/
                            index++;
                        }
                    }
                }
                Array.Sort(myMove, new MaxScoreFirstComparer());
                int max = -100000000;
                int min = 100000000;
                for (int I = 0; I < index && I < 10; I++)
                {
                    int selfScore = 0;
                    testBoard[(myMove[I].y), (myMove[I].x)] = side;
                    selfScore = MinMax(myMove[I].y, myMove[I].x, testBoard, (side == 1 ? -1 : 1), depth + 1, alpha, beta, newWhiteScores, newBlackScores);
                    testBoard[(myMove[I].y), (myMove[I].x)] = 0;
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
                            sumOfScores += (newWhiteScores[J, I] + newBlackScores[J, I]);
                            if (((side == 1) ? Math.Abs(newWhiteScores[J, I]) : Math.Abs(newBlackScores[J, I])) >= Math.Abs(int.Parse(weightTable["____wwwww"][1])))
                            {
                                return (side == 1) ? 100000000 : -100000000;
                            }
                        }
                    }
                }
                return sumOfScores;
            }
        }

        private delegate int MinMaxFunctionDelegate(int y, int x, int[,] testBoard, int side, int depth, int alpha,
            int beta, int[,] whiteScores, int[,] blackScores);

        public Tuple<int, int> NextMove(int side)
        {
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            Move[] myMove = new Move[256];
            int index = 0;
            int[,] newWhiteScores = new int[15, 15];
            int[,] newBlackScores = new int[15, 15];
            for (int I = 0; I < 15; I++)
            {
                for (int J = 0; J < 15; J++)
                {
                    if (MyTable[J, I] == 0)
                    {
                        int selfScore = 0;
                        int enemyScore = 0;
                        MyTable[J, I] = side;
                        selfScore = Evaluate(J, I, side, MyTable);  //yx
                        MyTable[J, I] = (side == 1) ? -1 : 1;
                        enemyScore = Evaluate(J, I, (side == 1) ? -1 : 1, MyTable);
                        MyTable[J, I] = 0;
                        myMove[index].x = I;
                        myMove[index].y = J;
                        myMove[index].score = Math.Abs(selfScore) + Math.Abs(enemyScore);
                        newWhiteScores[J, I] = (side == 1) ? selfScore : enemyScore;
                        newBlackScores[J, I] = (side == 1) ? enemyScore : selfScore;
                        if (Math.Abs(selfScore) >= int.Parse(weightTable["____wwwww"][1]))
                        {
                            return new Tuple<int, int>(myMove[index].y, myMove[index].x);
                        }
                        index++;

                    }
                }
            }
            Array.Sort(myMove, (side == 1) ? (IComparer<Move>)(new MaxScoreFirstComparer()) : (IComparer<Move>)(new MinScoreFirstComparer()));
            int max = -100000000;
            int min = 100000000;
            MinMaxFunctionDelegate minMaxFunctionDelegate = new MinMaxFunctionDelegate(MinMax);
            List<KeyValuePair<int, IAsyncResult>> asyncResultList = new List<KeyValuePair<int, IAsyncResult>>();
            for (int I = 0; I < index && I < 8; I++)
            {
                int[,] nextBoard = (int[,])MyTable.Clone();
                nextBoard[(myMove[I].y), (myMove[I].x)] = side;
                asyncResultList.Add(new KeyValuePair<int, IAsyncResult>(I, minMaxFunctionDelegate.BeginInvoke(myMove[I].y, myMove[I].x, nextBoard, (side == 1 ? -1 : 1), 0, max, min, newWhiteScores, newBlackScores, null, null)));
                if ((I + 1) % (Environment.ProcessorCount) == 0)
                {
                    WaitAllThreads(asyncResultList, minMaxFunctionDelegate, ref myMove, side, ref max, ref min);
                }
            }
            if (asyncResultList.Any())
            {
                WaitAllThreads(asyncResultList, minMaxFunctionDelegate, ref myMove, side, ref max, ref min);
            }
            Array.Sort(myMove, 0, 8, (side == 1) ? (IComparer<Move>)(new MaxScoreFirstComparer()) : (IComparer<Move>)(new MinScoreFirstComparer()));
            //stopWatch.Stop();
            //TimeSpan timeSpan = stopWatch.Elapsed;
            //timeSpanTotal += timeSpan;
            //TimeSpan timeSpanAvg = new TimeSpan(timeSpanTotal.Ticks / (roundCount/2+1)); 
            //TimeSpan timeSpan4s = new TimeSpan(0, 0,4);
            //TimeSpan timeSpan20s = new TimeSpan(0, 0, 20);
            //if (timeSpanAvg < timeSpan4s)
            //{
            //   DepthLimit+=2;
            //}
            //if (timeSpanAvg > timeSpan20s)
            //{
            //   DepthLimit-=2;
            //}
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