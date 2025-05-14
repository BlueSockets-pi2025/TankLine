using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSC : MonoBehaviour
{
    public Button continueButton; 
    public GameObject echapButton;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (continueButton != null && continueButton.gameObject.activeInHierarchy)
            {
                continueButton.onClick.Invoke();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (echapButton != null && echapButton.activeInHierarchy)
            {
                ExecuteEvents.Execute(echapButton, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }
        }
    }
}