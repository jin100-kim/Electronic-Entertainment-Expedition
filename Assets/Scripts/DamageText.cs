using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private static readonly Stack<DamageText> Pool = new Stack<DamageText>();
    private static Transform _root;

    private TextMesh _text;
    private MeshRenderer _renderer;
    private float _timer;
    private float _lifetime;
    private Vector3 _velocity;
    private Color _baseColor;

    public static void Spawn(Vector3 position, float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        var instance = GetFromPool();
        instance.Setup(position, amount);
    }

    private static DamageText GetFromPool()
    {
        while (Pool.Count > 0)
        {
            var item = Pool.Pop();
            if (item != null)
            {
                return item;
            }
        }

        if (_root == null)
        {
            var rootGo = GameObject.Find("DamageTextPool");
            if (rootGo == null)
            {
                rootGo = new GameObject("DamageTextPool");
            }
            _root = rootGo.transform;
        }

        var go = new GameObject("DamageText");
        go.transform.SetParent(_root, false);
        var text = go.AddComponent<TextMesh>();
        var renderer = go.GetComponent<MeshRenderer>();

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            text.font = font;
        }
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;
        text.characterSize = 0.12f;
        text.fontSize = 40;

        renderer.sortingOrder = 300;

        var instance = go.AddComponent<DamageText>();
        instance._text = text;
        instance._renderer = renderer;
        return instance;
    }

    private void Setup(Vector3 position, float amount)
    {
        _timer = 0f;
        _lifetime = 0.8f;
        _velocity = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.8f, 1.2f), 0f);
        _baseColor = new Color(1f, 0.35f, 0.25f, 1f);

        transform.position = position + new Vector3(0f, 0.4f, 0f);
        transform.localScale = Vector3.one;

        if (_text != null)
        {
            _text.text = Mathf.RoundToInt(amount).ToString();
            _text.color = _baseColor;
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;

        float t = Mathf.Clamp01(_timer / Mathf.Max(0.01f, _lifetime));
        if (_text != null)
        {
            var c = _baseColor;
            c.a = 1f - t;
            _text.color = c;
        }

        if (t >= 1f)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        Pool.Push(this);
    }
}
