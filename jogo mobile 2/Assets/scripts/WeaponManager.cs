using UnityEngine;

public enum WeaponType { Divisor = 0, Ricochete = 1 }

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Prefabs das armas (arraste o GameObject do prefab, não o script)")]
    public GameObject divisorPrefab;
    public GameObject ricochetPrefab;

    // Fonte única de verdade — em vez de cada projétil carregar sua própria
    // referência (que se mostrou frágil nesse projeto), tudo consulta aqui.
    public GameObject DivisorPrefabAsset => divisorPrefab;
    public GameObject RicochetPrefabAsset => ricochetPrefab;

    [Header("Força do tiro (m/s)")]
    public float minSpeed = 6f;
    public float maxSpeed = 20f;

    [Header("Cooldown de tiro")]
    public float fireCooldown = 0.25f;
    float lastFireTime = -999f;

    public bool CanFire => Time.time - lastFireTime >= EffectiveCooldown;
    public float EffectiveCooldown => Mathf.Max(0.05f, fireCooldown - UpgradeSystem.CooldownReduction);
    // 0 = acabou de atirar, 1 = pronto pra atirar de novo — útil pra uma barra de UI futura
    public float CooldownProgress01 => Mathf.Clamp01((Time.time - lastFireTime) / EffectiveCooldown);

    [Header("Cor por arma (usada pelo fundo da arena)")]
    public Color divisorColor = new Color(1f, 0.62f, 0.35f);
    public Color ricochetColor = new Color(0.2f, 0.85f, 0.91f);
    public Color CurrentColor => Current == WeaponType.Divisor ? divisorColor : ricochetColor;

    [Header("Intensidade de brilho por arma (ajuste fino do fundo)")]
    [Tooltip("Cores diferentes 'parecem' mais fracas ou fortes mesmo com o mesmo alpha — use isso pra compensar.")]
    public float divisorGlowIntensity = 1.4f;
    public float ricochetGlowIntensity = 1f;
    public float CurrentGlowIntensity => Current == WeaponType.Divisor ? divisorGlowIntensity : ricochetGlowIntensity;

    public WeaponType Current { get; private set; } = WeaponType.Divisor;
    public event System.Action<WeaponType> OnWeaponChanged;

    void Awake()
    {
        Instance = this;
        if (divisorPrefab == null)
            Debug.LogWarning("[WeaponManager] divisorPrefab não atribuído!", this);
    }

    // Ligado nos botões de UI (OnClick -> SwitchWeapon(0) / SwitchWeapon(1))
    public void SwitchWeapon(int typeIndex)
    {
        Current = (WeaponType)typeIndex;
        OnWeaponChanged?.Invoke(Current);
    }

    // origin = posição do player; direction já normalizada; power de 0 a 1 (força do arrasto)
    public void Fire(Vector2 origin, Vector2 direction, float power)
    {
        if (!CanFire) return;
        lastFireTime = Time.time;

        float speed = Mathf.Lerp(minSpeed, maxSpeed, power);
        GameObject prefabToFire = Current == WeaponType.Divisor ? divisorPrefab : ricochetPrefab;

        var go = Instantiate(prefabToFire, origin, Quaternion.identity);

        if (Current == WeaponType.Divisor)
        {
            var d = go.GetComponent<DivisorProjectile>();
            if (d != null) d.childCount += UpgradeSystem.DivisorExtraChildren;
        }
        else
        {
            var r = go.GetComponent<RicochetProjectile>();
            if (r != null)
            {
                r.bouncesLeft += UpgradeSystem.RicochetExtraBounces;
                r.seekSpeed += UpgradeSystem.RicochetSeekSpeedBonus;
            }
        }

        var proj = go.GetComponent<Projectile>();
        proj.Launch(direction * speed);

        ArenaBackground.Instance?.Pulse(CurrentColor, 1.6f, 0.15f);
    }
}
