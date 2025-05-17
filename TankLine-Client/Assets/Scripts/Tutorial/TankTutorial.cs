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
            "Welcome to Tankline",
            "Use the left joystick to move your tank",
            "Use the right joystick to aim your tank's turret",
            "Tap the right joystick to shoot bullets at your enemies",
            "You're now ready to start your journey! Destroy the enemy tanks"
        };

        [Header("References")]
        public GameObject playerTank;
        public GameObject enemyTanksContainer;
        public MoveJoystick moveJoystick;
        public ShootJoystick shootJoystick;

        private int currentStep = 0;
        private bool waitingForInput = false;
        private bool[] stepCompleted;
        private bool hasMoved = false;
        private bool hasAimed = false;
        private bool hasShot = false;

        public bool IsInShootingStep { get; private set; } = false;

        private void Start()
        {
            stepCompleted = new bool[tutorialSteps.Length];
            
            if (moveJoystick == null)
                moveJoystick = FindObjectOfType<MoveJoystick>();
            if (shootJoystick == null)
                shootJoystick = FindObjectOfType<ShootJoystick>();

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
                    if (moveJoystick.GetInputVector().magnitude > 0.5f)
                    {
                        hasMoved = true;
                    }
                    
                    if (hasMoved)
                    {
                        CompleteStep();
                    }
                    break;

                case 2: 
                    if (shootJoystick.GetInput().magnitude > 0.5f)
                    {
                        hasAimed = true;
                    }
                    
                    if (hasAimed)
                    {
                        CompleteStep();
                    }
                    break;

                case 3: 
                    break;
            }
        }

        public void NextStep()
        {
            if (currentStep < tutorialSteps.Length - 1)
            {
                currentStep++;
                tutorialText.text = tutorialSteps[currentStep];
                IsInShootingStep = currentStep >= 3;

                hasMoved = false;
                hasAimed = false;
                hasShot = false;

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

        public void NotifyShotFired()
        {
            if (currentStep == 3 && !stepCompleted[currentStep])
            {
                hasShot = true;
                CompleteStep();
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

            yield return new WaitForSeconds(3f);

            tutorialText.text = originalText;
        }
    }
}