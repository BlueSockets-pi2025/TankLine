using System;
using UnityEngine;
using System.Collections;
using TMPro;

public class FieldsValidation : MonoBehaviour 
{
    public static void ValidatePasswordField(TMP_InputField passwordField, GameObject errorText) {
        var outline = passwordField.GetComponent<UnityEngine.UI.Outline>();

        if (outline != null) {
            if (string.IsNullOrEmpty(passwordField.text) || !InputCheckers.IsValidPassword(passwordField.text)) {
                outline.enabled = true; 
                errorText.SetActive(true); 
            } else {
                outline.enabled = false; 
                errorText.SetActive(false);
            }
        } else {
            Debug.LogWarning("Outline component not found on the input field.");
        }
    }
    
    public static void ValidateConfirmPasswordField(TMP_InputField passwordField, TMP_InputField confirmPasswordField, GameObject errorText) {
        var outline = confirmPasswordField.GetComponent<UnityEngine.UI.Outline>();

        if (outline != null) {
            if (passwordField.text != confirmPasswordField.text) {
                outline.enabled = true;
                errorText.SetActive(true);
            } else {
                outline.enabled = false; 
                errorText.SetActive(false);
            }
        } else {
            Debug.LogWarning("Outline component not found on the confirm password field.");
        }
    }
}