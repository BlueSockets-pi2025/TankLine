using UnityEngine;
using UnityEngine.EventSystems;

public class ShootJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Vector2 inputVector;
    public RectTransform handle; // Drag and drop ton joystick handle ici
    public Vector2 GetInput() => inputVector;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            null,
            out pos
        );

        float radius = GetComponent<RectTransform>().sizeDelta.x / 2; // Rayon du joystick

        inputVector = pos.magnitude > radius ? pos.normalized : pos / radius;
        handle.anchoredPosition = inputVector * radius;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
