using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneMergeTools
{
    private const string ForestScenePath = "Assets/Scenes/ForestOpenWorld.unity";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const int GroundOrder = -5;
    private const int DetailOrder = -4;
    private const int ObstacleOrder = -3;
    private const int CollisionOrder = -2;

    [MenuItem("Tools/Legacy/Map/Inject Forest Map Into SampleScene")]
    private static void InjectForestMapIntoSampleScene()
    {
        var sample = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var forest = EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Additive);

        var forestGrid = FindRootByName(forest, "Grid");
        if (forestGrid == null)
        {
            Debug.LogError("ForestOpenWorld scene has no root object named 'Grid'.");
            EditorSceneManager.CloseScene(forest, true);
            return;
        }

        var existingGrid = FindRootByName(sample, "Grid");
        if (existingGrid != null)
        {
            forestGrid.name = "Grid_Forest";
        }

        SceneManager.MoveGameObjectToScene(forestGrid, sample);
        ApplyForestTilemapSorting(forestGrid);

        EditorSceneManager.MarkSceneDirty(sample);
        EditorSceneManager.SaveScene(sample);
        EditorSceneManager.CloseScene(forest, true);

        Debug.Log("Forest map injected into SampleScene.");
    }

    [MenuItem("Tools/Legacy/Map/Fix Forest Tilemap Sorting (Active Scene)")]
    private static void FixForestTilemapSorting()
    {
        var active = SceneManager.GetActiveScene();
        if (!active.IsValid())
        {
            Debug.LogError("No active scene.");
            return;
        }

        var root = FindRootByName(active, "Grid");
        if (root == null)
        {
            root = FindRootByName(active, "Grid_Forest");
        }

        if (root == null)
        {
            Debug.LogError("No Grid or Grid_Forest root found in active scene.");
            return;
        }

        ApplyForestTilemapSorting(root);
        EditorSceneManager.MarkSceneDirty(active);
        Debug.Log("Applied forest tilemap sorting orders.");
    }

    private static void ApplyForestTilemapSorting(GameObject root)
    {
        var tilemaps = root.GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>(true);
        foreach (var renderer in tilemaps)
        {
            if (renderer == null)
            {
                continue;
            }

            string name = renderer.gameObject.name;
            if (name == "Ground")
            {
                renderer.sortingOrder = GroundOrder;
            }
            else if (name == "Details")
            {
                renderer.sortingOrder = DetailOrder;
            }
            else if (name == "Obstacles")
            {
                renderer.sortingOrder = ObstacleOrder;
            }
            else if (name == "Collision")
            {
                renderer.sortingOrder = CollisionOrder;
                renderer.enabled = false;
            }
        }
    }

    private static GameObject FindRootByName(Scene scene, string name)
    {
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == name)
            {
                return roots[i];
            }
        }
        return null;
    }
}
