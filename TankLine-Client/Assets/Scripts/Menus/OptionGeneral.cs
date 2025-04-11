using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionGeneral : MonoBehaviour
{
    public List<Button> HubButtons;
    public List<Button> LangButtons;

    private string selectedHub = "Small";
    private string selectedLang = "En";
    public Sprite normalSprite;
    public Sprite selectedSprite;

    public void SelectHub(string Hub)
    {
        selectedHub = Hub;
        Debug.Log("Selected Hub: " + selectedHub);
        UpdateButtonStates(HubButtons, Hub);
    }

    public void SelectLang(string Lang)
    {
        selectedLang = Lang;
        Debug.Log("Selected Lang: " + selectedLang);
        UpdateButtonStates(LangButtons, Lang);
    }

    private void UpdateButtonStates<T>(List<Button> buttons, T selectedValue)
    {
        foreach (Button btn in buttons)
        {
            bool isSelected = btn.name == selectedValue.ToString();
            SetButtonSprite(btn, isSelected);
        }
    }

    private void SetButtonSprite(Button button, bool isSelected)
    {
        if (button.image != null)
        {
            button.image.sprite = isSelected ? selectedSprite : normalSprite;
        }
    }

    public void ValidateSelection()
    {
        Debug.Log("Hub Chosen: " + selectedHub);
        Debug.Log("Lang Chosen: " + selectedLang);
        ResetButtonSprites(HubButtons);
        ResetButtonSprites(LangButtons);
    }

    private void ResetButtonSprites(List<Button> buttons)
    {
        foreach (Button btn in buttons)
        {
            if (btn.image != null)
            {
                btn.image.sprite = normalSprite;
            }
        }
    }
}
