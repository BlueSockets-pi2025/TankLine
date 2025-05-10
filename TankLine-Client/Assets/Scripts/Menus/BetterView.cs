using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class BetterView : MonoBehaviour
{
    public RectTransform uiRoot;
    public List<TMP_InputField> inputFields;
    public float shiftAmount = 300f;

    private float addKey = 0f;
    private bool isShifted = false;

    void Start()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            int j=i;
            var input = inputFields[i];
            if (input != null)
            {
                input.onSelect.AddListener((_) => OnAnyInputSelected(j));
                input.onDeselect.AddListener((_) => OnAnyInputDeselected());
            }
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            var input = inputFields[i];
            if (input != null)
            {
                input.onSelect.RemoveAllListeners();
                input.onDeselect.RemoveAllListeners();
            }
        }
    }

    void OnAnyInputSelected(int indexField)
    {
        if (!isShifted)
        {
            addKey = shiftAmount + indexField * 150f;
            uiRoot.anchoredPosition += new Vector2(0, addKey);
            isShifted = true;
        }
    }

    void OnAnyInputDeselected()
    {
        StartCoroutine(DelayedFocusCheck());
    }

    System.Collections.IEnumerator DelayedFocusCheck()
    {
        yield return null;

        bool anyFocused = inputFields.Any(input => input != null && input.isFocused);

        if (!anyFocused && isShifted)
        {
            uiRoot.anchoredPosition -= new Vector2(0, addKey);
            addKey = 0f;
            isShifted = false;
        }
    }
}
