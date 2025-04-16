using UnityEngine;
using TMPro;

public class Clavier : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;

    public TMP_InputField tmpInputField;

    void Start()
    {
        if (tmpInputField != null)
        {
            tmpInputField.onSelect.AddListener(OnInputFieldSelected);
        }
    }

    void OnInputFieldSelected(string text)
    {
        OpenTouchKeyboard();
    }

    public void OpenTouchKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    void Update()
    {
        if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            Debug.Log("Clavier fermé");
        }
    }
}
