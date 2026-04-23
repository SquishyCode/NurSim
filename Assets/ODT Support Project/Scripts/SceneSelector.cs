// SceneSelector.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour
{
    [Header("Game scene to load after selection")]
    [SerializeField] private string gameSceneName = "GameScene";

    /// <summary>
    /// Wire this to each scenario button's OnClick() in the Inspector.
    /// Pass the scenarioId string that matches your ScenarioConfig asset.
    /// </summary>
    public void SelectScenario(string scenarioId)
    {
        ScenarioSessionData.SelectedScenarioId = scenarioId;
        SceneManager.LoadScene(gameSceneName);
    }
}