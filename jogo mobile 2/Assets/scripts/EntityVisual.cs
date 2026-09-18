using UnityEngine;
using DG.Tweening;

// Gera a aparência do objeto por código (círculo/polígono/estrela com contorno)
// — sem precisar de nenhuma arte desenhada. Ajusta os parâmetros aqui no
// Inspector até achar um visual bom; some com um Light2D no mesmo objeto pra
// ele brilhar igual o resto da UI/projéteis do jogo.
//
// [ExecuteAlways] + OnValidate fazem a forma atualizar sozinha no Editor
// conforme você mexe nos campos — sem precisar dar Play pra ver o resultado.
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class EntityVisual : MonoBehaviour
{
    [Header("Forma")]
    [Tooltip("0 = círculo. 3+ = polígono/estrela com esse número de pontas.")]
    public int sides = 0;
    [Range(0.1f, 1f)] public float innerRadiusRatio = 1f; // <1 vira estrela
    public float rotationOffsetDeg = 0f;

    [Header("Cor")]
    public Color fillColor = new Color(0.1f, 0.02f, 0.03f, 0.95f);
    public Color outlineColor = new Color(1f, 0.3f, 0.37f);
    [Range(0.02f, 0.4f)] public float outlineThickness = 0.12f;

    [Header("Animação idle (só roda em Play)")]
    public bool idleRotate = false;
    public float idleRotateSpeed = 20f; // graus por segundo
    public bool idlePulse = true;
    public float idlePulseAmount = 0.06f;
    public float idlePulseDuration = 1f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Regenerate();
    }

    // Chamado automaticamente pelo Unity toda vez que um campo muda no Inspector
    void OnValidate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        Regenerate();
    }

    void Regenerate()
    {
        if (sr == null) return;
        sr.sprite = ProceduralShapeSprite.Generate(
            sides, innerRadiusRatio, rotationOffsetDeg,
            fillColor, outlineColor, outlineThickness, 128);
    }

    void Start()
    {
        if (!Application.isPlaying) return; // animação só faz sentido rodando o jogo
        if (idlePulse)
        {
            transform.DOScale(transform.localScale * (1f + idlePulseAmount), idlePulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (idleRotate) transform.Rotate(0f, 0f, idleRotateSpeed * Time.deltaTime);
    }
}
