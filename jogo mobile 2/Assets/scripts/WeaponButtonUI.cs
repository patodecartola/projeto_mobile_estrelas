using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Coloca num botão de arma (Divisor ou Ricochete), junto com um RoundedPanelSkin
// no mesmo objeto/filho. Escuta o WeaponManager e se destaca sozinho quando
// essa arma vira a selecionada: cresce, fica mais brilhante e pulsa devagar.
public class WeaponButtonUI : MonoBehaviour
{
    public WeaponType weaponType;
    public RoundedPanelSkin skin;

    [Header("Cor — ajuste pra combinar com a arma (laranja no Divisor, ciano no Ricochete)")]
    public Color activeFill = new Color32(0x1C, 0x13, 0x0D, 0xF0);
    public Color activeBorder = new Color32(0xFF, 0x9E, 0x59, 0xFF);
    public Color inactiveFill = new Color32(0x14, 0x17, 0x1C, 0xC0);
    public Color inactiveBorder = new Color32(0x33, 0x38, 0x44, 0xFF);

    [Header("Juice da seleção")]
    public float selectedScale = 1.08f;
    public float scaleDuration = 0.25f;
    public float idlePulseAmount = 0.03f;
    public float idlePulseDuration = 0.9f;

    [Header("Halo de brilho (opcional — Image filho, atrás do botão, Raycast Target desmarcado)")]
    public Image glowImage;
    public float glowPulseMin = 0.25f;
    public float glowPulseMax = 0.55f;
    public float glowPulseDuration = 1.1f;

    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (glowImage != null)
        {
            if (glowImage.sprite == null) glowImage.sprite = UIGlowSprite.Get();
            glowImage.color = new Color(activeBorder.r, activeBorder.g, activeBorder.b, 0f);
        }
    }

    void OnEnable()
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.OnWeaponChanged += HandleWeaponChanged;
            HandleWeaponChanged(WeaponManager.Instance.Current);
        }
    }

    void OnDisable()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponChanged -= HandleWeaponChanged;
        rt.DOKill();
        if (glowImage != null) glowImage.DOKill();
    }

    void HandleWeaponChanged(WeaponType current)
    {
        bool selected = current == weaponType;
        rt.DOKill();

        if (selected)
        {
            rt.DOScale(selectedScale, scaleDuration).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    rt.DOScale(selectedScale * (1f + idlePulseAmount), idlePulseDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        }
        else
        {
            rt.DOScale(1f, scaleDuration).SetEase(Ease.OutBack);
        }

        if (skin != null)
            skin.Apply(selected ? activeFill : inactiveFill, selected ? activeBorder : inactiveBorder);

        if (glowImage != null)
        {
            glowImage.DOKill();
            if (selected)
            {
                glowImage.color = new Color(activeBorder.r, activeBorder.g, activeBorder.b, 0f);
                glowImage.DOFade(glowPulseMax, 0.3f).OnComplete(() =>
                {
                    glowImage.DOFade(glowPulseMin, glowPulseDuration)
                        .SetLoops(-1, LoopType.Yoyo);
                });
            }
            else
            {
                glowImage.DOFade(0f, 0.25f);
            }
        }
    }
}

