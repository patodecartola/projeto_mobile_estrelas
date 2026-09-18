using System.Collections.Generic;
using UnityEngine;

// Arraste os 6 UpgradeDefinition aqui. Recalcula UpgradeSystem no início do
// jogo e sempre que uma compra é feita — é a ponte entre o progresso salvo
// (MetaProgress) e os valores que o gameplay realmente usa (UpgradeSystem).
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Definições")]
    public UpgradeDefinition divisorChildCount;
    public UpgradeDefinition divisorFragmentSpeed;
    public UpgradeDefinition ricochetBounces;
    public UpgradeDefinition ricochetSeekSpeed;
    public UpgradeDefinition fireCooldown;
    public UpgradeDefinition maxHp;

    public IEnumerable<UpgradeDefinition> All
    {
        get
        {
            yield return divisorChildCount;
            yield return divisorFragmentSpeed;
            yield return ricochetBounces;
            yield return ricochetSeekSpeed;
            yield return fireCooldown;
            yield return maxHp;
        }
    }

    void Awake()
    {
        Instance = this;
        Recalculate();
    }

    public void Recalculate()
    {
        UpgradeSystem.DivisorExtraChildren = Mathf.RoundToInt(MetaProgress.GetTotalValue(divisorChildCount));
        UpgradeSystem.DivisorFragmentSpeedBonus = MetaProgress.GetTotalValue(divisorFragmentSpeed);
        UpgradeSystem.RicochetExtraBounces = Mathf.RoundToInt(MetaProgress.GetTotalValue(ricochetBounces));
        UpgradeSystem.RicochetSeekSpeedBonus = MetaProgress.GetTotalValue(ricochetSeekSpeed);
        UpgradeSystem.CooldownReduction = MetaProgress.GetTotalValue(fireCooldown);
        UpgradeSystem.MaxHpBonus = Mathf.RoundToInt(MetaProgress.GetTotalValue(maxHp));
    }
}
