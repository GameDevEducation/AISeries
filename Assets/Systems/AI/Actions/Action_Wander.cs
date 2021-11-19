using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Wander : BaseFSMAction<Action_Wander.EState>
{
    public enum EState
    {
        FindingLocation,
        MoveToLocation
    }

    [SerializeField] float MinWanderRange = 25f;
    [SerializeField] float MaxWanderRange = 50f;
    [SerializeField] float SearchRange = 5f;

    bool HasWanderPos = false;
    Vector3 WanderTarget;

    protected override void Initialise()
    {
        AddState(EState.FindingLocation, OnEnter_FindingLocation, OnTick_FindingLocation, null, CheckTransition_FindingLocation);
        AddState(EState.MoveToLocation, OnEnter_MoveToLocation, null, OnExit_MoveToLocation, CheckTransition_MoveToLocation);
    }

    void OnEnter_FindingLocation()
    {
        HasWanderPos = false;
        PickLocation();
    }

    void OnTick_FindingLocation()
    {
        if (!HasWanderPos)
            PickLocation();
    }

    EState CheckTransition_FindingLocation()
    {
        return HasWanderPos ? EState.MoveToLocation : EState.FindingLocation;
    }

    void OnEnter_MoveToLocation()
    {
        if (!Navigation.SetDestination(WanderTarget))
            HasWanderPos = false;
    }

    void OnExit_MoveToLocation()
    {
        Navigation.StopMovement();
    }

    EState CheckTransition_MoveToLocation()
    {
        // no wander pos? not currently finding or following path? at destination? Pick a new one
        if (!HasWanderPos || !Navigation.IsFindingOrFollowingPath || Navigation.IsAtDestination)
            return EState.FindingLocation;

        return EState.MoveToLocation;
    }

    void PickLocation()
    {
        // pick a random direction and distance
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(MinWanderRange, MaxWanderRange);

        // generate a position
        Vector3 wanderPos = transform.position;
        wanderPos.x += distance * Mathf.Sin(angle);
        wanderPos.z += distance * Mathf.Cos(angle);

        // find nearest valid point
        if (Navigation.FindNearestPoint(wanderPos, SearchRange, out WanderTarget))
            HasWanderPos = true;
    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        return goal is Goal_Wander;
    }

    public override float Cost()
    {
        return 0f;
    }

}
