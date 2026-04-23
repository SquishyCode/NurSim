// ScenarioManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour
{
    [Header("All Scenario Configs")]
    [SerializeField] private ScenarioConfig[] allScenarios;

    // Active state
    public ScenarioConfig ActiveConfig { get; private set; }
    public bool IsComplete { get; private set; }

    // Handlers — assign in Inspector or let manager find them
    // private CollectObjectiveHandler _collectHandler;
    // private CheckpointObjectiveHandler _checkpointHandler;
    private TimedObjectiveHandler _timedHandler;

    // Singleton
    public static ScenarioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        string id = ScenarioSessionData.SelectedScenarioId;

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("ScenarioManager: No scenario ID set. Loading first scenario as fallback.");
            id = allScenarios[0].scenarioId;
        }

        ActiveConfig = FindConfig(id);

        if (ActiveConfig == null)
        {
            Debug.LogError($"ScenarioManager: No config found for ID '{id}'");
            return;
        }

        StartCoroutine(LoadScenario(ActiveConfig));
    }

    private IEnumerator LoadScenario(ScenarioConfig config)
    {
        // Load additive scene
        if (!string.IsNullOrEmpty(config.additiveSceneName))
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(config.additiveSceneName, LoadSceneMode.Additive);
            yield return new WaitUntil(() => load.isDone);
        }

        // Show briefing (hook into your UI here)
        Debug.Log($"[{config.displayName}] {config.briefing}");

        // Boot the appropriate handler
        switch (config.objectiveType)
        {
            case ObjectiveType.Collect:
                BootCollect(config);
                break;
            case ObjectiveType.Checkpoint:
                BootCheckpoint(config);
                break;
            case ObjectiveType.Timed:
                BootTimed(config);
                break;
        }
    }

    // ── Handler bootstraps ───────────────────────────────────────────

    private void BootCollect(ScenarioConfig config)
    {
        //_collectHandler = gameObject.AddComponent<CollectObjectiveHandler>();
        //_collectHandler.Initialize(config.collectableTag, config.collectTargetCount, OnScenarioComplete);
    }

    private void BootCheckpoint(ScenarioConfig config)
    {
        // _checkpointHandler = gameObject.AddComponent<CheckpointObjectiveHandler>();
        // _checkpointHandler.Initialize(config.checkpointTag, OnScenarioComplete);
    }

    private void BootTimed(ScenarioConfig config)
    {
        _timedHandler = gameObject.AddComponent<TimedObjectiveHandler>();
        _timedHandler.Initialize(config.timeLimitSeconds, config, OnScenarioComplete, OnScenarioFailed);
    }

    // ── Completion ───────────────────────────────────────────────────

    public void OnScenarioComplete()
    {
        if (IsComplete) return;
        IsComplete = true;
        Debug.Log($"Scenario '{ActiveConfig.displayName}' COMPLETE");
        // Hook: show results UI, save progress, return to menu, etc.
    }

    public void OnScenarioFailed()
    {
        Debug.Log($"Scenario '{ActiveConfig.displayName}' FAILED");
        // Hook: show failure UI, offer retry, etc.
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private ScenarioConfig FindConfig(string id)
    {
        foreach (var c in allScenarios)
            if (c.scenarioId == id) return c;
        return null;
    }

    private void OnDestroy()
    {
        // Unload additive scene on exit
        if (ActiveConfig != null && !string.IsNullOrEmpty(ActiveConfig.additiveSceneName))
            SceneManager.UnloadSceneAsync(ActiveConfig.additiveSceneName);
    }
}