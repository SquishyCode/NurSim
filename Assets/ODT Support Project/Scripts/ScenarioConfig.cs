// ScenarioConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewScenario", menuName = "Scenarios/Scenario Config")]
public class ScenarioConfig : ScriptableObject
{
    [Header("Identity")]
    public string scenarioId;
    public string displayName;
    [TextArea] public string briefing;

    [Header("Scene Loading")]
    public string additiveSceneName;    // must match the scene name in Build Settings

    [Header("Trial")]
    // The Trial component lives in the additive scene on the TaskEnvironment.
    // Set this to match whichever Trial subclass that scene uses
    // (e.g. SenquentialGoalTrial). ScenarioManager will find it automatically
    // via TaskEnvironment — no extra wiring needed here.
    public bool autoStartTrial = true;  // if false, you start it manually
}

public enum ObjectiveType
{
    Collect,
    Checkpoint,
    Timed
}