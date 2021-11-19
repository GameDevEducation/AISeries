using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public enum EResolution
{
    NodeSize_1x1 = 1,
    NodeSize_2x2 = 2,
    NodeSize_4x4 = 4,
    NodeSize_8x8 = 8,
    NodeSize_16x16 = 16
}

[System.Serializable]
public class PathdataSet
{
    public string UniqueID;
    public EResolution Resolution = EResolution.NodeSize_1x1;
    public float SlopeLimit = 45f;
    public bool CanUseWater = false;

    public Pathdata Data;

    public bool Debug_ShowAreas = false;
    public bool Debug_ShowNodes = false;
    public bool Debug_ShowEdges = false;
}

public class PathdataManager : MonoBehaviour
{
    [SerializeField] List<PathdataSet> PathdataSets;
    [SerializeField] Terrain Source_Terrain;
    [SerializeField] Texture2D Source_BiomeMap;
    [SerializeField] Texture2D Source_SlopeMap;

    [SerializeField] float WaterHeight = 15f;

    public static PathdataManager Instance { get; private set; } = null;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found duplicate PathdataManager on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Pathdata GetPathdata(string uniqueID)
    {
        foreach(var pathdataSet in PathdataSets)
        {
            if (pathdataSet.UniqueID == uniqueID)
                return pathdataSet.Data;
        }

        return null;
    }

    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Vector3 cameraLocation = SceneView.currentDrawingSceneView.camera.transform.position;

        for (int setIndex = 0; setIndex < PathdataSets.Count; ++setIndex)
        {
            var pathdataSet = PathdataSets[setIndex];

            // pathdata not yet built
            if (pathdataSet.Data == null)
                continue;

            for (int nodeIndex = 0; nodeIndex < pathdataSet.Data.Nodes.Length; ++nodeIndex)
            {
                var node = pathdataSet.Data.Nodes[nodeIndex];

                if (node == null)
                    continue;

                if ((node.WorldPos - cameraLocation).sqrMagnitude > (50 * 50))
                    continue;

                DrawDebug(pathdataSet, node);
            }
        }
    }

    static Color[] AreaBank = new Color[] { Color.red, Color.black, Color.blue, Color.green, Color.cyan, Color.yellow, Color.magenta, Color.white };

    void DrawDebug(PathdataSet pathdataSet, PathdataNode node)
    {
        if (node.IsBoundary)
            return;

        // draw areas?
        if (pathdataSet.Debug_ShowAreas)
        {
            if (node.AreaID < 1)
                return;

            Gizmos.color = AreaBank[node.AreaID % AreaBank.Length];
            Gizmos.DrawLine(node.WorldPos, node.WorldPos + Vector3.up);

            return;
        }

        // draw the nodes?
        if (pathdataSet.Debug_ShowNodes)
        {
            if (node.IsWalkable)
                Gizmos.color = Color.green;
            else if (node.IsWater)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(node.WorldPos, node.WorldPos + Vector3.up);
        }

        // draw the edges
        if (pathdataSet.Debug_ShowEdges)
        {
            var pathdata = pathdataSet.Data;

            Gizmos.color = Color.white;
            if (node.HasNeighbour_N)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_North).WorldPos);
            if (node.HasNeighbour_NE)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_NorthEast).WorldPos);
            if (node.HasNeighbour_E)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_East).WorldPos);
            if (node.HasNeighbour_SE)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_SouthEast).WorldPos);
            if (node.HasNeighbour_S)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_South).WorldPos);
            if (node.HasNeighbour_SW)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_SouthWest).WorldPos);
            if (node.HasNeighbour_W)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_West).WorldPos);
            if (node.HasNeighbour_NW)
                Gizmos.DrawLine(node.WorldPos + Vector3.up, pathdata.GetNode(node.GridPos + GridHelpers.Step_NorthWest).WorldPos);
        }
    }

    public void OnEditorBuildPathdata()
    {
        Internal_BuildPathdata();
    }
    #endif // UNITY_EDITOR

    void Internal_BuildPathdata()
    {
        for (int index = 0; index < PathdataSets.Count; ++index)
            Internal_BuildPathdata(PathdataSets[index], index);
    }

    void Internal_BuildPathdata(PathdataSet pathdataSet, int pathdataIndex)
    {
        // allocate the pathdata
        var pathdata = ScriptableObject.CreateInstance<Pathdata>();

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(pathdata, "Create pathdata");

        string fileName = SceneManager.GetActiveScene().name + "_Pathdata_" + pathdataIndex;
        string assetPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SceneManager.GetActiveScene().path), fileName + ".asset");
        System.IO.File.Delete(assetPath);

        AssetDatabase.CreateAsset(pathdata, assetPath);
