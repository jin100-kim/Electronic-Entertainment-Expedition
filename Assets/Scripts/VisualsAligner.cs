using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VisualsAligner : MonoBehaviour
{
    [SerializeField]
    private bool centerOnSprite = true;

    [SerializeField]
    private Vector3 extraOffset = Vector3.zero;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (!centerOnSprite || _renderer == null || _renderer.sprite == null)
        {
            return;
        }

        Vector3 center = _renderer.sprite.bounds.center;
        Vector3 scale = transform.localScale;
        Vector3 scaledCenter = new Vector3(center.x * scale.x, center.y * scale.y, center.z * scale.z);
        transform.localPosition = -scaledCenter + extraOffset;
    }
}
