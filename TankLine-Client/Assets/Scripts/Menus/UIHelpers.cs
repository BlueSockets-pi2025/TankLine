using UnityEngine;

public static class UIHelpers
{
  public static void ShowTooltip(GameObject tooltip)
  {
    if (tooltip != null)
    {
      tooltip.SetActive(true); // Affiche le tooltip
    }
    else
    {
      Debug.LogWarning("Tooltip is not assigned in the inspector.");
    }
  }

  public static void HideTooltip(GameObject tooltip)
  {
    if (tooltip != null)
    {
      tooltip.SetActive(false); // Cache le tooltip
    }
    else
    {
      Debug.LogWarning("Tooltip is not assigned in the inspector.");
    }
  }

  public void RequestLoadScene(string sceneName)
  {
    if (Instance == null) return;
    Instance.StartCoroutine(SafeSceneLoad(sceneName));
  }

  private IEnumerator SafeSceneLoad(string sceneName)
  {
    Instance = null;
    Destroy(gameObject);
    yield return null;
    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
  }
}
