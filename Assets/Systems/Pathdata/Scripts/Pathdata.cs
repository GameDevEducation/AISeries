using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum EPathdataNodeAttributes : byte
{
    None     = 0x00,

    Walkable = 0x01,

    HasWater = 0x02,
   
    IsBoundary = 0x80
}

[System.Flags]
public enum ENeighbourFlags : byte
{
    None        = 0x00,

    North       = 0x01,
    NorthEast   = 0x02,
    East        = 0x04,
    SouthEast   = 0x08,
    South       = 0x10,
    SouthWest   = 0x20,
    West        = 0x40,
    NorthWest   = 0x80
}

public static class GridHelpers
{
    public static Vector3Int Step_North     = new Vector3Int(0, 1, 0);
    public static Vector3Int Step_NorthEast = new Vector3Int(1, 1, 0);
    public static Vector3Int Step_East      = new Vector3Int(1, 0, 0);
    public static Vector3Int Step_SouthEast = new Vector3Int(1, -1, 0);
    public static Vector3Int Step_South     = new Vector3Int(0, -1, 0);
    public static Vector3Int Step_SouthWest = new Vector3Int(-1, -1, 0);
    public static Vector3Int Step_West      = new Vector3Int(-1, 0, 0);
    public static Vector3Int Step_NorthWest = new Vector3Int(-1, 1, 0);
}

public class PathdataNode : System.IEquatable<PathdataNode>
{
    public Vector3 WorldPos;
    public Vector3Int GridPos;
    public EPathdataNodeAttributes Attributes;
    public ENeighbourFlags NeighbourFlags;
    public int AreaID;

    public int UniqueID;

    public bool IsBoundary => Attributes.HasFlag(EPathdataNodeAttributes.IsBoundary);
    public bool IsWalkable => !IsWater && Attributes.HasFlag(EPathdataNodeAttributes.Walkable);
    public bool IsWater => Attributes.HasFlag(EPathdataNodeAttributes.HasWater);

    public bool HasNeighbour_N => NeighbourFlags.HasFlag(ENeighbourFlags.North);
    public bool HasNeighbour_NE => NeighbourFlags.HasFlag(ENeighbourFlags.NorthEast);
    public bool HasNeighbour_E => NeighbourFlags.HasFlag(ENeighbourFlags.East);
    public bool HasNeighbour_SE => NeighbourFlags.HasFlag(ENeighbourFlags.SouthEast);
    public bool HasNeighbour_S => NeighbourFlags.HasFlag(ENeighbourFlags.South);
    public bool HasNeighbour_SW => NeighbourFlags.HasFlag(ENeighbourFlags.SouthWest);
    public bool HasNeighbour_W => NeighbourFlags.HasFlag(ENeighbourFlags.West);
    public bool HasNeighbour_NW => NeighbourFlags.HasFlag(ENeighbourFlags.NorthWest);

    public override bool Equals(object obj)
    {
        return Equals(obj as PathdataNode);
    }

    public bool Equals(PathdataNode other)
    {
        return other != null && other.UniqueID == UniqueID;
    }
    
    public override int GetHashCode()
    {
        return UniqueID.GetHashCode();
    }

    public static bool operator == (PathdataNode lhs, PathdataNode rhs)
    {
        return EqualityComparer<PathdataNode>.Default.Equals(lhs, rhs);
    }

    public static bool operator != (PathdataNode lhs, PathdataNode rhs)
    {
        return !(lhs == rhs);
    }
}

[System.Serializable]
public class Pathdata : ScriptableObject, ISerializationCallbackReceiver
{
    public string UniqueID;

    public EResolution Resolution = EResolution.NodeSize_1x1;
    public Vector2Int Dimensions;
    public Vector3 CellSize;
    [SerializeField] EPathdataNodeAttributes[] Attributes;
    [SerializeField] ENeighbourFlags[] NeighbourFlags;
    [SerializeField] float[] Heights;
    [SerializeField] int[] AreaIDs;

    [System.NonSerialized] public PathdataNode[] Nodes;

    public void Initialise(string _UniqueID, EResolution _Resolution, Vector2Int _Dimensions, Vector3 _CellSize)
    {
        UniqueID = _UniqueID;
        Resolution = _Resolution;
        Dimensions = _Dimensions;
        CellSize = _CellSize;

        Nodes = new PathdataNode[Dimensions.x * Dimensions.y];     
        for (int index = 0; index < Nodes.Length; ++index)  
            Nodes[index] = new PathdataNode();
    }

    public void InitialiseNode(int row, int column, Vector3 worldPos, EPathdataNodeAttributes attributes)
    {
        int nodeIndex = column + (row * Dimensions.x);

        Nodes[nodeIndex].UniqueID = nodeIndex;
        Nodes[nodeIndex].GridPos = new Vector3Int(column, row, 0);
        Nodes[nodeIndex].WorldPos = worldPos;
        Nodes[nodeIndex].Attributes = attributes;
        Nodes[nodeIndex].AreaID = -1;
    }

    public void OnAfterDeserialize()
    {
        // initialise the nodes
        Nodes = new PathdataNode[Attributes.Length];
        for (int index = 0; index < Attributes.Length; ++index)
        {
            Nodes[index] = new PathdataNode();

            Nodes[index].UniqueID = index;
            Nodes[index].Attributes = Attributes[index];
            Nodes[index].NeighbourFlags = NeighbourFlags[index];
            Nodes[index].AreaID = AreaIDs[index];

            int x = index % Dimensions.x;
            int y = (index - x) / Dimensions.x;
            Nodes[index].GridPos = new Vector3Int(x, y, 0);

            Nodes[index].WorldPos = new Vector3((y + 0.5f) * CellSize.x, Heights[index], (x + 0.5f) * CellSize.z);
        }
    }

    public void OnBeforeSerialize()
    {
        if (Nodes == null || Nodes.Length == 0)
        {
            Attributes = null;
            NeighbourFlags = null;
            Heights = null;
            AreaIDs = null;
            return;
        }

        // extract data from the nodes
        Attributes = new EPathdataNodeAttributes[Nodes.Length];
        NeighbourFlags = new ENeighbourFlags[Nodes.Length];
        Heights = new float[Nodes.Length];
        AreaIDs = new int[Nodes.Length];
        for (int index = 0; index < Attributes.Length; ++index)
        {
            Attributes[index] = Nodes[index].Attributes;
            NeighbourFlags[index] = Nodes[index].NeighbourFlags;
            Heights[index] = Nodes[index].WorldPos.y;
            AreaIDs[index] = Nodes[index].AreaID;
        }
    }

    public PathdataNode GetNode(int uniqueID)
    {
        if (uniqueID < 0 || uniqueID >= Nodes.Length)
            return null;

        int column = uniqueID % Dimensions.x;
        int row = (uniqueID - column) / Dimensions.x;
        return GetNode(row, column);
    }

    public PathdataNode GetNode(Vector3 worldPos)
    {
        int row = Mathf.FloorToInt((worldPos.x / CellSize.x) - 0.5f);
        int column = Mathf.FloorToInt((worldPos.z / CellSize.z) - 0.5f);

        return GetNode(row, column);
    }

    public PathdataNode GetNode(Vector3Int gridPos)
    {
        return GetNode(gridPos.y, gridPos.x);
    }

    public PathdataNode GetNode(int row, int column)
    {
        if (row < 0 || column < 0 || row >= Dimensions.y || column >= Dimensions.x)
            return null;

        return Nodes[column + (row * Dimensions.x)];
    }
}
