using System;
using System.Collections.Generic;
using System.Linq;

namespace JSW.Gomoku.Domain
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

        public void ClearExpiredSessions()
        {
            var expiredSessions = _gameSession
                .Where(s => DateTime.UtcNow.Subtract(s.Value.LastActivity).TotalMinutes >= 10)
                .Select(s => s.Key)
                .ToList();

            expiredSessions.ForEach(s => _gameSession.Remove(s));
        }
    }
}