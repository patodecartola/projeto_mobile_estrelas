using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAim : MonoBehaviour
{
    [Header("Referências")]
    public Camera cam;
    public LineRenderer aimLine; // opcional — desenha a linha de mira enquanto arrasta

    [Header("Estilingue")]
    public float maxDragDistance = 2.2f; // em unidades do mundo
    public float minDragToFire = 0.2f;

    Vector2 anchor;
    bool aiming;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) BeginAim();
        if (Input.GetMouseButton(0) && aiming) UpdateAim(GetWorldPoint());
        if (Input.GetMouseButtonUp(0) && aiming) EndAim(GetWorldPoint());
    }

    Vector2 GetWorldPoint()
    {
        Vector3 sp = Input.mousePosition;
        sp.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(sp);
    }

    void BeginAim()
    {
        // Não deixa o arrasto começar em cima de um botão (ex: os de arma) —
        // senão o toque no botão também contaria como início de mira.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        aiming = true;
        anchor = transform.position;
        if (aimLine != null) aimLine.gameObject.SetActive(true);
    }

    void UpdateAim(Vector2 current)
    {
        Vector2 drag = current - anchor;
        float dist = Mathf.Min(drag.magnitude, maxDragDistance);
        Vector2 dir = drag.sqrMagnitude > 0.0001f ? drag.normalized : Vector2.zero;
        Vector2 tip = anchor - dir * dist * 1.4f; // ponta da mira, do lado oposto ao arrasto

        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, anchor);
            aimLine.SetPosition(1, tip);
        }
    }

    void EndAim(Vector2 current)
    {
        aiming = false;
        if (aimLine != null) aimLine.gameObject.SetActive(false);

        Vector2 drag = current - anchor;
        float dist = Mathf.Min(drag.magnitude, maxDragDistance);
        if (dist < minDragToFire) return;

        Vector2 direction = -drag.normalized; // estilingue: dispara pro lado oposto do arrasto
        float power = dist / maxDragDistance;
        WeaponManager.Instance.Fire(anchor, direction, power);
    }
}
