using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerClickHandler
{
    public SFXType clip = SFXType.ButtonClick;
    private Button btn;
    private void Start()
    {
        btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlaySFXEnum(clip);
            });
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (btn == null)
        {
            SoundManager.Instance?.PlaySFXEnum(clip);
        }
    }
}

