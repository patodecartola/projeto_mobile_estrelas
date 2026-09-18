using UnityEngine;

// Gera um sprite de círculo, polígono ou estrela (contorno + preenchimento),
// tudo por código. sides=0 vira círculo; sides>=3 vira polígono regular;
// innerRadiusRatio < 1 transforma o polígono numa estrela (pontas alternando
// raio externo/interno).
public static class ProceduralShapeSprite
{
    public static Sprite Generate(int sides, float innerRadiusRatio, float rotationOffsetDeg,
        Color fillColor, Color outlineColor, float outlineThickness01, int textureSize = 128)
    {
        var poly = BuildPoints(sides, innerRadiusRatio, rotationOffsetDeg);
        return Rasterize(poly, fillColor, outlineColor, outlineThickness01, textureSize);
    }

    // Coração de verdade, via curva paramétrica (não é um polígono regular).
    public static Sprite GenerateHeart(Color fillColor, Color outlineColor, float outlineThickness01,
        int textureSize = 128, int segments = 60)
    {
        var poly = BuildHeartPoints(segments);
        return Rasterize(poly, fillColor, outlineColor, outlineThickness01, textureSize);
    }

    static Sprite Rasterize(Vector2[] poly, Color fillColor, Color outlineColor, float outlineThickness01, int textureSize)
    {
        var tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        var pixels = new Color[textureSize * textureSize];
        float half = textureSize / 2f;
        float margin = 6f; // folga pra o contorno não cortar na borda da textura
        float scale = half - margin;
        float outlinePx = outlineThickness01 * scale;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 p = (new Vector2(x + 0.5f, y + 0.5f) - new Vector2(half, half)) / scale;
                bool inside = PointInPolygon(p, poly);
                float distPx = DistToPolygonEdge(p, poly) * scale;

                Color c;
                if (!inside && distPx > outlinePx) c = new Color(0f, 0f, 0f, 0f);
                else if (distPx <= outlinePx) c = outlineColor;
                else c = fillColor;
                pixels[y * textureSize + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        // pixelsPerUnit = textureSize -> a forma ocupa 1 unidade de mundo (ou 1 "unidade" de
        // Image UI) por padrão; escala o Transform/RectTransform pra ajustar o tamanho final.
        return Sprite.Create(tex, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f),
            textureSize, 0, SpriteMeshType.FullRect);
    }

    static Vector2[] BuildPoints(int sides, float innerRatio, float rotationDeg)
    {
        int n = sides <= 0 ? 32 : sides; // círculo = polígono de 32 lados
        bool star = sides >= 3 && innerRatio < 0.999f;
        int count = star ? n * 2 : n;
        var pts = new Vector2[count];
        float rotRad = rotationDeg * Mathf.Deg2Rad;

        for (int i = 0; i < count; i++)
        {
            float angle = rotRad + i * Mathf.PI * 2f / count;
            float r = star ? (i % 2 == 0 ? 1f : innerRatio) : 1f;
            pts[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        }
        return pts;
    }

    // Fórmula clássica de curva de coração, normalizada pra caber num raio ~1.
    static Vector2[] BuildHeartPoints(int segments)
    {
        var pts = new Vector2[segments];
        const float maxR = 17f; // normalização aproximada do alcance da curva
        for (int i = 0; i < segments; i++)
        {
            float t = i * Mathf.PI * 2f / segments;
            float x = 16f * Mathf.Pow(Mathf.Sin(t), 3f);
            float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);
            pts[i] = new Vector2(x, y) / maxR;
        }
        return pts;
    }

    static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                inside = !inside;
        }
        return inside;
    }

    static float DistToPolygonEdge(Vector2 p, Vector2[] poly)
    {
        float min = float.MaxValue;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            min = Mathf.Min(min, DistToSegment(p, poly[j], poly[i]));
        return min;
    }

    static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f));
        Vector2 proj = a + ab * t;
        return Vector2.Distance(p, proj);
    }
}
