using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public enum PlayerVisualType
    {
        Mage,
        Warrior,
        DemonLord
    }

    [SerializeField]
    private string mageControllerPath = "Animations/Player_Wizard";

    [SerializeField]
    private string warriorControllerPath = "Animations/Player_Knight";

    [SerializeField]
    private string demonLordControllerPath = "Animations/Player_DemonLord";

    private Animator _animator;

    private void Awake()
    {
        var root = GetOrCreateVisualRoot();
        var renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = root.gameObject.AddComponent<SpriteRenderer>();
        }

        _animator = root.GetComponent<Animator>();
        if (_animator == null)
        {
            _animator = root.gameObject.AddComponent<Animator>();
        }

        if (GetComponent<ActorAnimatorDriver>() == null)
        {
            gameObject.AddComponent<ActorAnimatorDriver>();
        }
    }

    public void SetVisual(PlayerVisualType type)
    {
        string path = mageControllerPath;
        switch (type)
        {
            case PlayerVisualType.Warrior:
                path = warriorControllerPath;
                break;
            case PlayerVisualType.DemonLord:
                path = demonLordControllerPath;
                break;
        }

        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            _animator.runtimeAnimatorController = controller;
            ApplyAnimatorDefaults();
            _animator.Rebind();
            _animator.Update(0f);
        }
    }

    private void ApplyAnimatorDefaults()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.enabled = true;
        _animator.speed = 1f;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (Application.isPlaying && _animator.updateMode != AnimatorUpdateMode.Normal)
        {
            _animator.updateMode = AnimatorUpdateMode.Normal;
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
