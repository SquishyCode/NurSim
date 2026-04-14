// TimedObjectiveHandler.cs
using System;
using UnityEngine;

public class TimedObjectiveHandler : MonoBehaviour
{
    private float _timeLimit;
    private float _elapsed;
    private bool _running;
    private Action _onComplete;
    private Action _onFailed;

    // Inner objective (collect or checkpoint) nested inside the timer
    // private CollectObjectiveHandler _innerCollect;
    // private CheckpointObjectiveHandler _innerCheckpoint;

    public float TimeRemaining => Mathf.Max(0, _timeLimit - _elapsed);

    public void Initialize(float timeLimit, ScenarioConfig config, Action onComplete, Action onFailed)
    {
        _timeLimit = timeLimit;
        _onComplete = onComplete;
        _onFailed = onFailed;
        _running = true;

        // Boot inner objective
        switch (config.timedInnerObjective)
        {
            // case ObjectiveType.Collect:
            //     _innerCollect = gameObject.AddComponent<CollectObjectiveHandler>();
            //     _innerCollect.Initialize(config.collectableTag, config.collectTargetCount, OnInnerComplete);
            //     break;
            // case ObjectiveType.Checkpoint:
            //     _innerCheckpoint = gameObject.AddComponent<CheckpointObjectiveHandler>();
            //     _innerCheckpoint.Initialize(config.checkpointTag, OnInnerComplete);
            //     break;
        }

        Debug.Log($"Timed objective started: {_timeLimit}s");
    }

    private void Update()
    {
        if (!_running) return;
        _elapsed += Time.deltaTime;
        if (_elapsed >= _timeLimit)
        {
            _running = false;
            Debug.Log("Time's up!");
            _onFailed?.Invoke();
        }
    }

    private void OnInnerComplete()
    {
        _running = false;
        _onComplete?.Invoke();
    }
}