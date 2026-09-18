using UnityEngine;
using DG.Tweening;

// Coloque na Main Camera. Outros scripts chamam CameraShake.Instance.Shake(...)
// sempre que algo de impactante acontecer (dano no player, morte de inimigo, etc).
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    Vector3 originalLocalPos;

    void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    public void Shake(float duration = 0.15f, float strength = 0.25f, int vibrato = 20)
    {
        transform.DOKill();
        transform.localPosition = originalLocalPos;
        transform.DOShakePosition(duration, strength, vibrato, 90f, false, true)
            .OnComplete(() => transform.localPosition = originalLocalPos);
    }
}
