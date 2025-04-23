namespace Scripts.Tutoriel
{
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
    public Button skipButton;


    [Header("Tutorial Steps")]
    public string[] tutorialSteps = {
        "Welcome to Tankline Battle!",
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
    private bool[] wasdKeysPressed = new bool[4];
    private bool[] arrowKeysPressed = new bool[4];
    public bool IsInShootingStep { get; private set; } = false;

    private void Start()
    {
        stepCompleted = new bool[tutorialSteps.Length];

        tutorialPanel.SetActive(true);
        tutorialText.text = tutorialSteps[0];
        nextButton.onClick.AddListener(NextStep);
        skipButton.onClick.AddListener(SkipTutorial);

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
                wasdKeysPressed[0] = wasdKeysPressed[0] || Input.GetKey(KeyCode.Q);
                wasdKeysPressed[1] = wasdKeysPressed[1] || Input.GetKey(KeyCode.S);
                wasdKeysPressed[2] = wasdKeysPressed[2] || Input.GetKey(KeyCode.D);
                wasdKeysPressed[3] = wasdKeysPressed[3] || Input.GetKey(KeyCode.Z);

                arrowKeysPressed[0] = arrowKeysPressed[0] || Input.GetKey(KeyCode.LeftArrow);
                arrowKeysPressed[1] = arrowKeysPressed[1] || Input.GetKey(KeyCode.RightArrow);
                arrowKeysPressed[2] = arrowKeysPressed[2] || Input.GetKey(KeyCode.UpArrow);
                arrowKeysPressed[3] = arrowKeysPressed[3] || Input.GetKey(KeyCode.DownArrow);

                bool allWASDPressed = true;
                bool allArrowsPressed = true;

                for (int i = 0; i < 4; i++)
                {
                    if (!wasdKeysPressed[i]) allWASDPressed = false;
                    if (!arrowKeysPressed[i]) allArrowsPressed = false;
                }

                if (allWASDPressed || allArrowsPressed)
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
            IsInShootingStep = currentStep >= 4;

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
    public void SkipTutorial()
    {
        tutorialPanel.SetActive(false);
        IsInShootingStep = true;
        if (enemyTanksContainer != null) enemyTanksContainer.SetActive(true);
        this.enabled = false;
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

        yield return new WaitForSeconds(3f);

        tutorialText.text = originalText;
    }
}
}