using System;
using System.Collections.Generic;

namespace HSM {
    public class StateMachine {
        public readonly State Root;
        public readonly TransitionSequencer Sequencer;
        bool started;
        readonly Dictionary<Type, State> statesByType = new Dictionary<Type, State>();

        public StateMachine(State root) {
            Root = root;
            Sequencer = new TransitionSequencer(this);
        }

        internal void RegisterState(State s) {
            if (s == null) return;
            var t = s.GetType();
            if (!statesByType.ContainsKey(t)) statesByType.Add(t, s);
        }

        public T GetState<T>() where T : State {
            State s;
            if (statesByType.TryGetValue(typeof(T), out s)) return s as T;

            foreach (var kvp in statesByType) {
                if (typeof(T).IsAssignableFrom(kvp.Key)) return (T)kvp.Value;
            }

            return null;
        }

        public void Start() {
            if (started) return;
            
            started = true;
            Root.Enter();
        }

        public void Tick(float deltaTime) {
            if (!started) Start();
            Sequencer.Tick(deltaTime);
        }
        
        internal void InternalTick(float deltaTime) => Root.Update(deltaTime);
        
        // Perform the actual switch from 'from' to 'to' by exiting up to the shared ancestor, then entering down to the target.
        public void ChangeState(State from, State to) {
            if (from == to || from == null || to == null) return;
            
            State lca = TransitionSequencer.Lca(from, to);
            
            // Exit current branch up to (but not including) LCA
            for (State s = from; s != lca; s = s.Parent) s.Exit();
            
            // Enter target branch from LCA down to target
            var stack = new Stack<State>();
            for (State s = to; s != lca; s = s.Parent) stack.Push(s);
            while (stack.Count > 0) stack.Pop().Enter();
        }
    }
}
