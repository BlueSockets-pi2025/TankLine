using UnityEngine;
using UnityEngine.EventSystems;
using Scripts.Tutoriel;
public class ShootJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Vector2 inputVector;
    public RectTransform handle;
    public Vector2 GetInput() => inputVector;
    private Vector2 startTouchPosition;
    private bool isDragging = false;
    private TankTutorial tutorial;
    public GameObject player;
    bool canShoot = false;

    public void Start()
    {
        tutorial = FindObjectOfType<TankTutorial>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            null,
            out pos
        );

        float radius = GetComponent<RectTransform>().sizeDelta.x / 4;

        if (Vector2.Distance(startTouchPosition, eventData.position) > 10f)
        {
            isDragging = true;
        }

        inputVector = pos.magnitude > radius ? pos.normalized : pos / radius;
        handle.anchoredPosition = inputVector * radius;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
        isDragging = false;
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;

        if (!isDragging)
        {

            // Notify tutorial if active
            // var tutorial = FindObjectOfType<TankTutorial>();
            if (tutorial != null)
            {
                Debug.Log("NotifyShotFired");

                canShoot = tutorial.NotifyShotFired();
                if (canShoot)
                {
                    player.GetComponent<Tank_Offline>()?.OnShootButtonClick();
                }
            }

            if (player != null)
            {
                player.GetComponent<Tank_Player>()?.OnShootButtonClick();
            }
        }
    }
}
