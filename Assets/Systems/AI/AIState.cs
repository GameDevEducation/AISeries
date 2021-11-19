using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState : MonoBehaviour
{
    [SerializeField] float FearStartDistance = 75f;
    [SerializeField] float FearMultipliedDistance = 50f;
    [SerializeField] float FearBuildRate = 0.01f;
    [SerializeField] float FearDecayRate = 0.04f;

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
        // get the 2D distance
        var vecToHome = Home.transform.position - transform.position;
        vecToHome.y = 0f;
        float distToHome = vecToHome.magnitude;

        // is fear increasing
        if (distToHome >= FearStartDistance)
        {
            float fearMultiplier = 1f + Mathf.Floor((distToHome - FearStartDistance) / FearMultipliedDistance);

            Fear = Mathf.Clamp01(Fear + FearBuildRate * Time.deltaTime * fearMultiplier);
        }
        else
            Fear = Mathf.Clamp01(Fear - FearDecayRate * Time.deltaTime);
    }
}
