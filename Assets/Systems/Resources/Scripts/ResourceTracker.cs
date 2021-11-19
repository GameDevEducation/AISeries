using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceTracker : MonoBehaviour
{
    public static ResourceTracker Instance { get; private set; } = null;

    Dictionary<WorldResource.EType, List<WorldResource>> TrackedResources = new Dictionary<WorldResource.EType, List<WorldResource>>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"ResourceTracker already exists. Destroying newest one on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // setup the tracker for the resources
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        foreach(var value in resourceTypes)
        {
            TrackedResources[(WorldResource.EType)value] = new List<WorldResource>();
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

    public void RegisterResource(WorldResource resource)
    {
        TrackedResources[resource.Type].Add(resource);
    }

    float Distance2D(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Sqrt((pos1.x - pos2.x) * (pos1.x - pos2.x) +
                          (pos1.z - pos2.z) * (pos1.z - pos2.z));
    }

    public List<WorldResource> GetResourcesInRange(WorldResource.EType type, Vector3 location, float range)
    {
        return TrackedResources[type].Where(resource => Distance2D(resource.transform.position, location) <= range).ToList();
    }
}
