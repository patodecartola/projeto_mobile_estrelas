using UnityEngine;

// Sem isso, o orthographic size fixo mostra área diferente da arena em cada
// aparelho: telas muito estreitas (ex: iPhone SE) cortam a visão, telas muito
// largas (ex: tablets) mostram área demais. Esse script recalcula o tamanho
// visível no Awake — antes de qualquer outro script (EnemySpawner,
// ArenaBackground) ler orthographicSize no próprio Start.
[RequireComponent(typeof(Camera))]
public class CameraAspectFit : MonoBehaviour
{
    [Tooltip("Orthographic Size calibrado no aparelho que você já testou (o valor que está bom hoje).")]
    public float baseOrthographicSize = 7f;

    [Tooltip("Não deixa a largura visível da arena cair abaixo disso (telas estreitas, ex: iPhone SE).")]
    public float minVisibleWidth = 7f;

    [Tooltip("Não deixa a largura visível da arena passar disso (telas largas/tablets).")]
    public float maxVisibleWidth = 11f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Fit();
    }

    void Fit()
    {
        float aspect = (float)Screen.width / Screen.height;
        float size = baseOrthographicSize;
        float visibleWidth = size * 2f * aspect;

        if (visibleWidth < minVisibleWidth)
            size = minVisibleWidth / (2f * aspect);
        else if (visibleWidth > maxVisibleWidth)
            size = maxVisibleWidth / (2f * aspect);

        cam.orthographicSize = size;
    }
}
