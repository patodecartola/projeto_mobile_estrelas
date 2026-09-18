using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs (Comum, Fujão, Explosivo, Blindado...)")]
    public GameObject[] enemyPrefabs;

    [Header("Ritmo de spawn")]
    public float startInterval = 1.4f;
    public float minInterval = 0.5f;
    public float rampPerSecond = 0.008f;

    [Header("Limite")]
    [Tooltip("Não nasce inimigo novo enquanto esse número estiver vivo — evita ser cercado.")]
    public int maxConcurrentEnemies = 12;

    [Header("Área da arena")]
    public Camera arenaCamera;
    public float edgeMargin = 1.6f;

    float timer;
    float elapsed;

    // Chamado pelo GameManager ao reiniciar — volta o ritmo pro início da run.
    public void ResetSpawner()
    {
        timer = 0f;
        elapsed = 0f;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning) return;

        elapsed += Time.deltaTime;
        timer += Time.deltaTime;
        float interval = Mathf.Max(minInterval, startInterval - elapsed * rampPerSecond);

        if (timer >= interval && EnemyRegistry.Count < maxConcurrentEnemies)
        {
            timer = 0f;
            SpawnOne();
        }
        else if (timer >= interval)
        {
            timer = interval; // segura no limite, spawna assim que abrir vaga
        }
    }

    void SpawnOne()
    {
        if (enemyPrefabs.Length == 0) return;
        Vector2 pos = RandomEdgePosition();
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        var go = Instantiate(prefab, pos, Quaternion.identity);
        var enemy = go.GetComponent<Enemy>();
        enemy.speed = enemy.baseSpeed + Random.Range(0f, 0.6f) + Mathf.Min(elapsed * 0.02f, 1.2f);
    }

    Vector2 RandomEdgePosition()
    {
        float h = arenaCamera.orthographicSize;
        float w = h * arenaCamera.aspect;
        Vector2 center = arenaCamera.transform.position;
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return new Vector2(center.x - w - edgeMargin, center.y + Random.Range(-h, h));
            case 1: return new Vector2(center.x + w + edgeMargin, center.y + Random.Range(-h, h));
            case 2: return new Vector2(center.x + Random.Range(-w, w), center.y + h + edgeMargin);
            default: return new Vector2(center.x + Random.Range(-w, w), center.y - h - edgeMargin);
        }
    }
}
