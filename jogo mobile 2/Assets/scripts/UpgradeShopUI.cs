using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gera a lista de upgrades dentro de 'content' (ex: o Content de um Scroll View).
// Cada entrada usa 'upgradeButtonPrefab': um Button com pelo menos 2 TMP_Text
// filhos (o primeiro = nome/nível, o segundo = custo) e um RoundedPanelSkin.
public class UpgradeShopUI : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    public Transform content;
    public GameObject upgradeButtonPrefab;
    public TMP_Text currencyText;

    [Header("Cores por estado do botão")]
    public Color buyableFill = new Color32(0x14, 0x17, 0x1C, 0xEB);
    public Color buyableBorder = new Color32(0xFF, 0x9E, 0x59, 0xFF);
    public Color lockedFill = new Color32(0x0F, 0x0F, 0x12, 0xB3);
    public Color lockedBorder = new Color32(0x26, 0x26, 0x2B, 0xFF);

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        if (currencyText != null)
            currencyText.text = $"coins: {MetaProgress.Currency}";

        foreach (var def in upgradeManager.All)
        {
            if (def == null) continue;
            BuildEntry(def);
        }
    }

    void BuildEntry(UpgradeDefinition def)
    {
        var go = Instantiate(upgradeButtonPrefab, content);
        var texts = go.GetComponentsInChildren<TMP_Text>();
        int level = MetaProgress.GetLevel(def);
        bool maxed = MetaProgress.IsMaxed(def);
        bool canBuy = !maxed && MetaProgress.CanAfford(def);

        if (texts.Length > 0)
            texts[0].text = $"{def.displayName} (lv {level}/{def.maxLevel})";
        if (texts.Length > 1)
            texts[1].text = maxed ? "MAX" : $"{MetaProgress.GetCost(def)} coins";

        var skin = go.GetComponentInChildren<RoundedPanelSkin>();
        if (skin != null)
            skin.Apply(canBuy ? buyableFill : lockedFill, canBuy ? buyableBorder : lockedBorder);

        var btn = go.GetComponentInChildren<Button>();
        if (btn == null) return;
        btn.interactable = canBuy;
        btn.onClick.AddListener(() =>
        {
            if (MetaProgress.TryPurchase(def))
            {
                upgradeManager.Recalculate();
                Refresh();
            }
        });
    }
}

