using UnityEngine;

// Fundo da arena: só estrelas (duas camadas, com deriva própria pra sensação de
// profundidade), tingidas com a cor da arma equipada, e um sistema de "pulso"
// genérico que qualquer evento do jogo pode disparar (dano, tiro, etc).
// Tudo gerado por código, sem depender de arte pronta.
public class ArenaBackground : MonoBehaviour
{
    public static ArenaBackground Instance { get; private set; }

    [Header("Referência")]
    public Camera arenaCamera;

    [Header("Cor")]
    public float colorLerpSpeed = 3f;

    [Header("Estrelas — camada distante")]
    public int starsFarCount = 40;
    public float starsFarSize = 0.03f;
    public float starsFarAlpha = 0.45f;
    public Vector2 starsFarDrift = new Vector2(0.01f, 0.004f);

    [Header("Estrelas — camada próxima")]
    public int starsNearCount = 18;
    public float starsNearSize = 0.05f;
    public float starsNearAlpha = 0.8f;
    public Vector2 starsNearDrift = new Vector2(0.025f, 0.01f);

    SpriteRenderer starsFarSr, starsNearSr;
    Material starsFarMat, starsNearMat;

    Color baseTint = Color.white;
    Color flashColor;
    float flashIntensity;
    float flashTimer;
    float flashDuration;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (arenaCamera == null) arenaCamera = Camera.main;
        Build();
    }

    void Update()
    {
        Color weaponColor = WeaponManager.Instance != null ? WeaponManager.Instance.CurrentColor : Color.white;
        baseTint = Color.Lerp(baseTint, weaponColor, colorLerpSpeed * Time.deltaTime);

        Color tint = baseTint;
        float intensity = 1f;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(flashTimer / flashDuration); // 1 -> 0
            tint = Color.Lerp(baseTint, flashColor, t);
            intensity = Mathf.Lerp(1f, flashIntensity, t);
        }

        ApplyStarColor(starsFarSr, tint, starsFarAlpha, intensity);
        ApplyStarColor(starsNearSr, tint, starsNearAlpha, intensity);

        if (starsFarMat != null) starsFarMat.mainTextureOffset += starsFarDrift * Time.deltaTime;
        if (starsNearMat != null) starsNearMat.mainTextureOffset += starsNearDrift * Time.deltaTime;
    }

    void ApplyStarColor(SpriteRenderer sr, Color tint, float baseAlpha, float intensity)
    {
        if (sr == null) return;
        Color c = tint * intensity; // multiplica o brilho — pode passar de 1 e estourar no Bloom, se ativado
        c.a = baseAlpha;
        sr.color = c;
    }

    // API genérica de pulso — qualquer sistema do jogo pode chamar isso.
    // Ex de dano: Pulse(Color.red, 2.5f, 0.35f)
    // Ex de tiro: Pulse(WeaponManager.Instance.CurrentColor, 1.6f, 0.15f)
    public void Pulse(Color color, float intensity, float duration)
    {
        flashColor = color;
        flashIntensity = intensity;
        flashDuration = duration;
        flashTimer = duration;
    }

    void Build()
    {
        float worldH = arenaCamera.orthographicSize * 2.4f; // margem extra pra não ver borda
        float worldW = worldH * arenaCamera.aspect;
        Vector2 sizeCovering = new Vector2(worldW, worldH);

        starsFarSr = NewLayer("StarsFar", 0);
        starsFarSr.sprite = StarSprite(starsFarCount, starsFarSize);
        starsFarSr.drawMode = SpriteDrawMode.Tiled;
        starsFarSr.size = sizeCovering;
        starsFarMat = starsFarSr.material;

        starsNearSr = NewLayer("StarsNear", 1);
        starsNearSr.sprite = StarSprite(starsNearCount, starsNearSize);
        starsNearSr.drawMode = SpriteDrawMode.Tiled;
        starsNearSr.size = sizeCovering;
        starsNearMat = starsNearSr.material;

        transform.position = new Vector3(arenaCamera.transform.position.x, arenaCamera.transform.position.y, transform.position.z);
    }

    SpriteRenderer NewLayer(string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = order;
        return sr;
    }

    // Gera um "tile" com pontinhos espalhados aleatoriamente, com wrap perfeito
    // nas bordas (pra repetir sem costura visível em modo Tiled).
    Sprite StarSprite(int count, float dotWorldSize)
    {
        const int size = 128;
        const float worldTileSize = 3f; // quantas unidades de mundo cada tile cobre
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(1f, 1f, 1f, 0f);

        int dotPixelRadius = Mathf.Max(1, Mathf.RoundToInt(dotWorldSize * size / worldTileSize / 2f));
        for (int i = 0; i < count; i++)
        {
            int cx = Random.Range(0, size);
            int cy = Random.Range(0, size);
            float brightness = Random.Range(0.4f, 1f);
            for (int y = -dotPixelRadius; y <= dotPixelRadius; y++)
            {
                for (int x = -dotPixelRadius; x <= dotPixelRadius; x++)
                {
                    int px = (cx + x + size) % size;
                    int py = (cy + y + size) % size; // wrap pra tile perfeito
                    float d = Mathf.Sqrt(x * x + y * y) / dotPixelRadius;
                    if (d <= 1f)
                    {
                        float a = (1f - d) * brightness;
                        int idx = py * size + px;
                        if (a > pixels[idx].a) pixels[idx] = new Color(1f, 1f, 1f, a);
                    }
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / worldTileSize,
            0, SpriteMeshType.FullRect);
    }
}
