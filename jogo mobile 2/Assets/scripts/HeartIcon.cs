using UnityEngine;
using DG.Tweening;

// Coloca em cada ícone de coração. Respira sutilmente sozinho, e dá um punch
// (chamado pelo UIManager) sempre que a vida muda — depois volta a respirar.
public class HeartIcon : MonoBehaviour
{
    public float idleScaleAmount = 0.06f;
    public float idleDuration = 1.1f;

    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        StartIdle(0f);
    }

    void OnDisable()
    {
        rt.DOKill();
    }

    public void StartIdle(float delay)
    {
        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOScale(1f + idleScaleAmount, idleDuration)
            .SetEase(Ease.InOutSine)
            .SetDelay(delay)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Chamado pelo UIManager quando a vida muda (ganhou ou perdeu coração)
    public void Punch(float idleRestartDelay = 0f)
    {
        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOPunchScale(Vector3.one * 0.3f, 0.25f, 6, 0.8f)
            .OnComplete(() => StartIdle(idleRestartDelay));
    }
}
