using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Helper;

namespace WebApplication1.Controllers
{
    public class GameSession : Dictionary<Guid, Game>
    {
        private static GameSession _gameSession;

        public static GameSession Instance
        {
            get
            {
                if (_gameSession == null)
                {
                    _gameSession = new GameSession();
                }

                return _gameSession;
            }
        }
    }

    public class Game
    {
        public static GameSession Sessions => GameSession.Instance;

        public Guid Id { get; set; }
        public Random Rand { get; }
        public Rule Rule1 { get; }
        public AI MyAi { get; }
        public int RoundCount { get; set; }
        public bool IsNewGame => RoundCount == 1;

        public int GetSide(int side)
        {
            return (side == 1) ? -1 : 1;
        }

        private Game(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Rand = new Random();
            Rule1 = new Rule { myTable = new int[15, 15] };
            MyAi = new AI { MyTable = Rule1.myTable };


        }

        public static Game Start(string gameId)
        {
            var id = string.IsNullOrEmpty(gameId)
                ? Guid.NewGuid()
                : Guid.Parse(gameId);

            var game = new Game(id);

            if (Sessions.ContainsKey(id))
            {
                Sessions[id] = game;
            }
            else
            {
                Sessions.Add(game.Id, game);
            }

            return game;
        }

        public static Game Restore(string gameId)
        {
            var id = Guid.Parse(gameId);

            return Sessions.ContainsKey(id)
                    ? Sessions[id]
                    : Start(gameId);
        }

        public async Task<Tuple<int, int>> AiMove(int aiSide)
        {
            MyAi.LoadWeightTable();
            MyAi.MyTable = Rule1.myTable;

            var nextMove = await MyAi.NextMoveAsync(aiSide);

            Rule1.myTable[nextMove.Item1, nextMove.Item2] = aiSide;

            RoundCount++;

            return nextMove;
        }

        public bool ComputerWins(Tuple<int, int> nextMove, int aiSide)
        {
            var computerWins = Rule1.Referee(nextMove.Item1, nextMove.Item2, aiSide);
            return computerWins;
        }

        public bool PlayerWins(int posY, int posX, int palyerSide)
        {
            var playerWins = Rule1.Referee(posY, posX, palyerSide);
            return playerWins;
        }

        public void Move(int posY, int posX, int palyerSide)
        {
            Rule1.myTable[posY, posX] = palyerSide;
            RoundCount++;
        }
    }
}