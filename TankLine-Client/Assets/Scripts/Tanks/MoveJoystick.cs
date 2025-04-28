using UnityEngine;
using UnityEngine.EventSystems;

public class MoveJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Vector2 inputVector;
    public RectTransform handle;
    private Tank_Offline tank;
    // protected Transform tank;

    private void Start()
    {
        tank = FindObjectOfType<Tank_Offline>();
        // tank = gameObject.transform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (tank == null) return;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            null,
            out pos
        );

        inputVector = (pos.magnitude > 50) ? pos.normalized : pos / 50;
        handle.anchoredPosition = inputVector * 27;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tank == null) return;
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (tank == null) return;
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    public float GetHorizontal()
    {
        return inputVector.x;
    }

    public float GetVertical()
    {
        return inputVector.y;
    }
}
