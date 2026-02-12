using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBootstrap
{
    private const string BaseSceneName = "SampleScene";
    private static bool _loadRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBaseSceneLoaded()
    {
        if (_loadRequested)
        {
            return;
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (HasGameSession())
        {
            return;
        }

        if (IsSceneLoaded(BaseSceneName))
        {
            SetActiveIfNeeded(BaseSceneName);
            return;
        }

        _loadRequested = true;
        var op = SceneManager.LoadSceneAsync(BaseSceneName, LoadSceneMode.Additive);
        if (op != null)
        {
            op.completed += _ => SetActiveIfNeeded(BaseSceneName);
        }
    }

    private static bool HasGameSession()
    {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        return Object.FindFirstObjectByType<GameSession>(FindObjectsInactive.Include) != null;
#else
        return Object.FindObjectOfType<GameSession>() != null;
#endif
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        return scene.IsValid() && scene.isLoaded;
    }

    private static void SetActiveIfNeeded(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded && SceneManager.GetActiveScene() != scene)
        {
            SceneManager.SetActiveScene(scene);
        }
    }
}
