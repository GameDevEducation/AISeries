using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class BaseFSMAction<T> : BaseAction
{
    class StateConfig
    {
        public Action OnEnter;
        public Action OnTick;
        public Action OnExit;

        public Func<T> CheckTransition;
    }

    [SerializeField] protected T InitialState;

    Dictionary<T, StateConfig> StateMachine = new Dictionary<T, StateConfig>();    
    protected T State { get; private set; }

    protected void AddState(T state, Action onEnterFn = null, 
                                     Action onTickFn = null,
                                     Action onExitFn = null,
                                     Func<T> checkTransitionFn = null)
    {
        StateMachine[state] = new StateConfig() { OnEnter = onEnterFn != null ? onEnterFn : OnEnter, 
                                                  OnTick = onTickFn != null ? onTickFn : OnTick,
                                                  OnExit = onExitFn != null ? onExitFn : OnExit, 
                                                  CheckTransition = checkTransitionFn != null ? checkTransitionFn : CheckTransition };
    }

    public sealed override void Tick()
    {
        // tick the current state
        StateMachine[State].OnTick();

        // look for a transition
        T nextState = StateMachine[State].CheckTransition();
        if (EqualityComparer<T>.Default.Equals(State, nextState))
            return;

        // perform the transition
        StateMachine[State].OnExit();
        State = nextState;
        StateMachine[State].OnEnter();
    }

    protected virtual void OnEnter()
    {

    }

    protected virtual void OnTick()
    {

    }

    protected virtual void OnExit()
    {

    }

    protected virtual T CheckTransition()
    {
        return State;
    }

    public sealed override void Begin()
    {
        State = InitialState;
        HasFinished = false;
        StateMachine[State].OnEnter();        
    }

    public sealed override void Halt()
    {
        StateMachine[State].OnExit();
    }    
}
