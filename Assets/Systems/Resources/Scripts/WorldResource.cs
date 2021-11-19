using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldResource : MonoBehaviour
{
    public enum EType
    {
        Food,
        Water,
        Wood
    }
    [SerializeField] EType _Type;
    [SerializeField] int MinAmount = 15;
    [SerializeField] int MaxAmount = 50;

    public EType Type => _Type;
    public int AvailableAmount { get; private set; } = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        ResourceTracker.Instance.RegisterResource(this);
        AvailableAmount = Random.Range(MinAmount, MaxAmount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
