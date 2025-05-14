using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Heartbeat;
using UnityEngine.InputSystem;


public class SettingsSwapper : MonoBehaviour
{
    public GameObject General, Mouse, Keyboard;
    public InputActionAsset actions;

    private void Start()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            actions.LoadBindingOverridesFromJson(rebinds);
            rebinds = actions.SaveBindingOverridesAsJson();
        }
    }

    public void OpenSettingsGeneral()
    {
        Mouse.SetActive(false);
        Keyboard.SetActive(false);
        General.SetActive(true);
    }
    public void OpenSettingsMouse()
    {
        Keyboard.SetActive(false);
        General.SetActive(false);
        Mouse.SetActive(true);
    }
    public void OpenSettingsKeyboard()
    {
        Mouse.SetActive(false);
        General.SetActive(false);
        Keyboard.SetActive(true);
    }
}