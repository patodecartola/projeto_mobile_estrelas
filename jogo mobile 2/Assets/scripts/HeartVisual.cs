using UnityEngine;
using UnityEngine.UI;

// Gera o sprite do coração por código (branco + contorno sutil) — o
// UIManager continua controlando a cor de "vivo"/"perdido" via Image.color,
// exatamente como já fazia antes. Só cuida da FORMA.
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class HeartVisual : MonoBehaviour
{
    [Range(0f, 0.3f)] public float outlineThickness = 0.08f;
    [Range(0f, 1f)] public float outlineDarkness = 0.35f; // 0 = sem contorno visível

    Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        Regenerate();
    }

    void OnValidate()
    {
        if (img == null) img = GetComponent<Image>();
        Regenerate();
    }

    void Regenerate()
    {
        if (img == null) return;
        img.sprite = ProceduralShapeSprite.GenerateHeart(
            Color.white, new Color(0f, 0f, 0f, outlineDarkness), outlineThickness);
    }
}
