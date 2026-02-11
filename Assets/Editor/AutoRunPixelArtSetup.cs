#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AutoRunPixelArtSetup
{
    private const string FlagPath = "Assets/Editor/AutoRunPixelArtSetup.flag";

    static AutoRunPixelArtSetup()
    {
        EditorApplication.delayCall += TryRunOnce;
    }

    private static void TryRunOnce()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (!File.Exists(FlagPath))
        {
            return;
        }

        File.Delete(FlagPath);
        PixelArtAnimationSetup.SetupAnimations();
        Debug.Log("[AutoRunPixelArtSetup] PixelArt animations regenerated.");
    }
}
#endif
