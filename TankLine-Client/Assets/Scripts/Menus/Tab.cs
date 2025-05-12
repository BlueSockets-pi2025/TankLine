using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Tab : MonoBehaviour
{
    [SerializeField] private List<TMP_InputField> inputFields;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            int i = inputFields.FindIndex(f => f.gameObject == current);
            if (i >= 0)
            {
                int j = (i + 1) % inputFields.Count;
                inputFields[j].Select();
            }

            InputField currentInput = current?.GetComponent<InputField>();
            if (currentInput != null)
            {
                currentInput.OnDeselect(null);
            }
        }

    }
}
