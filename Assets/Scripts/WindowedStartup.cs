using UnityEngine;

public class WindowedStartup : MonoBehaviour
{
    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private int width = 1280;

    [SerializeField]
    private int height = 720;

    [SerializeField]
    private FullScreenMode fullscreenMode = FullScreenMode.Windowed;

    private void Awake()
    {
        ApplySettings();
        Application.runInBackground = true;
        Screen.SetResolution(width, height, fullscreenMode);
        Screen.fullScreenMode = fullscreenMode;
        Screen.fullScreen = fullscreenMode != FullScreenMode.Windowed;
    }

    private void ApplySettings()
    {
        var config = gameConfig != null ? gameConfig : GameConfig.LoadOrCreate();
        var settings = config.window;

        width = settings.width;
        height = settings.height;
        fullscreenMode = settings.fullscreenMode;
    }
}
