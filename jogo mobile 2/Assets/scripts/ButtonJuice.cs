using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// Adiciona vida a qualquer botão/painel — no estilo Balatro: aperta e "esmaga"
// um pouco, solta e dá uma pequena "explosão" de escala/rotação, e enquanto
// parado respira sutilmente. Só adicionar no mesmo objeto do Button/Image.
[RequireComponent(typeof(RectTransform))]
public class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Press (dedo encostado)")]
    public float pressScale = 0.92f;
    public float pressDuration = 0.08f;

    [Header("Click (soltar)")]
    public float clickPunchScale = 0.18f;
    public float clickPunchRotation = 6f;
    public float clickDuration = 0.35f;

    [Header("Idle (parado) — desative em botões que já têm seu próprio pulso, tipo os de arma")]
    public bool idleWobble = true;
    public float idleScaleAmount = 0.02f;
    public float idleDuration = 1.8f;

    RectTransform rt;
    Vector3 baseScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
    }

    void OnEnable()
    {
        if (idleWobble) StartIdle();
    }

    void OnDisable()
    {
        rt.DOKill();
    }

    void StartIdle()
    {
        rt.DOKill();
        rt.localScale = baseScale;
        rt.DOScale(baseScale * (1f + idleScaleAmount), idleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerDown(PointerEventData e)
    {
        rt.DOKill();
        rt.localRotation = Quaternion.identity;
        rt.DOScale(baseScale * pressScale, pressDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData e)
    {
        rt.DOKill();
        rt.localRotation = Quaternion.identity;
        rt.DOScale(baseScale, pressDuration).SetEase(Ease.OutBack);
    }

    public void OnPointerClick(PointerEventData e)
    {
        rt.DOKill();
        rt.localScale = baseScale;
        rt.localRotation = Quaternion.identity; // sem isso, cliques rápidos acumulavam rotação

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOPunchScale(Vector3.one * clickPunchScale, clickDuration, 6, 0.8f));
        seq.Join(rt.DOPunchRotation(new Vector3(0f, 0f, clickPunchRotation), clickDuration, 8, 0.9f));
        seq.OnComplete(() =>
        {
            rt.localScale = baseScale;
            rt.localRotation = Quaternion.identity;
            if (idleWobble) StartIdle();
        });
    }
}