#endif // UNITY_EDITOR        

        pathdataSet.Data = pathdata;

        // determine the pathdata size, allowing for scale
        var pathdataSize = (Source_Terrain.terrainData.heightmapResolution - 1) / (int)pathdataSet.Resolution;

        // determine the scaled node size
        var nodeSize = Source_Terrain.terrainData.heightmapScale;
        nodeSize.x *= (int)pathdataSet.Resolution;
        nodeSize.z *= (int)pathdataSet.Resolution;

        pathdata.Initialise(pathdataSet.UniqueID, pathdataSet.Resolution, new Vector2Int(pathdataSize, pathdataSize), nodeSize);

        Internal_BuildPathdata(pathdata, pathdataSet, pathdataSize, nodeSize);

#if UNITY_EDITOR
        // flag the asset as dirty and save it
        EditorUtility.SetDirty(pathdata);
        AssetDatabase.SaveAssets();
#endif // UNITY_EDITOR          
    }

    float SampleHeightMap(float[,] heightMap, EResolution resolution, int row, int column)
    {
        float heightSum = 0f;
        int numSamples = 0;

        int range = (int)resolution + 1;
        
        // retrieve the key heightmap parameters
        int heightMapSize = heightMap.GetLength(0);
        int startingRow = row * (int)resolution;
        int startingCol = column * (int)resolution;

        // sum the height values
        for (int workingRow = startingRow; workingRow < (startingRow + range) && workingRow < heightMapSize; ++workingRow)
        {
            for (int workingCol = startingCol; workingCol < (startingCol + range) && workingCol < heightMapSize; ++workingCol)
            {
                heightSum += heightMap[workingCol, workingRow];
                ++numSamples;
            }
        }

        return numSamples == 0 ? 0f : (heightSum / numSamples);
    }

    float SampleSlopeMap(EResolution resolution, int row, int column)
    {
        float minSlope = 1f;

        int range = (int)resolution + 1;

        // retrieve the key parameters
        int startingRow = row * (int)resolution;
        int startingCol = column * (int)resolution;

        // sum the slope values
        for (int workingRow = startingRow; workingRow < (startingRow + range) && workingRow < Source_SlopeMap.height; ++workingRow)
        {
            for (int workingCol = startingCol; workingCol < (startingCol + range) && workingCol < Source_SlopeMap.width; ++workingCol)
            {
                minSlope = Mathf.Min(Source_SlopeMap.GetPixel(workingRow, workingCol).r, minSlope);
            }
        }

        return minSlope;
    }

    void Internal_BuildPathdata(Pathdata pathdata, PathdataSet configuration, int pathdataSize, Vector3 nodeSize)
    {
        // extract key data
        var heightMap = Source_Terrain.terrainData.GetHeights(0, 0, 
                                                              Source_Terrain.terrainData.heightmapResolution, 
                                                              Source_Terrain.terrainData.heightmapResolution);
        var cosSlopeLimit = Mathf.Cos(configuration.SlopeLimit * Mathf.Deg2Rad);

        // build the nodes
        for (int row = 0; row < pathdataSize; ++row)
        {
            for (int column = 0; column < pathdataSize; ++column)
            {
                // retrieve the height
                float height = SampleHeightMap(heightMap, configuration.Resolution, row, column);

                // generate world pos
                Vector3 worldPos = new Vector3((row + 0.5f) * nodeSize.z, height * nodeSize.y, (column + 0.5f) * nodeSize.x);

                // determine the slope
                float cosSlope = SampleSlopeMap(configuration.Resolution, row, column);

                // build the attributes
                EPathdataNodeAttributes attributes = EPathdataNodeAttributes.None;

                if (worldPos.y < WaterHeight)
                    attributes |= EPathdataNodeAttributes.HasWater;
                else if (cosSlope >= cosSlopeLimit)
                    attributes |= EPathdataNodeAttributes.Walkable;

                if (row == 0 || column == 0 || row == (pathdataSize - 1) || column == (pathdataSize - 1))
                    attributes = EPathdataNodeAttributes.IsBoundary;

                // initialise the node
                pathdata.InitialiseNode(row, column, worldPos, attributes);
            }
        }

        // build up the neighbours
        for (int row = 0; row < pathdataSize; ++row)
        {
            for (int column = 0; column < pathdataSize; ++column)
            {
                var node = pathdata.GetNode(row, column);

                // boundary nodes do not connect
                if (node.IsBoundary)
                    continue;

                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.North, GridHelpers.Step_North);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.NorthEast, GridHelpers.Step_NorthEast);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.East, GridHelpers.Step_East);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.SouthEast, GridHelpers.Step_SouthEast);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.South, GridHelpers.Step_South);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.SouthWest, GridHelpers.Step_SouthWest);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.West, GridHelpers.Step_West);
                UpdateNeighbourFlags(node, pathdata, configuration, ENeighbourFlags.NorthWest, GridHelpers.Step_NorthWest);
            }
        }

        BuildAreaIds(pathdata);
    }

    void UpdateNeighbourFlags(PathdataNode currentNode, Pathdata pathdata, PathdataSet configuration,
                              ENeighbourFlags directionFlag, Vector3Int offset)
    {
        var neighbour = pathdata.GetNode(currentNode.GridPos + offset);

        // no neighbour or it's a boundary so do nothing
        if (neighbour == null || neighbour.IsBoundary)
            return;

        // link only if both the same
        if ((neighbour.IsWalkable && currentNode.IsWalkable) || (neighbour.IsWater && currentNode.IsWater))
            currentNode.NeighbourFlags |= directionFlag;

        // if we can connect to water then allow those connections
        if (configuration.CanUseWater && 
            ((neighbour.IsWalkable && currentNode.IsWater) || (neighbour.IsWater && currentNode.IsWalkable)))
        {
            currentNode.NeighbourFlags |= directionFlag;
        }
    }

    void BuildAreaIds(Pathdata pathdata)
    {
        bool[] flaggedForProcessing = new bool[pathdata.Dimensions.x * pathdata.Dimensions.y];

        int currentAreaID = 1;
        int minAreaSize = int.MaxValue;
        int maxAreaSize = int.MinValue;
        for (int index = 0; index < pathdata.Nodes.Length; ++index)
        {
            // skip if already visited
            if (pathdata.Nodes[index].AreaID >= 0)
                continue;

            // node has no connections - mark as isolated
            if (pathdata.Nodes[index].NeighbourFlags == ENeighbourFlags.None)
            {
                pathdata.Nodes[index].AreaID = 0;
                continue;
            }

            int areaSize = FloodFillArea(pathdata.Nodes[index], pathdata, currentAreaID, flaggedForProcessing);

            ++currentAreaID;

            minAreaSize = Mathf.Min(areaSize, minAreaSize);
            maxAreaSize = Mathf.Max(areaSize, maxAreaSize);
        }

        Debug.Log("Min: " + minAreaSize + ", Max: " + maxAreaSize + ", Total: " + currentAreaID);
    }

    int FloodFillArea(PathdataNode seedNode, Pathdata pathdata, int areaID, bool[] flaggedForProcessing)
    {
        Queue<Vector3Int> nodesToProcess = new Queue<Vector3Int>();
        nodesToProcess.Enqueue(seedNode.GridPos);
        int areaSize = 0;

        // while we have things to process
        while (nodesToProcess.Count > 0)
        {
            var node = pathdata.GetNode(nodesToProcess.Dequeue());

            if (node.AreaID >= 0)
                continue;

            // tag the node
            node.AreaID = areaID;
            ++areaSize;

            // add in any neighbours
            if (node.HasNeighbour_N)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_North, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_NE)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_NorthEast, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_E)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_East, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_SE)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_SouthEast, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_S)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_South, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_SW)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_SouthWest, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_W)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_West, nodesToProcess, flaggedForProcessing);
            if (node.HasNeighbour_NW)
                FloodFillArea_Helper(node, pathdata, GridHelpers.Step_NorthWest, nodesToProcess, flaggedForProcessing);
        }

        return areaSize;
    }

    void FloodFillArea_Helper(PathdataNode currentNode, Pathdata pathdata, Vector3Int offset, 
                              Queue<Vector3Int> nodesToProcess, bool[] flaggedForProcessing)
    {
        var neighbour = pathdata.GetNode(currentNode.GridPos + offset);
        var neighbourIndex = neighbour.GridPos.x + neighbour.GridPos.y * pathdata.Dimensions.x;

        // if already flagged then ignore
        if (flaggedForProcessing[neighbourIndex])
            return;

        // if already processed then ignore
        if (neighbour.AreaID < 0)
        {
            nodesToProcess.Enqueue(neighbour.GridPos);
            flaggedForProcessing[neighbourIndex] = true;
        }        
    }
}
