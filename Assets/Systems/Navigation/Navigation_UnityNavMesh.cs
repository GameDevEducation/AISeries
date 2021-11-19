using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Navigation_UnityNavMesh : BaseNavigation
{
    NavMeshAgent LinkedAgent;

    protected override void Initialise()
    {
        LinkedAgent = GetComponent<NavMeshAgent>();
    }

    protected override bool RequestPath()
    {
        LinkedAgent.speed = MaxMoveSpeed;
        LinkedAgent.angularSpeed = RotationSpeed;
        LinkedAgent.stoppingDistance = DestinationReachedThreshold;

        LinkedAgent.SetDestination(Destination);
        
        OnBeganPathFinding();

        return true;
    }

    protected override void Tick_Default()
    {

    }

    protected override void Tick_Pathfinding()
    {
        // no pathfinding in progress?
        if (!LinkedAgent.pathPending)
        {           
            if (LinkedAgent.pathStatus == NavMeshPathStatus.PathComplete)
                OnPathFound();
            else
                OnFailedToFindPath();
        }
    }

    protected override void Tick_PathFollowing()
    {
        // do we have a path and we near the destination?
        if (LinkedAgent.hasPath && LinkedAgent.remainingDistance <= LinkedAgent.stoppingDistance)
        {
            OnReachedDestination();
        }
        else
        {
            if (DEBUG_ShowHeading)
                Debug.DrawLine(transform.position + Vector3.up, LinkedAgent.steeringTarget, Color.green);
        }
    }
}
