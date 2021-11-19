using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_ReturnHome : BaseAction
{
    protected override void Initialise()
    {

    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        return goal is Goal_ReturnHome;
    }

    public override float Cost()
    {
        return 0f;
    }

    public override void Begin()
    {
        Navigation.SetDestination(LinkedAIState.Home.GetRandomSafePoint());
    }

    public override void Tick()
    {
    }

    public override void Halt()
    {
        Navigation.StopMovement();
    }
}
