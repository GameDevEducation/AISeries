using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_GatherResource : BaseFSMAction<Action_GatherResource.EState>
{
    public enum EState
    {
        PickResource,

        MoveToResource,
        CollectResource,

        ReturnHome,
        StoreResource
    }

    [SerializeField] float CollectionTime = 5f;
    [SerializeField] float StorageTime = 5f;
    float ActionTimeRemaining = 0f;

    WorldResource Target;

    protected override void Initialise()
    {
        AddState(EState.PickResource);
        AddState(EState.MoveToResource);
        AddState(EState.CollectResource);
        AddState(EState.ReturnHome);
        AddState(EState.StoreResource);
    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        return goal is Goal_GatherResource;
    }

    public override float Cost()
    {
        return 0f;
    }

    protected override void OnEnter()
    {
        switch(State)
        {
            case EState.PickResource:
                Target = LinkedAIState.Home.GetGatherTarget(LinkedBrain);
                break;

            case EState.MoveToResource:
                Navigation.SetDestination(Target.transform.position);
                break;

            case EState.CollectResource:
                ActionTimeRemaining = CollectionTime;
                break;

            case EState.ReturnHome:
                Navigation.SetDestination(LinkedAIState.Home.GetRandomSafePoint());
                break;

            case EState.StoreResource:
                ActionTimeRemaining = StorageTime;
                break;            
        }
    }

    protected override void OnTick()
    {
        switch (State)
        {
            case EState.CollectResource:
            case EState.StoreResource:
                ActionTimeRemaining -= Time.deltaTime;
                break;

            case EState.ReturnHome:
                if (!Navigation.IsAtDestination && !Navigation.IsFindingOrFollowingPath)
                {
                    Debug.LogError($"{gameObject.name} failed to return home. Picking new path");
                    Navigation.SetDestination(LinkedAIState.Home.GetRandomSafePoint());
                }
                break;
        }
    }

    protected override void OnExit()
    {
        Navigation.StopMovement();
    }

    protected override EState CheckTransition()
    {
        switch (State)
        {
            case EState.PickResource:
                if (Target != null)
                    return EState.MoveToResource;
                else
                    HasFinished = true;
                break;

            case EState.MoveToResource:
                if (Navigation.IsAtDestination)
                    return EState.CollectResource;
                else if (!Navigation.IsFindingOrFollowingPath)
                    return EState.PickResource;
                break;

            case EState.CollectResource:
                if (ActionTimeRemaining <= 0f)
                {
                    LinkedAIState.SetAmountCarried(Target.AvailableAmount);
                    return EState.ReturnHome;
                }
                break;

            case EState.ReturnHome:
                if (Navigation.IsAtDestination)
                    return EState.StoreResource;
                break;

            case EState.StoreResource:
                if (ActionTimeRemaining <= 0f)
                {
                    LinkedAIState.Home.StoreResource(Target.Type, LinkedAIState.AmountCarried);
                    LinkedAIState.SetAmountCarried(0);
                    HasFinished = true;
                }
                break;
        }

        return State;
    }

    public override string GetDebugInfo()
    {
        return $"State: {State} Resource: {Target}";
    }
}
