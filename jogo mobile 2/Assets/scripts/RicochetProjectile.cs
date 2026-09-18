using UnityEngine;
using DG.Tweening;

public class RicochetProjectile : Projectile
{
    [Header("Ricochete")]
    public int bouncesLeft = 3;
    public float recoilDuration = 0.23f;
    public float recoilDistance = 1.1f;
    public float spinDegrees = 720f; // giro rápido tipo adaga
    public float seekSpeed = 11f;
    public Color weaponColor = new Color(0.2f, 0.85f, 0.91f); // ciano
    public GameObject impactRingPrefab;

    protected override void OnImpact(Enemy enemy)
    {
        Vector2 impactPoint = enemy.transform.position;
        enemy.Kill(DamageSource.Ricochete, weaponColor);
        SpawnRing(impactPoint);

        bouncesLeft--;
        if (bouncesLeft <= 0) { Despawn(); return; }

        var nextTarget = EnemyRegistry.GetNearest(rb.position);
        if (nextTarget == null) { Despawn(); return; }

        // Visual: giro rápido tipo adaga — pura cosmética de rotação
        transform.DORotate(new Vector3(0f, 0f, spinDegrees), recoilDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad);

        // Física: o recuo é uma velocidade real que desacelera, resolvida no FixedUpdate
        float currentAngle = rb.rotation * Mathf.Deg2Rad;
        Vector2 facingDir = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
        Vector2 recoilDir = -facingDir;
        Vector2 recoilVel = recoilDir * (recoilDistance / recoilDuration);

        EnterTimedState(ProjState.Recoil, recoilVel, recoilDuration, () =>
        {
            if (this == null) return;
            rb.MoveRotation(0f);
            var finalTarget = nextTarget.isAlive ? nextTarget : EnemyRegistry.GetNearest(rb.position);
            if (finalTarget == null) { Despawn(); return; }

            Vector2 dir = ((Vector2)finalTarget.transform.position - rb.position).normalized;
            StartSeeking(finalTarget, dir * seekSpeed);
        });
    }

    void SpawnRing(Vector2 point)
    {
        if (impactRingPrefab == null) return;
        Instantiate(impactRingPrefab, point, Quaternion.identity)
            .GetComponent<ImpactRing>().Play(weaponColor);
    }
}
