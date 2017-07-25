using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JSW.Gomoku.Helper
{
    public class StateManager : IStateManager
    {
        public StateManager()
        {
            
        }

        public void Set(string key, object state)
        {
            throw new NotImplementedException();
        }

        public object Get(string key)
        {
            throw new NotImplementedException();
        }
    }

    public interface IStateManager
    {
        void Set(string key, object state);
        object Get(string key);
    }
}