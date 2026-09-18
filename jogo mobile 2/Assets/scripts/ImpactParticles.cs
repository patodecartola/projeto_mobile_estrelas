using UnityEngine;

// Explosão de partículas num ponto de impacto (acerto de arma, morte de inimigo).
// O "dot" usado nas partículas é gerado por código na primeira vez que roda —
// não depende de nenhuma imagem importada.
[RequireComponent(typeof(ParticleSystem))]
public class ImpactParticles : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystemRenderer psRenderer;
    static Sprite sharedDotSprite;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();

        EnsureDotSprite();
        if (psRenderer.sharedMaterial == null || psRenderer.sharedMaterial.mainTexture == null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = sharedDotSprite.texture;
            psRenderer.material = mat;
        }
    }

    static void EnsureDotSprite()
    {
        if (sharedDotSprite != null) return;

        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float alpha = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha)); // borda suave
            }
        }
        tex.Apply();
        sharedDotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void Play(Color color, int count = 14)
    {
        var main = ps.main;
        main.startColor = color;
        ps.Emit(count);
    }

    // Atalho estático: instancia o prefab, dispara e deixa o próprio
    // ParticleSystem se destruir sozinho (Stop Action = Destroy, configurado no prefab).
    public static void SpawnAt(GameObject prefab, Vector2 position, Color color, int count = 14)
    {
        if (prefab == null) return;
        var go = Instantiate(prefab, position, Quaternion.identity);
        go.GetComponent<ImpactParticles>().Play(color, count);
    }
}
