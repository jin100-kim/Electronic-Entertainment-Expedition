using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VisualsAligner : MonoBehaviour
{
    [SerializeField]
    private bool centerOnSprite = true;

    [SerializeField]
    private Vector3 extraOffset = Vector3.zero;

    [SerializeField]
    private bool lockCenterOnStart = false;

    private SpriteRenderer _renderer;
    private bool _locked;
    private Vector3 _lockedPosition;

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
        if (lockCenterOnStart)
        {
            if (!_locked)
            {
                _lockedPosition = -scaledCenter + extraOffset;
                _locked = true;
            }
            transform.localPosition = _lockedPosition;
            return;
        }

        transform.localPosition = -scaledCenter + extraOffset;
    }

    public void SetLockCenterOnStart(bool value)
    {
        lockCenterOnStart = value;
        _locked = false;
    }
}
