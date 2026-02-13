using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    private const string DefaultWindowsBuildPath = "Builds/Windows/My project.exe";

    [MenuItem("Tools/Build/Build Windows")]
    public static void BuildWindowsMenu()
    {
        BuildWindows();
    }

    public static void BuildWindows()
    {
        BuildForTarget(BuildTarget.StandaloneWindows64, DefaultWindowsBuildPath, BuildOptions.None);
    }

    private static void BuildForTarget(BuildTarget target, string locationPathName, BuildOptions options)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene != null && scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new BuildFailedException("Build Settings에 활성화된 씬이 없습니다.");
        }

        string dir = Path.GetDirectoryName(locationPathName);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            target = target,
            locationPathName = locationPathName,
            options = options
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException($"빌드 실패: {summary.result}");
        }
    }
}
