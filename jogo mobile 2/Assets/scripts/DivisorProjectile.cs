using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DivisorProjectile : Projectile
{
    [Header("Divisor")]
    public int depth = 0;
    public int maxDepth = 1;
    public int childCount = 2;
    public float burstSpeed = 3f;
    public float burstDuration = 0.17f;
    public Color weaponColor = new Color(1f, 0.48f, 0.24f); // laranja
    public GameObject impactRingPrefab;

    protected override void OnImpact(Enemy enemy)
    {
        Vector2 impactPoint = enemy.transform.position;
        enemy.Kill(DamageSource.Divisor, weaponColor);
        SpawnRing(impactPoint);

        Despawn(); // o projétil "mãe" some — quem continua são os fragmentos

        if (depth >= maxDepth) return;

        var used = new HashSet<Enemy>();
        for (int i = 0; i < childCount; i++)
        {
            var t = EnemyRegistry.GetNearestExcluding(impactPoint, used);
            if (t == null) break;
            used.Add(t);
            SpawnFragment(impactPoint, t);
        }
    }

    void SpawnRing(Vector2 point)
    {
        if (impactRingPrefab == null) return;
        Instantiate(impactRingPrefab, point, Quaternion.identity)
            .GetComponent<ImpactRing>().Play(weaponColor);
    }

    void SpawnFragment(Vector2 origin, Enemy fragTarget)
    {
        // Fonte única de verdade: o mesmo GameObject de prefab que o WeaponManager usa pra atirar.
        var sourceGO = WeaponManager.Instance != null ? WeaponManager.Instance.DivisorPrefabAsset : null;
        if (sourceGO == null) return;

        var fragGO = Instantiate(sourceGO, origin, Quaternion.identity);
        var frag = fragGO.GetComponent<DivisorProjectile>();
        frag.depth = depth + 1;
        frag.transform.localScale = transform.localScale * 0.5f; // metade do tamanho de quem gerou

        // Visual: cresce enquanto "explode" pra fora (só escala — não mexe em posição)
        frag.transform.DOScale(1f, burstDuration).SetEase(Ease.OutBack);

        // Física: quem resolve a posição do "pop" é o próprio FixedUpdate do Projectile,
        // com velocidade real que desacelera — por isso nunca fica travado.
        float burstAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 burstVel = new Vector2(Mathf.Cos(burstAngle), Mathf.Sin(burstAngle)) * burstSpeed;

        frag.EnterTimedState(ProjState.Burst, burstVel, burstDuration, () =>
        {
            if (frag == null) return;
            var finalTarget = (fragTarget != null && fragTarget.isAlive)
                ? fragTarget
                : EnemyRegistry.GetNearest(frag.rb.position);
            if (finalTarget == null) { frag.Despawn(); return; }

            Vector2 dir = ((Vector2)finalTarget.transform.position - frag.rb.position).normalized;
            float speed = Random.Range(9f, 11f) + UpgradeSystem.DivisorFragmentSpeedBonus;
            frag.StartSeeking(finalTarget, dir * speed);
        });
    }
}
