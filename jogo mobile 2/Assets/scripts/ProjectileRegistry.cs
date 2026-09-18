using System.Collections.Generic;
using UnityEngine;

// Lista estática de projéteis/fragmentos ativos em cena — equivalente ao
// EnemyRegistry, só que do lado dos projéteis. Usada por comportamentos de
// inimigo que precisam "sentir" projéteis por perto (Fujão foge, Explosivo empurra).
public static class ProjectileRegistry
{
    static readonly List<Projectile> active = new List<Projectile>();

    public static void Register(Projectile p)
    {
        if (!active.Contains(p)) active.Add(p);
    }

    public static void Unregister(Projectile p)
    {
        active.Remove(p);
    }

    // Usado no restart — apaga todo projétil/fragmento ainda voando.
    public static void DestroyAll()
    {
        var copy = new List<Projectile>(active);
        foreach (var p in copy)
            if (p != null) Object.Destroy(p.gameObject);
    }

    public static Projectile GetNearestWithin(Vector2 position, float radius)
    {
        Projectile best = null;
        float bestDistSq = radius * radius;
        foreach (var p in active)
        {
            if (p == null) continue;
            float d = ((Vector2)p.transform.position - position).sqrMagnitude;
            if (d <= bestDistSq) { bestDistSq = d; best = p; }
        }
        return best;
    }

    // Usado pelo inimigo Explosivo ao morrer: empurra todo projétil/fragmento
    // dentro do raio pra uma direção diferente da que estava seguindo.
    public static void PushNearby(Vector2 center, float radius, float force)
    {
        foreach (var p in active)
        {
            if (p == null) continue;
            Vector2 offset = (Vector2)p.transform.position - center;
            float dist = offset.magnitude;
            if (dist <= radius && dist > 0.0001f)
                p.ApplyImpulse((offset / dist) * force);
        }
    }
}
