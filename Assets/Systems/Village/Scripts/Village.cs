using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Village : MonoBehaviour
{
    [SerializeField] List<Building> Buildings;
    [SerializeField] GameObject VillagerPrefab;
    [SerializeField] List<Transform> SpawnPoints;
    [SerializeField] int InitialPopulation = 10;
    [SerializeField] float PerfectKnowledgeRange = 100f;

    List<GOAPBrain> Villagers = new List<GOAPBrain>();
    Dictionary<WorldResource.EType, List<WorldResource>> TrackedResources = null;

    // Start is called before the first frame update
    void Start()
    {
        List<Transform> workingSpawnPoints = new List<Transform>(SpawnPoints);

        // spawn the initial villager population
        for (int index = 0; index < InitialPopulation; ++index)
        {
            // pick a spawn point
            int spawnIndex = Random.Range(0, workingSpawnPoints.Count);
            Transform spawnPoint = workingSpawnPoints[spawnIndex];
            workingSpawnPoints.RemoveAt(spawnIndex);

            // spawn the villager and track them
            var villager = Instantiate(VillagerPrefab, spawnPoint.position, spawnPoint.rotation);
            villager.name = $"{gameObject.name}_Villager_{(index + 1)}";
            Villagers.Add(villager.GetComponent<GOAPBrain>());

            villager.GetComponent<AIState>().SetHome(this);
        }
    }

    void PopulateResources()
    {
        // build up the resource knowledge
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        TrackedResources = new Dictionary<WorldResource.EType, List<WorldResource>>();
        foreach (var value in resourceTypes)
        {
            var type = (WorldResource.EType)value;
            TrackedResources[type] = ResourceTracker.Instance.GetResourcesInRange(type, transform.position, PerfectKnowledgeRange);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TrackedResources == null)
            PopulateResources();
    }

    public Vector3 GetRandomSafePoint()
    {
        return SpawnPoints[Random.Range(0, SpawnPoints.Count)].position;
    } 
}
