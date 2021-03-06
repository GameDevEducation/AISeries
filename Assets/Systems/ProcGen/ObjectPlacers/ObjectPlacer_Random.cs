using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class ObjectPlacer_Random : BaseObjectPlacer
{
    [SerializeField] float TargetDensity = 0.1f;
    [SerializeField] int MaxSpawnCount = 1000;
    [SerializeField] GameObject Prefab;

    public override void Execute(Transform objectRoot, int mapResolution, float[,] heightMap, Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfigSO biome = null)
    {
        // get potential spawn location
        List<Vector3> candidateLocations = GetAllLocationsForBiome(mapResolution, heightMap, heightmapScale, biomeMap, biomeIndex);

        int numToSpawn = Mathf.FloorToInt(Mathf.Min(MaxSpawnCount, candidateLocations.Count * TargetDensity));
        for (int index = 0; index < numToSpawn; ++index)
        {
            // pick a random location to spawn at
            int randomLocationIndex = Random.Range(0, candidateLocations.Count);
            Vector3 spawnLocation = candidateLocations[randomLocationIndex];
            candidateLocations.RemoveAt(randomLocationIndex);

#if UNITY_EDITOR
            GameObject newObject = PrefabUtility.InstantiatePrefab(Prefab, objectRoot) as GameObject;
            newObject.transform.position = spawnLocation;
            Undo.RegisterCreatedObjectUndo(newObject, "Spawn object");
#else
            // instantiate the prefab
            Instantiate(Prefab, spawnLocation, Quaternion.identity, objectRoot);
#endif
        }
    }
}
