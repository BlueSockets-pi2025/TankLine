using UnityEngine;
using UnityEngine.SceneManagement;

public class BadgeClicked : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("Clicked badge !");
        SceneManager.LoadScene("PlayedGame"); 
    }
}
