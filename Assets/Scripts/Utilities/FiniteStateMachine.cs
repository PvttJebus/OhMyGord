using System;
using System.Collections.Generic;

namespace Utilities
{
    /// <summary>
    /// A lightweight, generic finite state machine.
    /// </summary>
    /// <typeparam name="TState">Enum or string representing states.</typeparam>
    public class FiniteStateMachine<TState>
    {
        private class State
        {
            public Action OnEnter;
            public Action OnExit;
            public Action OnUpdate;
        }

        private readonly Dictionary<TState, State> states = new();
        private TState currentStateKey;
        private State currentState;

        public TState CurrentState => currentStateKey;

        /// <summary>
        /// Add a state with optional enter, exit, and update callbacks.
        /// </summary>
        public void AddState(TState stateKey, Action onEnter = null, Action onExit = null, Action onUpdate = null)
        {
            states[stateKey] = new State
            {
                OnEnter = onEnter,
                OnExit = onExit,
                OnUpdate = onUpdate
            };
        }

        /// <summary>
        /// Change to a new state, calling exit and enter callbacks.
        /// </summary>
        public void ChangeState(TState newState)
        {
            if (EqualityComparer<TState>.Default.Equals(newState, currentStateKey))
                return;

            currentState?.OnExit?.Invoke();

            currentStateKey = newState;
            states.TryGetValue(newState, out currentState);

            currentState?.OnEnter?.Invoke();
        }

        /// <summary>
        /// Call the current state's update callback.
        /// </summary>
        public void Update()
        {
            currentState?.OnUpdate?.Invoke();
        }
    }
}