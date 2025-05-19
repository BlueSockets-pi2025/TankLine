using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TogglePasswordVisibility : MonoBehaviour
{
    public TMP_InputField passwordInput;
    public Button eyeButton;
    public RawImage eyeImage; // Change to RawImage instead of Image
    public Texture2D eyeOpenTexture; // Raw image texture for open eye
    public Texture2D eyeClosedTexture; // Raw image texture for closed eye

    private bool isPasswordVisible = false;

    void Start()
    {
        eyeButton.onClick.AddListener(TogglePassword);
        UpdateEyeIcon();
    }

    void TogglePassword()
    {
        #if UNITY_ANDROID
        isPasswordVisible = !isPasswordVisible;

        passwordInput.contentType = isPasswordVisible ? 
            TMP_InputField.ContentType.Standard : 
            TMP_InputField.ContentType.Password;

        passwordInput.ForceLabelUpdate();
        UpdateEyeIcon();
        #endif
    }

    void UpdateEyeIcon()
    {
        #if UNITY_ANDROID
        if (eyeImage != null)
        {
            // Set the texture based on password visibility
            eyeImage.texture = isPasswordVisible ? eyeOpenTexture : eyeClosedTexture;
        }
        #endif
    }
}
