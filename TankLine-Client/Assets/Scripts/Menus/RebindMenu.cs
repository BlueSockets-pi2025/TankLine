using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindMenu : MonoBehaviour
{
    public InputActionAsset action;
    public InputActionReference MoveRef, ShootRef;
    private void OnEnable()
    {
        MoveRef.action.Disable();
        ShootRef.action.Disable();
    }
    private void OnDisable()
    {
        MoveRef.action.Enable();
        ShootRef.action.Enable();
    }


}