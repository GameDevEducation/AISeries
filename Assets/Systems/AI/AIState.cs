using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState : MonoBehaviour
{
    public Village Home { get; private set; } = null;
    public float Fear { get; private set; } = 0f;

    public void SetHome(Village _Home)
    {
        Home = _Home;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
