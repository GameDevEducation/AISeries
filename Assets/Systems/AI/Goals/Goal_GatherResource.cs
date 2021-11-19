using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_GatherResource : BaseGoal
{
    [SerializeField] int MinGatherPriority = 25;
    [SerializeField] int MaxGatherPriority = MaxPriority;

    public override void PreTick()
    {
        // priority cannot change while running
        if (IsActive)
            return;

        float gatherPriority = LinkedAIState.Home.GetGathererPriority();

        if (gatherPriority <= 0f)
        {
            CanRun = false;
            Priority = 0;
        }
        else
        {
            CanRun = true;
            Priority = Mathf.RoundToInt(Mathf.Lerp(MinGatherPriority, MaxGatherPriority, gatherPriority));
        }
    }

    public override void Wakeup()
    {
        base.Wakeup();

        LinkedAIState.Home.AddGatherer(LinkedBrain);
    }

    public override void GoToSleep()
    {
        base.GoToSleep();

        LinkedAIState.Home.RemoveGatherer(LinkedBrain);
    }

}
