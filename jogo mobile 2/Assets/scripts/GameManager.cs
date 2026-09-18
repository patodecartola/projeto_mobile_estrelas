using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public Transform PlayerTransform;
    public PlayerAim playerAim;
    public EnemySpawner enemySpawner;

    [Header("Vida")]
    public int baseMaxHp = 3;
    public int EffectiveMaxHp => baseMaxHp + UpgradeSystem.MaxHpBonus;
    int hp;

    [Header("Economia")]
    [Tooltip("Quantas moedas cada ponto de score vale ao final da run")]
    public float currencyPerPoint = 0.1f;

    public bool IsRunning { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // A run não começa sozinha mais — fica esperando o jogador
        // apertar Play no menu inicial.
        if (playerAim != null) playerAim.enabled = false;
        UIManager.Instance.ShowMainMenu();
    }

    // Ligado no botão "Play" do menu inicial
    public void PlayButtonPressed()
    {
        UIManager.Instance.HideMainMenu();
        Restart(); // reaproveita a limpeza defensiva da arena + StartRun()
    }

    // Ligado no botão "Restart" do painel de Game Over
    public void Restart()
    {
        EnemyRegistry.DestroyAll();
        ProjectileRegistry.DestroyAll();
        if (enemySpawner != null) enemySpawner.ResetSpawner();
        StartRun();
    }

    public void StartRun()
    {
        hp = EffectiveMaxHp;
        IsRunning = true;
        if (playerAim != null) playerAim.enabled = true;
        ScoreManager.Instance.ResetScore();
        UIManager.Instance.UpdateHearts(hp, EffectiveMaxHp);
        UIManager.Instance.HideGameOver();
    }

    public void HitPlayer()
    {
        if (!IsRunning) return;
        hp--;
        UIManager.Instance.UpdateHearts(hp, EffectiveMaxHp);
        CameraShake.Instance?.Shake(0.22f, 0.35f, 20);
        ArenaBackground.Instance?.Pulse(Color.red, 2.5f, 0.35f);
        if (hp <= 0) EndRun();
    }

    void EndRun()
    {
        IsRunning = false;
        if (playerAim != null) playerAim.enabled = false;

        int score = ScoreManager.Instance.Score;
        int earned = Mathf.RoundToInt(score * currencyPerPoint);
        int totalBefore = MetaProgress.Currency;
        MetaProgress.AddCurrency(earned);
        int totalAfter = MetaProgress.Currency;

        UIManager.Instance.ShowGameOver(score, earned, totalBefore, totalAfter);
    }
}
