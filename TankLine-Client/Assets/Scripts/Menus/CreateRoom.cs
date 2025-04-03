using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    public Text selectedNumberText;
    public Text selectedModeText;
    public List<Button> numberButtons;
    public List<Button> modeButtons;
    private int selectedNumber = 2;
    private string selectedMode = "Public";
    private Color normalColor = Color.white;
    private Color selectedColor = Color.red;
    public void SelectNumber(int number)
    {
        selectedNumber = number;
        if (selectedNumberText != null)
            selectedNumberText.text = "Selected Number: " + selectedNumber;
        UpdateButtonColors(numberButtons, number);
    }

    public void SelectMode(string mode)
    {
        selectedMode = mode;
        if (selectedModeText != null)
            selectedModeText.text = "Selected Mode: " + selectedMode;
        UpdateButtonColors(modeButtons, mode);
    }

    private void UpdateButtonColors<T>(List<Button> buttons, T selectedValue)
    {
        foreach (Button btn in buttons)
        {
            bool isSelected = btn.name == selectedValue.ToString();

            if (btn.image != null)
            {
                btn.image.color = isSelected ? selectedColor : normalColor;
            }
        }
    }
    public void ValidateSelection()
{
    Debug.Log("Number Chosen: " + selectedNumber);
    Debug.Log("Mode Chosen: " + selectedMode);
    ResetButtonColors(numberButtons);
    ResetButtonColors(modeButtons);
    if (selectedNumberText != null)
        selectedNumberText.text = "Selected Number: ";
    if (selectedModeText != null)
        selectedModeText.text = "Selected Mode: ";
}

private void ResetButtonColors(List<Button> buttons)
{
    foreach (Button btn in buttons)
    {
        if (btn.image != null)
        {
            btn.image.color = normalColor;
        }
    }
}
}