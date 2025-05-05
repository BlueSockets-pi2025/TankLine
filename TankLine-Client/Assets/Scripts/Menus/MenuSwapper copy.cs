using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public class MenuSwapper2 : MonoBehaviour
{

    public GameObject _canvas;
    private Transform Canvas;
    public GameObject ErrorPopup;
    public InputActionAsset actions;


    void Awake()
    {
        Canvas = _canvas.transform;
    }


    private void Start()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            actions.LoadBindingOverridesFromJson(rebinds);
            rebinds = actions.SaveBindingOverridesAsJson();
        }
    }
    public void OpenErr()
    {
        ErrorPopup.SetActive(true);
    }
    public void CloseErr()
    {
        ErrorPopup.SetActive(false);
    }
}