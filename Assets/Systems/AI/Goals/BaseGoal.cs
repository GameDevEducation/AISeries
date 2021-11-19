using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGoal : MonoBehaviour
{
    public const int MaxPriority = 100;
    
    public bool CanRun { get; protected set; } = false;
    public int Priority { get; protected set; } = 0;
    public bool IsActive { get; protected set; } = false;

    protected BaseAction LinkedAction;
    protected AIState LinkedAIState;
    protected GOAPBrain LinkedBrain;

    void Awake()
    {
        LinkedAIState = GetComponent<AIState>();
        LinkedBrain = GetComponent<GOAPBrain>();
    }

    public virtual void Wakeup() 
    {
        IsActive = true;
    }

    public virtual void GoToSleep() 
    {
        LinkedAction.Halt();

        IsActive = false;
    }

    public void SetAction(BaseAction newAction)
    {
        LinkedAction = newAction;

        LinkedAction.Begin();
    }

    public abstract void PreTick();

    public void Tick()
    {
        LinkedAction.Tick();
    }

    public virtual string GetDebugInfo()
    {
        return $"{GetType().Name}: Priority={Priority} CanRun={CanRun}";
    }
}
