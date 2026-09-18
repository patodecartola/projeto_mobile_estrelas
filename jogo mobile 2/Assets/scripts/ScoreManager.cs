using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Juice")]
    public GameObject impactParticlesPrefab;
    public float killShakeStrength = 0.06f;
    public float killShakeDuration = 0.08f;

    public int Score { get; private set; }
    public int Combo { get; private set; }

    [Header("Combo")]
    [Tooltip("Quanto tempo, em segundos, você tem pra encadear o próximo kill antes do combo cair")]
    public float comboWindow = 1.8f;

    // 1 = combo acabou de acontecer, 0 = prestes a expirar — pra UI colorir/animar
    public float ComboWindowRemaining01
    {
        get
        {
            if (Combo <= 1) return 0f;
            return Mathf.Clamp01(1f - (Time.time - lastKillTime) / comboWindow);
        }
    }

    float lastKillTime = -999f;

    void Awake() => Instance = this;

    void Update()
    {
        // Expira o combo sozinho quando o tempo acaba, mesmo sem outro kill
        // acontecer — assim dá pra animar o "combo perdido" na hora certa.
        if (Combo > 1 && Time.time - lastKillTime > comboWindow)
        {
            Combo = 0;
            UIManager.Instance.NotifyComboExpired();
        }
    }

    public void ResetScore()
    {
        Score = 0;
        Combo = 0;
        lastKillTime = -999f;
        UIManager.Instance.ResetHud();
    }

    // Chamado pelo Enemy.Kill() — a cor vem da arma que matou, útil se quiser
    // dar feedback visual diferente por arma futuramente (ex: partículas na UI)
    public void RegisterKill(Vector2 position, int baseValue, Color color)
    {
        Combo = (Time.time - lastKillTime < comboWindow) ? Combo + 1 : 1;
        lastKillTime = Time.time;
        Score += baseValue * Combo;
        UIManager.Instance.UpdateScore(Score, Combo);

        ImpactParticles.SpawnAt(impactParticlesPrefab, position, color);
        CameraShake.Instance?.Shake(killShakeDuration, killShakeStrength, 10);
    }
}
