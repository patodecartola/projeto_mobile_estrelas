using UnityEngine;

// Valores já calculados a partir do progresso salvo — os sistemas de jogo
// (WeaponManager, DivisorProjectile, GameManager...) só leem daqui, nunca
// falam diretamente com MetaProgress. Repopulado pelo UpgradeManager.
public static class UpgradeSystem
{
    public static int DivisorExtraChildren;
    public static float DivisorFragmentSpeedBonus;
    public static int RicochetExtraBounces;
    public static float RicochetSeekSpeedBonus;
    public static float CooldownReduction;
    public static int MaxHpBonus;
}
