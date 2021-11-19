using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Navigation_CustomPathfinding : BaseNavigation
{
    [Header("Pathfinding")]
    [SerializeField] string PathdataUID;
    [SerializeField] bool UseSynchronous = true;
    [SerializeField] float ArrivalDistance = 5f;
    [SerializeField] AnimationCurve ArrivalSpeedScale;

    [Header("Ground Raycasts")]
    [SerializeField] float RaycastOffset = 2f;
    [SerializeField] float RaycastDepth = 10f;
    [SerializeField] LayerMask RaycastMask = ~0;

    [Header("Path Following")]
    [SerializeField] float NodeReachedThreshold = 1f;
    [SerializeField] int NodeLookAheadLimit = 10;

    Rigidbody LinkedRB;

    List<PathdataNode> Path = null;
    int TargetNode = -1;

    protected override void Initialise()
    {
        LinkedRB = GetComponent<Rigidbody>();
    }

    protected override bool RequestPath()
    {
        EPathfindResult result = EPathfindResult.NoPathdata;

        if (UseSynchronous)
        {
            result = PathfindingManager.Instance.RequestPath_Synchronous(PathdataUID, transform.position, Destination, 
                                                                         CalculateCost, out Path);

            if (result == EPathfindResult.PathFound)
            {
                OnPathFound();
                return true;
            }
        }
        else
        {
            result = PathfindingManager.Instance.RequestPath_Asynchronous(PathdataUID, transform.position, Destination,
                                                                          CalculateCost, OnPathfindingComplete);

            if (result == EPathfindResult.PathfindingInProgress)
            {
                OnBeganPathFinding();
                return true;
            }
        }

        OnFailedToFindPath();

        return false;
    }

    float CalculateCost(PathdataNode node1, PathdataNode node2)
    {
        return (node1.WorldPos - node2.WorldPos).magnitude;
    }

    void OnPathfindingComplete(List<PathdataNode> foundPath, EPathfindResult result)
    {
        if (result == EPathfindResult.PathFound)
        {
            Path = foundPath;
            OnPathFound();
        }
        else
            OnFailedToFindPath();
    }

    protected override void OnPathFound()
    {
        base.OnPathFound();

        TargetNode = 0;
    }

    protected override void Tick_Default()
    {

    }

    protected override void Tick_Pathfinding()
    {

    }

    protected override void Tick_PathFollowing()
    {
        Vector3 targetPosition = (TargetNode == (Path.Count - 1)) ? Destination : Path[TargetNode].WorldPos;

        // calculate the 2D vector to the target
        Vector3 vecToTarget = targetPosition - transform.position;
        vecToTarget.y = 0f;

        // select our threshold
        float distanceThreshold = (TargetNode == (Path.Count - 1)) ? DestinationReachedThreshold : NodeReachedThreshold;

        // reached the node?
        if (vecToTarget.magnitude <= distanceThreshold)
        {
            // advance the node
            ++TargetNode;

            // have we reached the destination?
            if (TargetNode >= Path.Count)
            {
                LinkedRB.velocity = Vector3.zero;
                OnReachedDestination();
                return;
            }

            // perform lookahead
            for (int nodeOffset = NodeLookAheadLimit; nodeOffset > 0; --nodeOffset)
            {
                int candidateNode = TargetNode + nodeOffset;

                // node does not exist?
                if (candidateNode >= Path.Count)
                    continue;

                // is there a shortcut?
                if (PathfindingManager.Instance.CanWalkBetween(PathdataUID, Path[TargetNode - 1], Path[candidateNode]))
                {
                    TargetNode = candidateNode;
                    break;
                }
            }
        }

        if (DEBUG_ShowHeading)
            Debug.DrawLine(transform.position + Vector3.up, Path[TargetNode].WorldPos + Vector3.up, Color.magenta);

        float targetSpeed = MaxMoveSpeed;

        // heading towards final point?
        if (TargetNode == (Path.Count - 1))
        {
            float arrivalFactor = vecToTarget.magnitude / ArrivalDistance;
            targetSpeed *= ArrivalSpeedScale.Evaluate(arrivalFactor);
        }

        // rotate the character
        Quaternion targetRotation = Quaternion.LookRotation(vecToTarget, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

        // run our grounded check
        RaycastHit hitInfo;
        Vector3 movementVector = transform.forward;
        if (Physics.Raycast(transform.position + Vector3.up * RaycastOffset, Vector3.down, out hitInfo,
                            RaycastDepth, RaycastMask, QueryTriggerInteraction.Ignore))
        {
            movementVector = Vector3.ProjectOnPlane(movementVector, hitInfo.normal);
        }

        // generate steering command
        LinkedRB.velocity = movementVector * targetSpeed;
    }
}
