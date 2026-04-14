// ScenarioConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewScenario", menuName = "Scenarios/Scenario Config")]
public class ScenarioConfig : ScriptableObject
{
    [Header("Identity")]
    public string scenarioId;           // must match ScenarioSessionData value
    public string displayName;
    [TextArea] public string briefing;  // shown to player at start

    [Header("Additive Scene")]
    public string additiveSceneName;    // scene name to load on top of base

    [Header("Objectives")]
    public ObjectiveType objectiveType;
    
    [Header("Collect Settings")]
    public int collectTargetCount;
    public string collectableTag = "Collectable";

    [Header("Checkpoint Settings")]
    public string checkpointTag = "Checkpoint";

    [Header("Timed Settings")]
    public float timeLimitSeconds;
    public ObjectiveType timedInnerObjective; // what to do within the time limit
}

public enum ObjectiveType
{
    Collect,
    Checkpoint,
    Timed
}