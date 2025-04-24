using UnityEngine;

public class TooltipHandler : MonoBehaviour
{
    public GameObject tooltip; 

    private void OnDisable()
    {
        if (tooltip != null)
        {
            MainMenuSwapper.HideTooltip(tooltip); 
        }
    }
}