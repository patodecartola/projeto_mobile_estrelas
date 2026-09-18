using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public Image[] hearts; // arraste os ícones de coração na cena, na ordem

    [Header("Cor do texto de score (fica mais quente com o combo alto)")]
    public Color scoreColorNormal = new Color(0.93f, 0.95f, 0.96f);
    public Color scoreColorHot = new Color32(0xFF, 0x9E, 0x59, 0xFF);
    public int comboForMaxScoreColor = 10;

    [Header("Cor do texto de combo (esfria conforme o tempo pra expirar acaba)")]
    public Color comboColorHot = new Color32(0xFF, 0x9E, 0x59, 0xFF);
    public Color comboColorCold = new Color32(0xFF, 0x4D, 0x5E, 0xFF);

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text currencyEarnedText;
    public TMP_Text totalCurrencyText;
    public float scoreCountUpDuration = 0.8f;
    public float currencyCountUpDuration = 0.6f;

    [Header("Trava outros controles durante o Game Over")]
    public CanvasGroup weaponButtonsGroup; // arraste o CanvasGroup que envolve os botões de arma

    [Header("Menu Inicial")]
    public GameObject mainMenuPanel;
    public GameObject hudRoot; // o painel (ex: SafeArea) que contém score, combo, corações, botões de arma
    public TMP_Text mainMenuCurrencyText;

    [Header("Cores dos corações")]
    public Color heartOnColor = new Color(1f, 0.3f, 0.37f);
    public Color heartOffColor = new Color(0.17f, 0.18f, 0.22f);

    [Header("Contagem do score (tipo caixa registradora)")]
    public float scoreTickDuration = 0.35f;
    int displayedScore;
    Tween scoreTween;

    void Awake() => Instance = this;

    void Update()
    {
        UpdateComboColor();
    }

    // A cor do combo esfria (laranja -> vermelho) conforme o tempo pra expirar
    // acaba, dando um aviso visual antes dele sumir de vez.
    void UpdateComboColor()
    {
        if (ScoreManager.Instance == null || comboText == null) return;
        if (ScoreManager.Instance.Combo <= 1) return; // sem combo ativo pra "esquentar"

        float remaining = ScoreManager.Instance.ComboWindowRemaining01;
        float eased = remaining * remaining * remaining; // segura perto de 1 (quente) na maior parte da janela, só cai rápido no fim
        comboText.color = Color.Lerp(comboColorCold, comboColorHot, eased);
    }

    // Chamado no início/restart da run — sem isso, o score ficaria "contando
    // pra trás" do valor da run anterior até zero.
    public void ResetHud()
    {
        scoreTween?.Kill();
        displayedScore = 0;
        scoreText.text = "0";
        scoreText.color = scoreColorNormal;
        comboText.text = "combo x1";
        comboText.color = comboColorHot;
    }

    public void UpdateScore(int score, int combo)
    {
        scoreTween?.Kill();
        scoreTween = DOVirtual.Int(displayedScore, score, scoreTickDuration, v =>
        {
            displayedScore = v;
            scoreText.text = v.ToString();
        }).SetEase(Ease.OutCubic);

        scoreText.transform.DOKill();
        scoreText.transform.localScale = Vector3.one;
        scoreText.transform.DOPunchScale(Vector3.one * 0.22f, 0.22f, 6, 0.8f);

        float heat = Mathf.Clamp01((Mathf.Max(1, combo) - 1) / (float)comboForMaxScoreColor);
        scoreText.color = Color.Lerp(scoreColorNormal, scoreColorHot, heat);

        comboText.text = $"combo x{Mathf.Max(1, combo)}";
        comboText.color = comboColorHot;
        comboText.transform.DOKill();
        comboText.transform.localScale = Vector3.one;
        comboText.transform.DOPunchScale(Vector3.one * 0.35f, 0.28f, 8, 0.8f);
    }

    // Chamado pelo ScoreManager quando o combo expira sozinho (sem novo kill a tempo)
    public void NotifyComboExpired()
    {
        if (comboText == null) return;
        comboText.text = "combo x1";
        comboText.color = comboColorCold;
        comboText.transform.DOKill();
        comboText.transform.localScale = Vector3.one;
        Sequence seq = DOTween.Sequence();
        seq.Append(comboText.transform.DOPunchScale(Vector3.one * -0.25f, 0.3f, 4, 0.6f));
        seq.Join(comboText.DOFade(0.35f, 0.3f));
    }

    public void UpdateHearts(int hp, int maxHp)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < maxHp;
            hearts[i].color = i < hp ? heartOnColor : heartOffColor;

            var heartIcon = hearts[i].GetComponent<HeartIcon>();
            if (heartIcon != null) heartIcon.Punch(i * 0.06f);
        }
    }

    public void ShowGameOver(int finalScore, int currencyEarned, int totalCurrencyBefore, int totalCurrencyAfter)
    {
        gameOverPanel.SetActive(true);
        SetOtherControlsInteractable(false);

        finalScoreText.text = "score: 0";
        if (currencyEarnedText != null) currencyEarnedText.text = "";
        if (totalCurrencyText != null) totalCurrencyText.text = $"total: {totalCurrencyBefore}";

        Sequence seq = DOTween.Sequence();

        // 1) o score sobe contando até o valor final
        seq.Append(DOVirtual.Int(0, finalScore, scoreCountUpDuration,
            v => finalScoreText.text = $"score: {v}").SetEase(Ease.OutCubic));

        seq.AppendInterval(0.15f);

        // 2) aparece quanto isso virou de moeda
        seq.AppendCallback(() =>
        {
            if (currencyEarnedText == null) return;
            currencyEarnedText.text = $"+{currencyEarned} coins";
            currencyEarnedText.transform.DOKill();
            currencyEarnedText.transform.localScale = Vector3.one * 0.3f;
            currencyEarnedText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        });

        seq.AppendInterval(0.5f);

        // 3) a moeda ganha soma no total, contando a partir do valor antigo
        seq.Append(DOVirtual.Int(totalCurrencyBefore, totalCurrencyAfter, currencyCountUpDuration,
            v => { if (totalCurrencyText != null) totalCurrencyText.text = $"total: {v}"; }).SetEase(Ease.OutCubic));
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
        SetOtherControlsInteractable(true);
    }

    // Chamado uma vez ao abrir o jogo — antes da primeira run começar
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (hudRoot != null) hudRoot.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (mainMenuCurrencyText != null) mainMenuCurrencyText.text = $"coins: {MetaProgress.Currency}";
    }

    // Chamado quando o jogador aperta "Play"
    public void HideMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudRoot != null) hudRoot.SetActive(true);
    }

    void SetOtherControlsInteractable(bool value)
    {
        if (weaponButtonsGroup == null) return;
        weaponButtonsGroup.interactable = value;
        weaponButtonsGroup.blocksRaycasts = value;
    }
}
