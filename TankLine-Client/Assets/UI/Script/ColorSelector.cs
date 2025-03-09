using UnityEngine;
using UnityEngine.UI;
using System;

public class ColorSelector : MonoBehaviour
{
    public GameObject colorChosen;
    public Button c1, c2, c3;
    private Button selectedButton;
    private Color originalColor = Color.white;

    private void Start()
    {
        c1.onClick.AddListener(() => SetColor(Color.red, c1));
        c2.onClick.AddListener(() => SetColor(Color.green, c2));
        c3.onClick.AddListener(() => SetColor(Color.blue, c3));

        // Store the original color of the button border
        originalColor = c1.GetComponent<Image>().color;
    }

    private void SetColor(Color color, Button button)
    {
        // Set the color of the target object
        Renderer renderer = colorChosen.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Image image = colorChosen.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        // Handle button selection
        if (selectedButton != null)
        {
            // Reset the previous button's border
            var prevOutline = selectedButton.GetComponent<Outline>();
            if (prevOutline != null)
            {
                Destroy(prevOutline);
            }
        }

        // Add a black border to the selected button
        var outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, 3);

        selectedButton = button;
    }

    public void SetColorRed() => SetColor(Color.red, c1);
    public void SetColorGreen() => SetColor(Color.green, c2);
    public void SetColorBlue() => SetColor(Color.blue, c3);
}
