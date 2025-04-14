using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
public class Bootstrap : MonoBehaviour
{
  private static Bootstrap _instance;

  private void Awake()
  {
    if (_instance != null && _instance != this)
    {
      Destroy(gameObject);
      return;
    }
    _instance = this;
    DontDestroyOnLoad(gameObject);

    Debug.Log("[Bootstrap] NetworkRoot initialized");
  }

  private void Start()
  {
    StartCoroutine(LoadMainMenu());
  }

  private System.Collections.IEnumerator LoadMainMenu()
  {
    yield return new WaitForSeconds(0.1f); // for safety
    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Load the main menu scene
  }
}
