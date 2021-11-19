using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPBrain : MonoBehaviour
{
    BaseGoal[] Goals;
    BaseAction[] Actions;

    BaseGoal ActiveGoal;
    BaseAction ActiveAction;

    public string DebugInfo_ActiveGoal => ActiveGoal != null ? ActiveGoal.GetType().Name : "None";
    public string DebugInfo_ActiveAction => ActiveAction != null ? $"{ActiveAction.GetType().Name}{ActiveAction.GetDebugInfo()}" : "None";
    public int NumGoals => Goals.Length;

    public string DebugInfo_ForGoal(int index)
    {
        return Goals[index].GetDebugInfo();
    }

    void Awake()
    {
        Goals = GetComponents<BaseGoal>();
        Actions = GetComponents<BaseAction>();        
    }

    void Start()
    {
        if (AIDebugger.Instance != null)
            AIDebugger.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (AIDebugger.Instance != null)
            AIDebugger.Instance.Deregister(this);
    }

    void Update()
    {
        // pretick all goals to refresh priorities
        for (int goalIndex = 0; goalIndex < Goals.Length; ++goalIndex)
            Goals[goalIndex].PreTick();

        RefreshPlan();

        if (ActiveGoal != null)
        {
            ActiveGoal.Tick();

            // if action finished - cleanup the goal
            if (ActiveAction.HasFinished)
            {
                ActiveGoal.GoToSleep();
                ActiveGoal = null;
                ActiveAction = null;
            }
        }
    }

    void RefreshPlan()
    {
        // find the best goal-action pair
        BaseGoal bestGoal = null;
        BaseAction bestAction = null;
        for (int goalIndex = 0; goalIndex < Goals.Length; ++goalIndex)
        {
            var candidateGoal = Goals[goalIndex];

            // skip if can't run
            if (!candidateGoal.CanRun)
                continue;

            // skip if current best goal is a higher priority
            if (bestGoal != null && bestGoal.Priority > candidateGoal.Priority)
                continue;

            // find the cheapest action for this goal
            BaseAction bestActionForCandidateGoal = null;
            for (int actionIndex = 0; actionIndex < Actions.Length; ++actionIndex)
            {
                var candidateAction = Actions[actionIndex];

                // skip if action cannot satisfy the goal
                if (!candidateAction.CanSatisfy(candidateGoal))
                    continue;

                // is this action more expensive - if so skip
                if (bestActionForCandidateGoal != null && candidateAction.Cost() > bestActionForCandidateGoal.Cost())
                    continue;

                bestActionForCandidateGoal = candidateAction;
            }

            // found a viable action
            if (bestActionForCandidateGoal != null)
            {
                bestGoal = candidateGoal;
                bestAction = bestActionForCandidateGoal;
            }
        }

        // current plan holds - do nothing
        if (bestGoal == ActiveGoal && bestAction == ActiveAction)
            return;

        // no plan viable currently
        if (bestGoal == null)
        {
            if (ActiveGoal != null)
                ActiveGoal.GoToSleep();

            ActiveGoal = null;
            ActiveAction = null;
            return;
        }

        // goal has changed?
        if (bestGoal != ActiveGoal)
        {
            if (ActiveGoal != null)
                ActiveGoal.GoToSleep();

            bestGoal.Wakeup();
        }

        // start the action
        ActiveGoal = bestGoal;
        ActiveAction = bestAction;
        ActiveGoal.SetAction(ActiveAction);
    }
}
