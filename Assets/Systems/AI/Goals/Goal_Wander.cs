using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Wander : BaseGoal
{
    [SerializeField] float WanderTime = 30f;
    [SerializeField] float WanderCooldownTime = 5f;
    [SerializeField] int BasePriority = 25;

    float ActionTime;

    public override void PreTick()
    {
        // are we currently wandering?
        if (IsActive)
        {
            ActionTime += Time.deltaTime;

            CanRun = ActionTime < WanderTime;
        }
        else
        {
            if (ActionTime < WanderCooldownTime)
                ActionTime += Time.deltaTime;

            CanRun = ActionTime > WanderCooldownTime;
        }

        Priority = BasePriority;
    }

    public override void Wakeup() 
    { 
        base.Wakeup();

        ActionTime = 0f;
    }

    public override void GoToSleep() 
    { 
        base.GoToSleep();

        ActionTime = 0f;
    }
}
