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
    public EType Type => _Type;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
