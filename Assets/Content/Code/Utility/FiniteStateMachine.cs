using UnityEngine;
using System.Collections.Generic;
using System;

namespace PhantomBrigade.FSM
{

    public delegate void StateCallback ();
    public delegate void TransitionCallback ();

    public class State
    {
        protected StateCallback onEnter;
        protected StateCallback onUpdate;
        protected StateCallback onExit;

        public enum CallbackType
        {
            Update,
            Enter,
            Exit
        }

        public State () { }

        public State 
        (
            StateCallback onEnter = null, 
            StateCallback onUpdate = null, 
            StateCallback onExit = null
        )
        {
            if (onEnter != null)
                this.onEnter = onEnter;
            if (onUpdate != null)
                this.onUpdate = onUpdate;
            if (onExit != null)
                this.onExit = onExit;
        }

        public void AddCallback (CallbackType type, StateCallback callback)
        {
            switch (type)
            {
                case CallbackType.Update:
                    onUpdate += callback;
                    break;
                case CallbackType.Enter:
                    onEnter += callback;
                    break;
                case CallbackType.Exit:
                    onExit += callback;
                    break;
            }
        }

        public void RemoveCallback (CallbackType type, StateCallback callback)
        {
            switch (type)
            {
                case CallbackType.Update:
                    onUpdate -= callback;
                    break;
                case CallbackType.Enter:
                    onEnter -= callback;
                    break;
                case CallbackType.Exit:
                    onExit -= callback;
                    break;
            }
        }

        public void ClearCallback (CallbackType type)
        {
            switch (type)
            {
                case CallbackType.Update:
                    onUpdate = null;
                    break;
                case CallbackType.Enter:
                    onEnter = null;
                    break;
                case CallbackType.Exit:
                    onExit = null;
                    break;
            }
        }

        public void OnEnter ()
        {
            onEnter?.Invoke ();
        }

        public void Evaluate ()
        {
            onUpdate?.Invoke ();
        }

        public void OnExit ()
        {
            onExit?.Invoke ();
        }
    }

    public class FiniteStateMachine<T> where T : struct, IConvertible
    {
        protected T stateCurrent;
        protected T statePrevious;

        // If on, the FSM will not change states when an explicitly defined transition for a given pair of states doesn't exist
        public bool strictTransitions = false;
        
        // If on, the FSM would not change states no matter what
        protected bool locked = false;
        public bool debug = false;

        protected Dictionary<StateTransition<T>, TransitionCallback> transitions;
        protected Dictionary<T, State> states;

        public void AddTransition (T stateOrigin, T stateTarget, TransitionCallback callback)
        {
            StateTransition<T> transition = new StateTransition<T> (stateOrigin, stateTarget);

            if (transitions.ContainsKey (transition))
            {
                Debug.LogWarning ($"FSM | AddTransition | Transition {stateOrigin}->{stateTarget} already exists, you may add multiple methods to the callback if you need multicast support");
                return;
            }

            transitions.Add (transition, callback);
        }

        public FiniteStateMachine ()
        {
            transitions = new Dictionary<StateTransition<T>, TransitionCallback> ();
            states = new Dictionary<T, State> ();
        }

        public void Initialize (T targetState)
        {
            UnlockFSM ();
            stateCurrent = targetState;
            if (states.ContainsKey (targetState))
                states[targetState].OnEnter ();
        }

        public void Deactivate ()
        {
            LockFSM ();
            if (states.ContainsKey (stateCurrent))
                states[stateCurrent].OnExit ();
        }

        public void AddState (T stateType, State state)
        {
            if (states.ContainsKey (stateType))
                Debug.LogWarning ($"FSM | AddState | State already exists for " + stateType);
            else
                states.Add (stateType, state);
        }
        
        public void AddState (T stateType, StateCallback onEnter, StateCallback onEvaluate, StateCallback onExit)
        {
            if (states.ContainsKey (stateType))
                Debug.LogWarning ($"FSM | AddState | State already exists for " + stateType);
            else
                states.Add (stateType, new State (onEnter, onEvaluate, onExit));
        }

        public void EvaluateState ()
        {
            states[stateCurrent].Evaluate ();
        }

        public void ChangeState (T targetState)
        {
            if (locked)
            {
                Debug.LogWarning ("FSM | ChangeState | FSM is locked, transition from " + stateCurrent + " to " + targetState + " is not possible");
                return;
            }
            
            // if (targetState.Equals (stateCurrent))
            //     return;

            StateTransition<T> transition = new StateTransition<T> (stateCurrent, targetState);
            TransitionCallback transitionCallback = null;
            bool transitionExists = transitions.TryGetValue (transition, out transitionCallback);

            if (transitionExists && transitionCallback != null)
                transitionCallback.Invoke ();
            else if (strictTransitions)
            {
                Debug.LogWarning ("FSM | ChangeState | FSM set to strict transition mode, and no transition from " + stateCurrent + " to " + targetState + " was found, aborting...");
                return;
            }

            if (debug)
                Debug.Log ("FSM | ChangeState | Advancing from " + stateCurrent.ToString () + " to " + targetState.ToString ());

            statePrevious = stateCurrent;
            stateCurrent = targetState;

            if (states.ContainsKey (statePrevious))
            {
                states[statePrevious].OnExit ();
            }

            if (states.ContainsKey (targetState))
                states[targetState].OnEnter ();
        }

        public void LockFSM ()
        {
            locked = true;
        }

        public void UnlockFSM ()
        {
            locked = false;
        }

        public T GetCurrentState ()
        {
            return stateCurrent;
        }

        public T GetPreviousState ()
        {
            return statePrevious;
        }
    }

    public class StateTransition<T> : IEquatable<StateTransition<T>> where T : struct, IConvertible
    {
        protected T currentState;
        protected T targetState;

        public StateTransition (T current, T target)
        {
            currentState = current;
            targetState = target;
        }

        public bool Equals (StateTransition<T> other)
        {
            if (ReferenceEquals (this, other))
                return true;

            return currentState.Equals (other.GetCurrentState ()) && targetState.Equals (other.GetTargetState ());
        }

        public override int GetHashCode ()
        {
            //Trying to avoid possible collisions by comparing in a way that A,B and B,A are unique hashes
            return Utilities.ShiftAndWrap (currentState.GetHashCode (), 2) ^ targetState.GetHashCode ();
        }

        public T GetCurrentState ()
        {
            return currentState;
        }

        public T GetTargetState ()
        {
            return targetState;
        }
    }

}
