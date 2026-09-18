using UnityEngine;

// Cada upgrade é um asset (Create > Corrente Viva > Upgrade Definition).
// O efeito em si (o que o número faz no jogo) é interpretado pelo UpgradeManager,
// baseado no campo effectType — o ScriptableObject só guarda o dado.
[CreateAssetMenu(fileName = "Upgrade", menuName = "Corrente Viva/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    [Tooltip("Identificador único e estável — usado na chave de save. Não mude depois de já ter builds publicados.")]
    public string id;

    public string displayName;
    [TextArea] public string description;

    [Header("Progressão")]
    public int maxLevel = 3;
    public int baseCost = 50;
    [Tooltip("Multiplicador de custo a cada nível (1.5 = +50% de custo por nível)")]
    public float costGrowth = 1.5f;

    [Header("Efeito")]
    public UpgradeEffectType effectType;
    [Tooltip("Quanto cada nível soma no valor final (interpretado de acordo com o effectType)")]
    public float valuePerLevel = 1f;
}

public enum UpgradeEffectType
{
    DivisorChildCount,     // +N fragmentos por divisão (inteiro)
    DivisorFragmentSpeed,  // +N na velocidade de perseguição dos fragmentos
    RicochetBounces,       // +N quiques por tiro (inteiro)
    RicochetSeekSpeed,     // +N na velocidade de perseguição do ricochete
    FireCooldownReduction, // -N segundos de cooldown de tiro
    MaxHp                  // +N de vida máxima (inteiro)
}
