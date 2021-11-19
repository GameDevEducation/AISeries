using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    protected BaseNavigation Navigation;
    protected AIState LinkedAIState;
    protected GOAPBrain LinkedBrain;
    public bool HasFinished { get; protected set; } = false;

    void Awake()
    {
        Navigation = GetComponent<BaseNavigation>();
        LinkedAIState = GetComponent<AIState>();
        LinkedBrain = GetComponent<GOAPBrain>();
    }

    void Start()
    {
        Initialise();
    }
    
    protected abstract void Initialise();

    public abstract bool CanSatisfy(BaseGoal goal);
    public abstract float Cost();

    public abstract void Begin();
    public abstract void Tick();
    public abstract void Halt();

    public virtual string GetDebugInfo()
    {
        return string.Empty;
    }
}
