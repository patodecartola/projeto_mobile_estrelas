using System.Collections.Generic;
using UnityEngine;

// Lista estática de inimigos vivos — usada pelas armas para achar o alvo mais próximo,
// equivalente ao nearestEnemy() da demo em JS. Não precisa ser adicionado a nenhum
// GameObject: é uma classe estática pura, os próprios Enemy se registram/removem.
public static class EnemyRegistry
{
    static readonly List<Enemy> alive = new List<Enemy>();

    public static int Count => alive.Count;

    public static void Register(Enemy e)
    {
        if (!alive.Contains(e)) alive.Add(e);
    }

    public static void Unregister(Enemy e)
    {
        alive.Remove(e);
    }

    // Usado no restart — destrói tudo que estiver vivo na arena.
    public static void DestroyAll()
    {
        var copy = new List<Enemy>(alive);
        foreach (var e in copy)
            if (e != null) Object.Destroy(e.gameObject);
    }

    public static Enemy GetNearest(Vector2 fromPosition, Enemy exclude = null)
    {
        Enemy best = null;
        float bestDist = float.MaxValue;
        foreach (var e in alive)
        {
            if (e == null || e == exclude || !e.isAlive) continue;
            float d = ((Vector2)e.transform.position - fromPosition).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = e; }
        }
        return best;
    }

    public static Enemy GetNearestExcluding(Vector2 fromPosition, HashSet<Enemy> exclude)
    {
        Enemy best = null;
        float bestDist = float.MaxValue;
        foreach (var e in alive)
        {
            if (e == null || !e.isAlive || (exclude != null && exclude.Contains(e))) continue;
            float d = ((Vector2)e.transform.position - fromPosition).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = e; }
        }
        return best;
    }
}
