using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Idle : BaseAction
{
    protected override void Initialise()
    {

    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        return goal is Goal_Idle;
    }

    public override float Cost()
    {
        return 0f;
    }

    public override void Begin()
    {

    }

    public override void Tick()
    {
    }

    public override void Halt()
    {

    }    
}
