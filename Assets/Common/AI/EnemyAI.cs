using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AwarenessSystem))]
public class EnemyAI : BaseAI
{
    public override void OnSuspicious()
    {
        FeedbackDisplay.text = "I hear you";
    }

    public override void OnDetected(GameObject target)
    {
        FeedbackDisplay.text = "I see you " + target.gameObject.name;
    }

    public override void OnFullyDetected(GameObject target)
    {
        FeedbackDisplay.text = "Charge! " + target.gameObject.name;
    }

    public override void OnLostDetect(GameObject target)
    {
        FeedbackDisplay.text = "Where are you " + target.gameObject.name;
    }

    public override void OnLostSuspicion()
    {
        FeedbackDisplay.text = "Where did you go";
    }

    public override void OnFullyLost()
    {
        FeedbackDisplay.text = "Must be nothing";
    }
}
