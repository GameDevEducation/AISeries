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

    [SerializeField] int MinResourcesToAllowGathering = 20;
    [SerializeField] [Range(0f, 1f)] float GathererProportion = 0.6f;
    [SerializeField] int GatherPickRange = 10;
    [SerializeField] WorldResource.EType DefaultResource = WorldResource.EType.Water;

    List<GOAPBrain> Villagers = new List<GOAPBrain>();
    Dictionary<WorldResource.EType, List<WorldResource>> TrackedResources = null;

    List<GOAPBrain> Gatherers = new List<GOAPBrain>();
    Dictionary<GOAPBrain, WorldResource.EType> GathererAssignments = new Dictionary<GOAPBrain, WorldResource.EType>();

    Dictionary<WorldResource.EType, int> ResourcesStored = new Dictionary<WorldResource.EType, int>();

    public int NumAvailableResources { get; private set; } = 0;

    void Awake()
    {
        // nothing stored to begin with
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        foreach (var value in resourceTypes)
        {
            ResourcesStored[(WorldResource.EType)value] = 0;
        }        
    }

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

            NumAvailableResources += TrackedResources[type].Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TrackedResources == null)
            PopulateResources();
    }

    public int GetAmountStored(WorldResource.EType type)
    {
        return ResourcesStored[type];
    }

    public Vector3 GetRandomSafePoint()
    {
        return SpawnPoints[Random.Range(0, SpawnPoints.Count)].position;
    } 

    public float GetGathererPriority()
    {
        // insufficient resources to gather yet?
        if (NumAvailableResources < MinResourcesToAllowGathering)
            return 0f;
            
        int desiredNumGatherers = Mathf.FloorToInt(GathererProportion * Villagers.Count);
        int currentNumGatherers = Gatherers.Count;
        int surplusGatherers = currentNumGatherers - desiredNumGatherers;

        if (surplusGatherers >= 0)
            return 0f;

        return Mathf.Abs((float)surplusGatherers / (float)desiredNumGatherers);
    }

    public WorldResource GetGatherTarget(GOAPBrain brain)
    {
        int lowestNumGatherers = int.MaxValue;
        WorldResource.EType targetResource = DefaultResource;

        // find the resource most in need
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        foreach(var typeValue in resourceTypes)
        {
            var resourceType = (WorldResource.EType)typeValue;

            // if there are none of this resource type then skip
            if (TrackedResources[resourceType].Count == 0)
                continue;

            // determine how many are gathering this resource
            int numGatherers = 0;
            foreach(var gatheredResource in GathererAssignments.Values)
            {
                if (gatheredResource == resourceType)
                    ++numGatherers;
            }

            // found a new best target?
            if (numGatherers < lowestNumGatherers)
            {
                lowestNumGatherers = numGatherers;
                targetResource = resourceType;
            }
        }

        if (TrackedResources[targetResource].Count == 0)
            return null;

        GathererAssignments[brain] = targetResource;

        var sortedResources = TrackedResources[targetResource].OrderBy(resource => Vector3.Distance(brain.transform.position, resource.transform.position)).ToList();
        return sortedResources[Random.Range(0, Mathf.Min(GatherPickRange, sortedResources.Count))];
    }

    public void SawResource(WorldResource resource)
    {
        if (!TrackedResources[resource.Type].Contains(resource))
        {
            TrackedResources[resource.Type].Add(resource);
            ++NumAvailableResources;
        }
    }

    public void AddGatherer(GOAPBrain brain)
    {
        Gatherers.Add(brain);
    }

    public void RemoveGatherer(GOAPBrain brain)
    {
        Gatherers.Remove(brain);
        GathererAssignments.Remove(brain);
    }

    public void StoreResource(WorldResource.EType type, int amount)
    {
        ResourcesStored[type] += amount;
    }
}
