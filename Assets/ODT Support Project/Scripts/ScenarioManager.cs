// ScenarioManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour
{
    [Header("Scenario Registry")]
    [SerializeField] private ScenarioConfig[] allScenarios;

    public ScenarioConfig ActiveConfig { get; private set; }
    public TaskEnvironment ActiveEnvironment { get; private set; }
    public Trial ActiveTrial { get; private set; }

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
            Debug.LogWarning("ScenarioManager: No scenario ID in session, falling back to first config.");
            id = allScenarios[0].scenarioId;
        }

        ActiveConfig = FindConfig(id);

        if (ActiveConfig == null)
        {
            Debug.LogError($"ScenarioManager: No ScenarioConfig found for id '{id}'");
            return;
        }

        StartCoroutine(LoadScenario(ActiveConfig));
    }

    private IEnumerator LoadScenario(ScenarioConfig config)
    {
        // Load the additive scene that contains the TaskEnvironment for this scenario
        if (!string.IsNullOrEmpty(config.additiveSceneName))
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(config.additiveSceneName, LoadSceneMode.Additive);
            yield return new WaitUntil(() => op.isDone);
        }

        // TaskEnvironment.Awake() will have run by now and registered itself in instances.
        // Find the one that belongs to our additive scene.
        ActiveEnvironment = FindEnvironmentForScene(config.additiveSceneName);

        if (ActiveEnvironment == null)
        {
            Debug.LogError($"ScenarioManager: No TaskEnvironment found in scene '{config.additiveSceneName}'");
            yield break;
        }

        // The Trial component should already be on the TaskEnvironment (or its GameObject)
        ActiveTrial = ActiveEnvironment.trial;

        if (ActiveTrial == null)
        {
            Debug.LogError($"ScenarioManager: TaskEnvironment in '{config.additiveSceneName}' has no Trial assigned.");
            yield break;
        }

        // Hook completion/failure before starting
        // ActiveTrial.onTrialComplete += OnScenarioComplete;
        // ActiveTrial.onTrialFailed  += OnScenarioFailed;

        Debug.Log($"[ScenarioManager] Loaded '{config.displayName}': {config.briefing}");

        // Trial.Start() in SenquentialGoalTrial calls StartTrial() itself.
        // Only call manually if autoStartTrial is true AND the trial doesn't self-start.
        // if (config.autoStartTrial && !ActiveTrial.HasStarted)
        //     ActiveTrial.StartTrial();
    }

    private void OnScenarioComplete()
    {
        Debug.Log($"[ScenarioManager] Scenario '{ActiveConfig.displayName}' COMPLETE");
        // Hook: show results UI, save progress, return to menu, etc.
    }

    private void OnScenarioFailed()
    {
        Debug.Log($"[ScenarioManager] Scenario '{ActiveConfig.displayName}' FAILED");
        // Hook: show failure UI, offer retry, etc.
    }

    private void OnDestroy()
    {
        if (ActiveTrial != null)
        {
            // ActiveTrial.onTrialComplete -= OnScenarioComplete;
            // ActiveTrial.onTrialFailed  -= OnScenarioFailed;
        }

        if (ActiveConfig != null && !string.IsNullOrEmpty(ActiveConfig.additiveSceneName))
            SceneManager.UnloadSceneAsync(ActiveConfig.additiveSceneName);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private ScenarioConfig FindConfig(string id)
    {
        foreach (var c in allScenarios)
            if (c.scenarioId == id) return c;
        return null;
    }

    private TaskEnvironment FindEnvironmentForScene(string sceneName)
    {
        foreach (var env in TaskEnvironment.instances)
            if (env.sceneName == sceneName) return env;
        return null;
    }
}