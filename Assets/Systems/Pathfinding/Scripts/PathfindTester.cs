using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindTester : MonoBehaviour
{
    [SerializeField] string PathdataUID;
    [SerializeField] Transform StartMarker;
    [SerializeField] Transform EndMarker;
    [SerializeField] bool FilterPaths = true;

    System.Diagnostics.Stopwatch asyncPathfindTimer = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        TestSynchronous();

        asyncPathfindTimer.Start();
        PathfindingManager.Instance.RequestPath_Asynchronous(PathdataUID, StartMarker.position, EndMarker.position,
                    delegate (PathdataNode current, PathdataNode destination)
                    {
                        return Vector3.Distance(current.WorldPos, destination.WorldPos);
                    },
                    OnAsyncPathfindComplete);
    }

    void OnAsyncPathfindComplete(List<PathdataNode> path, EPathfindResult result)
    {
        asyncPathfindTimer.Stop();
        Debug.Log("Async Pathfind: " + asyncPathfindTimer.ElapsedMilliseconds);

        if (path != null)
        {
            if (FilterPaths)
                PathfindingManager.Instance.OptimisePath(PathdataUID, path);

            for (int index = 0; index < path.Count - 1; ++index)
            {
                Vector3 node1Pos = path[index].WorldPos;
                Vector3 node2Pos = path[index + 1].WorldPos;

                Debug.DrawLine(node1Pos + Vector3.up, node2Pos + Vector3.up, Color.cyan, 600f);
                Debug.DrawLine(node1Pos, node1Pos + Vector3.up, Color.white, 600f);
            }
        }
    }

    void TestSynchronous()
    {
        System.Diagnostics.Stopwatch syncPathfindTimer = new System.Diagnostics.Stopwatch();

        syncPathfindTimer.Start();
        List<PathdataNode> path;
        var result = PathfindingManager.Instance.RequestPath_Synchronous(PathdataUID, StartMarker.position, EndMarker.position,
                    delegate (PathdataNode current, PathdataNode destination)
                    {
                        return Vector3.Distance(current.WorldPos, destination.WorldPos);
                    },
                    out path);
        syncPathfindTimer.Stop();

        Debug.Log("Sync Pathfind: " + syncPathfindTimer.ElapsedMilliseconds);

        if (path != null)
        {
            if (FilterPaths)
                PathfindingManager.Instance.OptimisePath(PathdataUID, path);

            for (int index = 0; index < path.Count - 1; ++index)
            {
                Vector3 node1Pos = path[index].WorldPos;
                Vector3 node2Pos = path[index + 1].WorldPos;

                Debug.DrawLine(node1Pos + Vector3.up, node2Pos + Vector3.up, Color.magenta, 600f);
                Debug.DrawLine(node1Pos, node1Pos + Vector3.up, Color.white, 600f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
