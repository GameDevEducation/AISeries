using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDebugger : MonoBehaviour
{
    public static AIDebugger Instance { get; private set; } = null;

    public class TrackedAI
    {
        public string Name;
        public bool IsExpanded;
    }

    public Dictionary<GOAPBrain, TrackedAI> TrackedAIs { get; private set; } = new Dictionary<GOAPBrain, TrackedAI>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Trying to create a new AIDebugger when on already exists. Destroying {gameObject.name}");
            Destroy(Instance.gameObject);
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

    public void Register(GOAPBrain brain)
    {
        TrackedAIs[brain] = new TrackedAI() { Name = brain.gameObject.name };
    }

    public void Deregister(GOAPBrain brain)
    {
        TrackedAIs.Remove(brain);
    }    
}
