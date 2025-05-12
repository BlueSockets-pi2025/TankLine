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
        Debug.Log("OnEnable rebindMenu");
        // MoveRef.action.DeactivateInput();
        action.Disable();
        // MoveRef.action.Disable();
        ShootRef.action.Disable();

    }
    private void OnDisable()
    {
        Debug.Log("OnDisable rebindMenu");
        MoveRef.action.Enable();
        ShootRef.action.Enable();
    }


}