using System;
using System.Threading.Tasks;

namespace JSW.Gomoku.Domain
{
    public class Game
    {
        public static GameSession Sessions => GameSession.Instance;

        public Guid Id { get; set; }
        public Random Rand { get; }
        public Rule Rule1 { get; }
        public AI MyAi { get; }
        public int RoundCount { get; set; }
        public bool IsNewGame => RoundCount == 1;
        public DateTime StartTime { get; set; }
        public DateTime LastActivity { get; set; }

        public int GetSide(int side)
        {
            return (side == 1) ? -1 : 1;
        }

        private Game(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            StartTime = DateTime.UtcNow;
            LastActivity = DateTime.Now;

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

            Sessions.ClearExpiredSessions();

            return game;
        }

        public static Game Restore(string gameId)
        {
            var id = Guid.Parse(gameId);

            var game = Sessions.ContainsKey(id)
                    ? Sessions[id]
                    : Start(gameId);

            game.LastActivity = DateTime.UtcNow;

            Sessions.ClearExpiredSessions();

            return game;
        }

        public Tuple<int, int> Move(int posY, int posX, int palyerSide)
        {
            Rule1.myTable[posY, posX] = palyerSide;
            RoundCount++;
            return Tuple.Create(posY, posX);
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
    }
}