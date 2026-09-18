using UnityEngine;
using DG.Tweening;

public enum EnemyType { Comum, Fujao, Rapido, Blindado }

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Config")]
    public EnemyType type = EnemyType.Comum;
    public float baseSpeed = 1.2f;
    public int scoreValue = 10;

    [Header("Vida (Blindado usa 2)")]
    public int maxHits = 1;

    [Header("Fujão — foge de projéteis próximos")]
    public float fleeDetectRadius = 2.5f;

    [HideInInspector] public float speed;
    [HideInInspector] public bool isAlive = true;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor;
    int hitsTaken;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        if (speed <= 0f) speed = baseSpeed;
    }

    void OnEnable()
    {
        EnemyRegistry.Register(this);
        isAlive = true;
        hitsTaken = 0;
    }

    void OnDisable()
    {
        EnemyRegistry.Unregister(this);
    }

    void FixedUpdate()
    {
        if (!isAlive || GameManager.Instance == null || GameManager.Instance.PlayerTransform == null) return;

        Vector2 moveDir;

        if (type == EnemyType.Fujao)
        {
            var threat = ProjectileRegistry.GetNearestWithin(rb.position, fleeDetectRadius);
            if (threat != null)
                moveDir = (rb.position - (Vector2)threat.transform.position).normalized; // foge do projétil
            else
                moveDir = ((Vector2)GameManager.Instance.PlayerTransform.position - rb.position).normalized;
        }
        else
        {
            moveDir = ((Vector2)GameManager.Instance.PlayerTransform.position - rb.position).normalized;
        }

        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
    }

    // Chamado pela arma que acertou. Se ainda não atingiu maxHits, só dá feedback
    // visual (flash) e sobrevive — é assim que o Blindado aguenta 2 hits.
    public void Kill(DamageSource source, Color hitColor)
    {
        if (!isAlive) return;

        hitsTaken++;
        if (hitsTaken < maxHits)
        {
            FlashHit();
            return;
        }

        isAlive = false;
        ScoreManager.Instance.RegisterKill(transform.position, scoreValue, hitColor);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        sr?.DOKill();
        transform.DOKill();
    }

    void FlashHit()
    {
        if (sr == null) return;
        sr.DOKill();
        sr.color = Color.white;
        sr.DOColor(baseColor, 0.15f);
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 8, 0.9f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;
        if (other.CompareTag("Player"))
        {
            isAlive = false;
            GameManager.Instance.HitPlayer();
            Destroy(gameObject);
        }
    }
}
