using UnityEngine;

// Coloque num painel vazio dentro do Canvas, e arraste todo o HUD (corações,
// score, botões de arma, painel de game over) como filho desse painel — em vez
// de filhos diretos do Canvas. Sem isso, em aparelhos com notch/ilha dinâmica/
// cantos arredondados, elementos perto da borda podem ficar cortados ou atrás
// do sistema.
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rect;
    Rect lastSafeArea;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        // Recalcula se girar o aparelho ou (em editor) redimensionar o preview
        if (Screen.safeArea != lastSafeArea) Apply();
    }

    void Apply()
    {
        lastSafeArea = Screen.safeArea;

        Vector2 anchorMin = lastSafeArea.position;
        Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
