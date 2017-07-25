using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JSW.Gomoku.Helper;

namespace JSW.Gomoku.Domain
{
    public partial class AI
    {
        public int[,] MyTable;

        private Dictionary<string, string[]> _weightTable;

        private const int DepthLimit = 8;

        public void LoadWeightTable()
        {
            var patterns = File.ReadAllLines(HttpContext.Current.Server.MapPath("~/Helper/JSW.dll"));
            _weightTable = patterns.Select(p => p.Split(' ')).ToDictionary(a => a[0]);
        }

        public async Task<Tuple<int, int>> NextMoveAsync(int side)
        {
            var myMove = new Move[256];
            var index = 0;
            var newWhiteScores = new int[15, 15];
            var newBlackScores = new int[15, 15];
            for (var I = 0; I < 15; I++)
            {
                for (var J = 0; J < 15; J++)
                {
                    if (MyTable[J, I] != 0) continue;

                    var selfScore = 0;
                    var enemyScore = 0;
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
                    if (Math.Abs(selfScore) >= int.Parse(_weightTable["____wwwww"][1]))
                    {
                        return new Tuple<int, int>(myMove[index].y, myMove[index].x);
                    }

                    index++;
                }
            }

            Array.Sort(myMove, (side == 1)
                ? (IComparer<Move>)new MaxScoreFirstComparer()
                : new MinScoreFirstComparer());

            var max = int.MinValue;
            var min = int.MaxValue;

            var tasks = new List<Task<int>>();

            for (var I = 0; I < index && I < 8; I++)
            {
                var nextBoard = (int[,])MyTable.Clone();

                nextBoard[myMove[I].y, myMove[I].x] = side;

                var task = MinMaxAsync(new MinMaxData(myMove[I].y, myMove[I].x, nextBoard, side == 1 ? -1 : 1, 0, max, min, newWhiteScores, newBlackScores));

                tasks.Add(task);

                if ((I + 1) % Environment.ProcessorCount != 0) continue;

                var selfScores = await Task.WhenAll(tasks.ToArray());
                WaitAllThreads(selfScores, new NextMoveData(myMove, side, max, min));
                tasks.Clear();
            }

            if (tasks.Any())
            {
                var selfScores = await Task.WhenAll(tasks.ToArray());
                WaitAllThreads(selfScores, new NextMoveData(myMove, side, max, min));
            }

            var comparer = side == 1
                ? (IComparer<Move>)new Helper.MaxScoreFirstComparer()
                : new Helper.MinScoreFirstComparer();

            Array.Sort(myMove, 0, 8, comparer);

            return new Tuple<int, int>(myMove[0].y, myMove[0].x);
        }

        private int Evaluate(int y, int x, int c, int[,] testBoard)
        {
            var direct = new int[4, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 } }; //YX
            var sum = 0;
            var pattern = new char[9];
            for (var I = 0; I < 4; I++)
            {
                for (var K = -4; K < 5; K++)
                {
                    var j = K + 4;
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
                sum += int.Parse(_weightTable[new string(pattern)][1]);
            }
            return sum;
        }

        private int[,] RangeUpdate(int y, int x, int c, int[,] testBoard, int[,] scoreBoard)
        {
            var direct = new int[4, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 } }; //YX
            var newScoreBoard = scoreBoard.Clone() as int[,];
            for (var I = 0; I < 4; I++)
            {
                for (var K = -4; K < 5; K++)
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
            try
            {
                var newWhiteScores = RangeUpdate(y, x, 1, testBoard, whiteScores);
                var newBlackScores = RangeUpdate(y, x, -1, testBoard, blackScores);
                if (depth < DepthLimit)
                {
                    var myMove = new Move[256];
                    var index = 0;


                    for (var I = 0; I < 15; I++)
                    {
                        for (var J = 0; J < 15; J++)
                        {
                            if (testBoard[J, I] == 0)
                            {
                                var selfScore = 0;
                                var enemyScore = 0;
                                testBoard[J, I] = side;
                                selfScore = (side == 1) ? newWhiteScores[J, I] : newBlackScores[J, I];
                                testBoard[J, I] = (side == 1) ? -1 : 1;
                                enemyScore = (side == 1) ? newBlackScores[J, I] : newWhiteScores[J, I];
                                testBoard[J, I] = 0;
                                myMove[index].x = I;
                                myMove[index].y = J;
                                myMove[index].score = Math.Abs(selfScore) + Math.Abs(enemyScore);
                                /**/
                                if (Math.Abs(selfScore) >= Math.Abs(int.Parse(_weightTable["____wwwww"][1])))
                                {
                                    return (side == 1) ? 10000000 : -10000000;
                                }
                                /**/
                                index++;
                            }
                        }
                    }
                    Array.Sort(myMove, new MaxScoreFirstComparer());
                    var max = int.MinValue;
                    var min = int.MaxValue;
                    for (var I = 0; I < index && I < 10; I++)
                    {
                        var selfScore = 0;
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
                    var sumOfScores = 0;
                    for (var I = 0; I < 15; I++)
                    {
                        for (var J = 0; J < 15; J++)
                        {
                            if (testBoard[J, I] == 0)
                            {
                                sumOfScores += (newWhiteScores[J, I] + newBlackScores[J, I]);
                                if (((side == 1) ? Math.Abs(newWhiteScores[J, I]) : newBlackScores[J, I]) >= Math.Abs(int.Parse(_weightTable["____wwwww"][1])))
                                {
                                    return (side == 1) ? 10000000 : -10000000;
                                }

                            }
                        }
                    }
                    return sumOfScores;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private Task<int> MinMaxAsync(MinMaxData minMaxData)
        {
            return Task.Run(() =>
            MinMax(
                minMaxData.Y,
                minMaxData.X,
                minMaxData.TestBoard,
                minMaxData.Side == 1 ? -1 : 1,
                0,
                minMaxData.Depth,
                minMaxData.Beta,
                minMaxData.WhiteScores,
                minMaxData.BlackScores));
        }

        private static void WaitAllThreads(int[] selfScores, NextMoveData nextMoveData)
        {
            var i = 0;
            foreach (var selfScore in selfScores)
            {
                nextMoveData.MyMove[i++].score = selfScore;
                if (nextMoveData.Side == 1)
                {
                    if (selfScore > nextMoveData.Max)
                    {
                        nextMoveData.Max = selfScore;
                    }
                }
                else
                {
                    if (selfScore < nextMoveData.Min)
                    {
                        nextMoveData.Min = selfScore;
                    }
                }
            }
        }
       
    }
}