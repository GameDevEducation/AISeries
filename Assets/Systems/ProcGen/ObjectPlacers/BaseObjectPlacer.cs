using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjectPlacer : MonoBehaviour
{
    [SerializeField] protected bool LimitToHeightRange = false;
    [SerializeField] protected float MinHeight = 0f;
    [SerializeField] protected float MaxHeight = 0f;

    protected List<Vector3> GetAllLocationsForBiome(int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap, int biomeIndex)
    {
        List<Vector3> locations = new List<Vector3>(mapResolution * mapResolution / 10);

        for (int y = 0; y < mapResolution; ++y)
        {
            for (int x = 0; x < mapResolution; ++x)
            {
                if (biomeMap[x, y] != biomeIndex)
                    continue;

                float height = heightMap[x, y] * heightmapScale.y;

                // outside of height range
                if (LimitToHeightRange && (height < MinHeight || height > MaxHeight))
                    continue;

                locations.Add(new Vector3(y * heightmapScale.z, height, x * heightmapScale.x));
            }
        }

        return locations;
    }

    public virtual void Execute(Transform objectRoot, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        Debug.LogError("No implementation of Execute function for " + gameObject.name);
    }
}
