using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Idle : BaseGoal
{
    public override void PreTick()
    {
        CanRun = true;
        Priority = 0;        
    }
}
