using UnityEngine;
using DG.Tweening;

// Prefab simples: um SpriteRenderer de anel (círculo vazado) + opcionalmente um Light2D.
// As duas armas usam o mesmo prefab, só passando cores diferentes.
public class ImpactRing : MonoBehaviour
{
    public SpriteRenderer ring;
    public float maxScale = 1.6f;
    public float duration = 0.3f;

    public void Play(Color color)
    {
        ring.color = color;
        transform.localScale = Vector3.one * 0.2f;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(maxScale, duration).SetEase(Ease.OutCubic));
        seq.Join(ring.DOFade(0f, duration).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
