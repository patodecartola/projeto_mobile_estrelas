using UnityEngine;

// Sprite de brilho radial (branco, forte no centro, some na borda) — usado
// como o "halo" atrás do botão de arma selecionado. Gerado uma vez só e
// reaproveitado, sem depender de nenhuma imagem importada.
public static class UIGlowSprite
{
    static Sprite cached;

    public static Sprite Get()
    {
        if (cached != null) return cached;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float a = Mathf.Clamp01(1f - d);
                a *= a; // falloff mais suave, concentrado no centro
                pixels[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect);
        return cached;
    }
}
