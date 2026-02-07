using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    public enum EnemyVisualType
    {
        Slime,
        Mushroom,
        Skeleton
    }

    [SerializeField]
    private EnemyVisualType visualType = EnemyVisualType.Slime;

    [SerializeField]
    private string slimeControllerPath = "Animations/Enemy_Slime";

    [SerializeField]
    private string mushroomControllerPath = "Animations/Enemy_Mushroom";

    [SerializeField]
    private string skeletonControllerPath = "Animations/Enemy_Skeleton";

    [SerializeField]
    private float visualScale = 4f;

    private Animator _animator;

    private void Awake()
    {
        var root = GetOrCreateVisualRoot();
        root.localScale = Vector3.one * visualScale;

        var renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = root.gameObject.AddComponent<SpriteRenderer>();
        }
        renderer.color = Color.white;

        _animator = root.GetComponent<Animator>();
        if (_animator == null)
        {
            _animator = root.gameObject.AddComponent<Animator>();
        }

        if (GetComponent<ActorAnimatorDriver>() == null)
        {
            gameObject.AddComponent<ActorAnimatorDriver>();
        }

        ApplyVisual();
    }

    public void SetType(EnemyVisualType type)
    {
        visualType = type;
        ApplyVisual();
    }

    public void SetVisualScale(float scale)
    {
        visualScale = Mathf.Max(0.1f, scale);
        var root = GetOrCreateVisualRoot();
        root.localScale = Vector3.one * visualScale;
    }

    private void ApplyVisual()
    {
        string path = slimeControllerPath;
        switch (visualType)
        {
            case EnemyVisualType.Mushroom:
                path = mushroomControllerPath;
                break;
            case EnemyVisualType.Skeleton:
                path = skeletonControllerPath;
                break;
        }

        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            _animator.runtimeAnimatorController = controller;
        }
    }

    private Transform GetOrCreateVisualRoot()
    {
        var existing = transform.Find("Visuals");
        if (existing != null)
        {
            EnsureAligner(existing.gameObject);
            return existing;
        }

        var root = new GameObject("Visuals");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        EnsureAligner(root);
        return root.transform;
    }

    private static void EnsureAligner(GameObject root)
    {
        if (root.GetComponent<VisualsAligner>() == null)
        {
            root.AddComponent<VisualsAligner>();
        }
    }
}
