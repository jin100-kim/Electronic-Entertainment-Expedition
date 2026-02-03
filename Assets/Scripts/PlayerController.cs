using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private Color playerColor = new Color(0.2f, 0.9f, 0.3f, 1f);

    [SerializeField]
    private bool allowOfflineControl = true;

    private const int SpriteSize = 50;
    private float _moveSpeedMult = 1f;

    private void Awake()
    {
        EnsureVisual();
        EnsurePhysics();
        EnsureHealth();
    }

    private void Update()
    {
        if (!CanReadInput())
        {
            return;
        }

        Vector2 input = ReadMovement();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        float speed = moveSpeed * _moveSpeedMult;
        Vector2 delta = input * speed * Time.deltaTime;
        transform.Translate(delta, Space.World);

        if (GameSession.Instance != null)
        {
            transform.position = GameSession.Instance.ClampToBounds(transform.position);
        }
    }

    public void SetMoveSpeedMultiplier(float value)
    {
        _moveSpeedMult = Mathf.Max(0.1f, value);
    }

    private bool CanReadInput()
    {
        if (IsOwner)
        {
            return true;
        }

        if (!allowOfflineControl)
        {
            return false;
        }

        return NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening;
    }

    private static Vector2 ReadMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

        return new Vector2(x, y);
    }

    private void EnsureVisual()
    {
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        transform.localScale = Vector3.one;

        if (renderer.sprite == null)
        {
            renderer.sprite = CreateCircleSprite(SpriteSize);
        }

        renderer.color = playerColor;
    }

    private void EnsurePhysics()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }

        col.isTrigger = true;
        col.radius = 0.5f;
    }

    private void EnsureHealth()
    {
        if (GetComponent<Health>() == null)
        {
            gameObject.AddComponent<Health>();
        }
    }

    private static Sprite CreateCircleSprite(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var colors = new Color32[size * size];
        float r = (size - 1) * 0.5f;
        float cx = r;
        float cy = r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                bool inside = (dx * dx + dy * dy) <= r * r;
                colors[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
