using UnityEngine;

// Fonte de verdade do progresso permanente do jogador — sobrevive entre runs
// e entre sessões (PlayerPrefs). Não guarda nada específico da run atual.
public static class MetaProgress
{
    const string CurrencyKey = "cv_currency";
    const string LevelKeyPrefix = "cv_upgrade_lv_";

    public static int Currency
    {
        get => PlayerPrefs.GetInt(CurrencyKey, 0);
        private set => PlayerPrefs.SetInt(CurrencyKey, Mathf.Max(0, value));
    }

    public static void AddCurrency(int amount)
    {
        if (amount <= 0) return;
        Currency += amount;
        PlayerPrefs.Save();
    }

    public static int GetLevel(UpgradeDefinition def)
    {
        if (def == null) return 0;
        return PlayerPrefs.GetInt(LevelKeyPrefix + def.id, 0);
    }

    public static bool IsMaxed(UpgradeDefinition def)
    {
        return def != null && GetLevel(def) >= def.maxLevel;
    }

    public static int GetCost(UpgradeDefinition def)
    {
        if (def == null) return 0;
        int level = GetLevel(def);
        return Mathf.RoundToInt(def.baseCost * Mathf.Pow(def.costGrowth, level));
    }

    public static bool CanAfford(UpgradeDefinition def)
    {
        return !IsMaxed(def) && Currency >= GetCost(def);
    }

    public static bool TryPurchase(UpgradeDefinition def)
    {
        if (!CanAfford(def)) return false;
        Currency -= GetCost(def);
        PlayerPrefs.SetInt(LevelKeyPrefix + def.id, GetLevel(def) + 1);
        PlayerPrefs.Save();
        return true;
    }

    // Valor total já desbloqueado (nível atual × valor por nível)
    public static float GetTotalValue(UpgradeDefinition def)
    {
        if (def == null) return 0f;
        return GetLevel(def) * def.valuePerLevel;
    }
}
