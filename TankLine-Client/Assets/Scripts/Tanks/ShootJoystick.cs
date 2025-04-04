using UnityEngine;
using UnityEngine.EventSystems;

public class ShootJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Vector2 inputVector;
    public RectTransform handle;
    public Vector2 GetInput() => inputVector;
    private Vector2 startTouchPosition;
    private bool isDragging = false;
    private Tank_Offline tank;

    private void Start()
    {
        tank = FindObjectOfType<Tank_Offline>();
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

        float radius = GetComponent<RectTransform>().sizeDelta.x / 2;

        if (Vector2.Distance(startTouchPosition, eventData.position) > 10f)
        {
            isDragging = true;
        }

        inputVector = pos.magnitude > radius ? pos.normalized : pos / radius;
        handle.anchoredPosition = inputVector * radius;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (tank == null) return;
        startTouchPosition = eventData.position;
        isDragging = false;
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (tank == null) return;
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;

        if (!isDragging)
        {
            FindObjectOfType<Tank_Offline>().OnShootButtonClick();
        }
    }
}
