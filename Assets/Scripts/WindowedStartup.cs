using UnityEngine;

public class WindowedStartup : MonoBehaviour
{
    [SerializeField]
    private int width = 1280;

    [SerializeField]
    private int height = 720;

    [SerializeField]
    private FullScreenMode fullscreenMode = FullScreenMode.Windowed;

    private void Awake()
    {
        Screen.SetResolution(width, height, fullscreenMode);
    }
}
