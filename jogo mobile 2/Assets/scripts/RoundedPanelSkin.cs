using UnityEngine;
using UnityEngine.UI;

// Adiciona no mesmo objeto que tem um componente Image (botão, painel, etc).
// Gera um retângulo arredondado com borda colorida por código — sem precisar
// de nenhuma imagem importada. O sprite é 9-sliced automaticamente, então
// funciona bem em qualquer tamanho de botão sem esticar os cantos.
[RequireComponent(typeof(Image))]
public class RoundedPanelSkin : MonoBehaviour
{
    [Header("Forma")]
    public int cornerRadius = 18;
    public int borderThickness = 3;
    public int textureSize = 128;

    [Header("Cores")]
    public Color fillColor = new Color(0.08f, 0.09f, 0.11f, 0.92f);
    public Color borderColor = new Color(1f, 0.62f, 0.35f); // laranja — troque pra ciano nos elementos do Ricochete

    Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        Apply(fillColor, borderColor);
    }

    // Chame isso em runtime pra trocar as cores (ex: botão da loja mudando
    // de "comprável" pra "sem moeda/MAX"). Regenera o sprite — não é caro
    // pra poucos botões, mas evite chamar isso todo frame.
    public void Apply(Color fill, Color border)
    {
        if (img == null) img = GetComponent<Image>();
        fillColor = fill;
        borderColor = border;
        img.sprite = Generate();
        img.type = Image.Type.Sliced;
    }

    Sprite Generate()
    {
        int size = textureSize;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        Vector2 halfSize = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - center;
                float d = RoundedBoxSDF(p, halfSize, cornerRadius);

                Color c;
                if (d > 0.5f) c = new Color(0f, 0f, 0f, 0f);          // fora do formato
                else if (d > -borderThickness) c = borderColor;        // faixa da borda
                else c = fillColor;                                    // preenchimento
                pixels[y * size + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        float b = Mathf.Min(size / 2f - 1, cornerRadius + borderThickness + 4);
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
    }

    // SDF de retângulo arredondado (Inigo Quilez) — negativo = dentro, 0 = na borda, positivo = fora
    static float RoundedBoxSDF(Vector2 p, Vector2 halfSize, float radius)
    {
        Vector2 d = new Vector2(Mathf.Abs(p.x) - halfSize.x + radius, Mathf.Abs(p.y) - halfSize.y + radius);
        float outside = new Vector2(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f)).magnitude;
        float inside = Mathf.Min(Mathf.Max(d.x, d.y), 0f);
        return outside + inside - radius;
    }
}
