using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AIDebuggerWindow : EditorWindow
{
    [MenuItem("AI/Debugger")]
    static void Init()
    {
        EditorWindow.GetWindow<AIDebuggerWindow>().Show();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {        
        // early out if not playing
        if (!Application.isPlaying)
        {
            GUILayout.Label("Game must be playing to use the AI Debugger");
            return;
        }

        // early out if there is no debugger
        if (AIDebugger.Instance == null)
        {
            GUILayout.Label("Unable to find an AI Debugger. Make sure there is one in the scene.");
            return;
        }

        foreach(var kvp in AIDebugger.Instance.TrackedAIs)
        {
            AIDebugger.TrackedAI trackedAI = kvp.Value;
            GOAPBrain brain = kvp.Key;

            trackedAI.IsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(trackedAI.IsExpanded, trackedAI.Name);

            if (trackedAI.IsExpanded)
            {
                GUILayout.Label($"Active Goal: {brain.DebugInfo_ActiveGoal}");
                GUILayout.Label($"Active Action: {brain.DebugInfo_ActiveAction}");

                for (int goalIndex = 0; goalIndex < brain.NumGoals; ++goalIndex)
                {
                    GUILayout.Label(brain.DebugInfo_ForGoal(goalIndex));
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
