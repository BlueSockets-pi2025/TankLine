using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TankTutorial : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    public Button nextButton;
    
    [Header("Tutorial Steps")]
    public string[] tutorialSteps = {
        "Welcome to Tank Battle!",
        "You can move your tank using QSZD or Arrow keys",
        "Move your mouse to change the direction of your tank's turret",
        "Left-click to shoot bullets at your enemies",
        "You're now ready to start your journey! Destroy the enemy tanks"
    };
    
    [Header("References")]
    public GameObject playerTank;
    public GameObject enemyTanksContainer;
    
    private int currentStep = 0;
    private bool waitingForInput = false;
    private bool[] stepCompleted;
    
    private void Start()
    {
        stepCompleted = new bool[tutorialSteps.Length];
        
        tutorialPanel.SetActive(true);
        tutorialText.text = tutorialSteps[0];
        nextButton.onClick.AddListener(NextStep);
        
        if (enemyTanksContainer != null)
        {
            enemyTanksContainer.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!waitingForInput) return;
        
        switch (currentStep)
        {
            case 1:
                if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Z) ||
                    Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
                {
                    CompleteStep();
                }
                break;
                
            case 2: 
                if (Mathf.Abs(Input.GetAxis("Mouse X")) > 1.5f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 1.5f)
                {
                    CompleteStep();
                }
                break;
                
            case 3:
                if (Input.GetMouseButtonDown(0))
                {
                    CompleteStep();
                }
                break;
        }
    }
    
    public void NextStep()
    {
        if (currentStep < tutorialSteps.Length - 1)
        {
            currentStep++;
            tutorialText.text = tutorialSteps[currentStep];
            
            if (currentStep >= 1 && currentStep <= 3)
            {
                nextButton.gameObject.SetActive(false);
                waitingForInput = true;
            }
            else
            {
                nextButton.gameObject.SetActive(true);
                waitingForInput = false;
            }
            
            if (currentStep == tutorialSteps.Length - 1 && enemyTanksContainer != null)
            {
                enemyTanksContainer.SetActive(true);
            }
        }
        else
        {
        
            tutorialPanel.SetActive(false);
            this.enabled = false; 
        }
    }
    
    private void CompleteStep()
    {
        if (!stepCompleted[currentStep])
        {
            stepCompleted[currentStep] = true;
            waitingForInput = false;
            nextButton.gameObject.SetActive(true);
            
            StartCoroutine(ShowCompletionFeedback());
        }
    }
    
    private IEnumerator ShowCompletionFeedback()
    {
        string originalText = tutorialText.text;
        tutorialText.text = originalText + "\n<color=green>(Completed!)</color>";
        
        yield return new WaitForSeconds(2f);
        
        tutorialText.text = originalText;
    }
}