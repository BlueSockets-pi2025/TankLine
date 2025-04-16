using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI tutorialText;
    public Image controlsImage;
    public TextMeshProUGUI bulletCountText;
    public TextMeshProUGUI livesText;
    
    [Header("Tutorial Steps")]
    public string[] tutorialMessages;
    public Sprite[] controlImages;
    
    private int currentStep = 0;
    private Tank_Offline playerTank;
    
    void Start()
    {
        playerTank = FindObjectOfType<Tank_Offline>();
        ShowCurrentStep();
        UpdateBulletCount(playerTank.MaxBulletShot - playerTank.nbBulletShot);
        UpdateLives((int)playerTank.nbLifeLeft);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextStep();
        }
    }
    
    public void NextStep()
    {
        if (currentStep < tutorialMessages.Length - 1)
        {
            currentStep++;
            ShowCurrentStep();
        }
        else
        {
            // Tutorial complete
            tutorialText.text = "Press ESC to exit";
        }
    }
    
    private void ShowCurrentStep()
    {
        tutorialText.text = tutorialMessages[currentStep];
        if (currentStep < controlImages.Length)
        {
            controlsImage.sprite = controlImages[currentStep];
        }
    }
    
    public void UpdateBulletCount(int count)
    {
        bulletCountText.text = $"Bullets: {count}/{playerTank.MaxBulletShot}";
    }
    
    public void UpdateLives(int lives)
    {
        livesText.text = $"Lives: {lives}/3";
    }
}