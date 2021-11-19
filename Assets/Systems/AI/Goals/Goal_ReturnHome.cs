using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_ReturnHome : BaseGoal
{
    [SerializeField] [Range(0f, 1f)] float PanicStartThreshold = 0.8f;
    [SerializeField] [Range(0f, 1f)] float PanicEndThreshold = 0.2f;

    public override void PreTick()
    {
        if (IsActive)
            CanRun = LinkedAIState.Fear >= PanicEndThreshold;
        else
            CanRun = LinkedAIState.Fear >= PanicStartThreshold;

        Priority = MaxPriority;
    }

    public override string GetDebugInfo()
    {
        return $"{base.GetDebugInfo()}{System.Environment.NewLine}Fear: {LinkedAIState.Fear}";
    }    
}
