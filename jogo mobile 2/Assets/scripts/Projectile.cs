using UnityEngine;

public enum ProjState { Flying, Burst, Recoil, Seeking }
public enum DamageSource { Divisor, Ricochete }

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Projectile : MonoBehaviour
{
    [Header("Config comum")]
    public float lifeTime = 2.6f;
    public float turnRateDegrees = 400f; // velocidade da curva no homing (estado "seek")
    public float stateDamping = 6f;      // desaceleração da velocidade durante burst/recoil

    protected Rigidbody2D rb;
    protected Vector2 velocity;
    protected Enemy target;
    protected ProjState state = ProjState.Flying;

    [Header("Orientação do sprite")]
    [Tooltip("Se o sprite foi desenhado apontando pra cima em vez de pra direita, use -90 aqui.")]
    public float spriteForwardOffsetDegrees = 0f;

    float lifeTimer;
    float stateTimer;
    System.Action onStateTimerEnd;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnEnable()
    {
        ProjectileRegistry.Register(this);
    }

    protected virtual void OnDisable()
    {
        ProjectileRegistry.Unregister(this);
    }

    protected virtual void Start()
    {
        lifeTimer = lifeTime;
    }

    // Usado pelo inimigo Explosivo ao morrer, pra empurrar fragmentos próximos
    // pra uma direção diferente da que estavam seguindo.
    public void ApplyImpulse(Vector2 impulse)
    {
        velocity += impulse;
    }

    // Tiro inicial do player
    public virtual void Launch(Vector2 initialVelocity)
    {
        state = ProjState.Flying;
        velocity = initialVelocity;
        FaceVelocity();
    }

    // Entra num estado temporário com velocidade própria (o "pop" do Divisor ou o
    // recuo do Ricochete). Ao terminar, chama onEnd — é ali que a arma escolhe o próximo alvo.
    // A posição sempre é resolvida aqui no FixedUpdate, nunca por uma tween externa —
    // isso evita qualquer disputa entre DOTween e o Rigidbody2D cinemático.
    protected void EnterTimedState(ProjState newState, Vector2 stateVelocity, float duration, System.Action onEnd)
    {
        state = newState;
        velocity = stateVelocity;
        stateTimer = duration;
        onStateTimerEnd = onEnd;
    }

    protected void StartSeeking(Enemy newTarget, Vector2 initialVelocity)
    {
        target = newTarget;
        velocity = initialVelocity;
        state = ProjState.Seeking;
    }

    // Gira o Rigidbody2D (não só o Transform) — funciona de forma confiável
    // independente do Body Type, e nunca é sobrescrito pela física.
    protected void FaceVelocity()
    {
        if (velocity.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle + spriteForwardOffsetDegrees);
    }

    protected virtual void FixedUpdate()
    {
        lifeTimer -= Time.fixedDeltaTime;
        if (lifeTimer <= 0f) { Despawn(); return; }

        if (state == ProjState.Burst || state == ProjState.Recoil)
        {
            stateTimer -= Time.fixedDeltaTime;
            velocity *= Mathf.Max(0f, 1f - stateDamping * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            if (stateTimer <= 0f)
            {
                var callback = onStateTimerEnd;
                onStateTimerEnd = null;
                callback?.Invoke();
            }
            return;
        }

        if (state == ProjState.Seeking && target != null)
        {
            if (!target.isAlive) target = EnemyRegistry.GetNearest(rb.position);
            if (target != null)
            {
                Vector2 desired = ((Vector2)target.transform.position - rb.position).normalized;
                float speed = velocity.magnitude;
                Vector2 newDir = Vector3.RotateTowards(
                    velocity.normalized, desired,
                    turnRateDegrees * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
                velocity = newDir.normalized * speed;
            }
        }

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        FaceVelocity();
    }

    protected void Despawn()
    {
        if (this != null) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Durante o "pop" do Divisor ou o recuo do Ricochete, o projétil fica imune
        // a colisão — igual na demo original, onde essa checagem era pulada nesses
        // estados. Sem isso, um fragmento que nasce encostado em outro inimigo
        // acaba se consumindo instantaneamente, antes de sequer começar a animação.
        if (state == ProjState.Burst || state == ProjState.Recoil) return;

        var enemy = other.GetComponent<Enemy>();
        if (enemy == null || !enemy.isAlive) return;

        OnImpact(enemy);
    }

    // Cada arma (Divisor, Ricochete, futuras...) implementa sua própria reação ao acerto
    protected abstract void OnImpact(Enemy enemy);
}
