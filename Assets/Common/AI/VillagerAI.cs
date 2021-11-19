using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AwarenessSystem))]
public class VillagerAI : BaseAI
{
    AIState LinkedAIState;

    void Start()
    {
        LinkedAIState = GetComponent<AIState>();
    }

    public override void OnSuspicious()
    {
    }

    public override void OnDetected(GameObject target)
    {
    }

    public override void OnFullyDetected(GameObject target)
    {
        FeedbackDisplay.text = "I last saw " + target.gameObject.name;

        WorldResource foundResource;
        if (target.TryGetComponent<WorldResource>(out foundResource))
            LinkedAIState.Home.SawResource(foundResource);
    }

    public override void OnLostDetect(GameObject target)
    {
    }

    public override void OnLostSuspicion()
    {
    }

    public override void OnFullyLost()
    {
        FeedbackDisplay.text = string.Empty;
    }
}
