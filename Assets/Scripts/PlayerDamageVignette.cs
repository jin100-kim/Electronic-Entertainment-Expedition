using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Health))]
public class PlayerDamageVignette : MonoBehaviour
{
    [SerializeField]
    private Color edgeColor = new Color(1f, 0.1f, 0.1f, 1f);

    [SerializeField]
    private float maxAlpha = 0.9f;

    [SerializeField]
    private float fadeDuration = 0.25f;

    [SerializeField]
    [Range(0.02f, 0.2f)]
    private float edgeSize = 0.035f;

    [SerializeField]
    [Range(2, 10)]
    private int gradientSteps = 5;

    [SerializeField]
    private float gradientExponent = 1.6f;

    private Health _health;
    private float _timer;
    private float _alpha;
    private Texture2D _pixel;

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
        {
            _health.OnDamaged += OnDamaged;
        }

        _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamaged -= OnDamaged;
        }
    }

    private void Update()
    {
        if (_timer <= 0f)
        {
            _alpha = 0f;
            return;
        }

        _timer -= Time.deltaTime;
        float t = Mathf.Clamp01(_timer / Mathf.Max(0.01f, fadeDuration));
        _alpha = maxAlpha * t;
    }

    private void OnDamaged(float amount)
    {
        if (!ShouldShowEffect())
        {
            return;
        }

        _timer = fadeDuration;
        _alpha = maxAlpha;
    }

    private bool ShouldShowEffect()
    {
        var net = GetComponent<NetworkBehaviour>();
        if (net != null)
        {
            if (net.IsOwner)
            {
                return true;
            }

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private void OnGUI()
    {
        if (_alpha <= 0f || _pixel == null)
        {
            return;
        }

        float thickness = Mathf.Max(4f, Mathf.Min(Screen.width, Screen.height) * edgeSize);
        int steps = Mathf.Max(2, gradientSteps);
        float stepThickness = thickness / steps;
        var prev = GUI.color;

        for (int i = 0; i < steps; i++)
        {
            float t = steps <= 1 ? 1f : 1f - (i / (float)(steps - 1));
            float a = Mathf.Pow(t, Mathf.Max(0.1f, gradientExponent)) * _alpha;
            var color = edgeColor;
            color.a = Mathf.Clamp01(a);
            GUI.color = color;

            float inset = i * stepThickness;
            float size = stepThickness;
            GUI.DrawTexture(new Rect(0f, inset, Screen.width, size), _pixel);
            GUI.DrawTexture(new Rect(0f, Screen.height - inset - size, Screen.width, size), _pixel);
            GUI.DrawTexture(new Rect(inset, 0f, size, Screen.height), _pixel);
            GUI.DrawTexture(new Rect(Screen.width - inset - size, 0f, size, Screen.height), _pixel);
        }

        GUI.color = prev;
    }
}
