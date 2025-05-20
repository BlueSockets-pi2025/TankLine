namespace Scripts.Tutoriel
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections;
    using UnityEngine.InputSystem;

    public class TankTutorial : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject tutorialPanel;
        public TMP_Text tutorialText;
        public Button nextButton;
        public Button skipButton;
        public Button exitButton;

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
        public InputActionReference aimAction;

        private int currentStep = 0;
        private bool waitingForInput = false;
        private bool[] stepCompleted;
        private bool hasMoved = false;
        private bool hasAimed = false;
        private bool hasShot = false;
        private bool[] wasdKeysPressed = new bool[4];
        private bool[] arrowKeysPressed = new bool[4];

        public bool IsInShootingStep { get; private set; } = false;

        private void Start()
        {
            stepCompleted = new bool[tutorialSteps.Length];

            // Find mobile controls if not assigned
#if UNITY_ANDROID || UNITY_IOS
            if (moveJoystick == null)
                moveJoystick = FindObjectOfType<MoveJoystick>();
            if (shootJoystick == null)
                shootJoystick = FindObjectOfType<ShootJoystick>();
#endif

            tutorialPanel.SetActive(true);
            tutorialText.text = tutorialSteps[0];
            nextButton.onClick.AddListener(NextStepButton);
            skipButton.onClick.AddListener(SkipTutorialButton);
            exitButton.onClick.AddListener(ExitTutorial);

            if (enemyTanksContainer != null)
            {
                enemyTanksContainer.SetActive(false);
            }
        }

        private void Update()
        {
            // if (!waitingForInput) return;

            switch (currentStep)
            {
                case 1: // Movement step
#if UNITY_ANDROID || UNITY_IOS
                    if (moveJoystick.GetInputVector().magnitude > 0.5f)
                    {
                        hasMoved = true;
                    }
#else
                    wasdKeysPressed[0] = wasdKeysPressed[0] || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A);
                    wasdKeysPressed[1] = wasdKeysPressed[1] || Input.GetKey(KeyCode.S);
                    wasdKeysPressed[2] = wasdKeysPressed[2] || Input.GetKey(KeyCode.D);
                    wasdKeysPressed[3] = wasdKeysPressed[3] || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W);

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
                        hasMoved = true;
                    }
#endif
                    if (hasMoved)
                    {
                        CompleteStep();
                    }
                    break;

                case 2: // Aiming step
#if UNITY_ANDROID || UNITY_IOS
                    if (shootJoystick.GetInput().magnitude > 0.5f)
                    {
                        hasAimed = true;
                    }
#else
                    if (Mathf.Abs(Input.GetAxis("Mouse X")) > 1.5f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 1.5f)
                    {
                        hasAimed = true;
                    }
#endif
                    if (hasAimed)
                    {
                        CompleteStep();
                    }
                    break;

                case 3: // Shooting step
                    break;
                case 4:
                    if (!enemyTanksContainer)
                    {
                        exitButton.gameObject.SetActive(true);
                    }
                    break;
            }
        }
        public void NextStepButton()
        {
            StartCoroutine(NextStep());
        }

        public IEnumerator NextStep()
        {
            if (currentStep < tutorialSteps.Length - 1)
            {
                currentStep++;
                tutorialText.text = tutorialSteps[currentStep];
                IsInShootingStep = currentStep >= 3;

                hasMoved = false;
                hasAimed = false;
                hasShot = false;
                wasdKeysPressed = new bool[4];
                arrowKeysPressed = new bool[4];

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
                    Debug.Log("step" + currentStep);
                    enemyTanksContainer.SetActive(true);
                    nextButton.gameObject.SetActive(false);
                    skipButton.gameObject.SetActive(false);
                    yield return new WaitForSeconds(3f);
                    tutorialText.gameObject.SetActive(false);

                }
            }
            else
            {
                tutorialPanel.SetActive(false);
                this.enabled = false;
            }
        }
        public void SkipTutorialButton()
        {
            StartCoroutine(SkipTutorial());
        }

        public IEnumerator SkipTutorial()
        {
            // tutorialPanel.SetActive(false);
            nextButton.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
            IsInShootingStep = true;
            if (enemyTanksContainer != null) enemyTanksContainer.SetActive(true);
            currentStep = 4;
            tutorialText.text = tutorialSteps[currentStep];
            yield return new WaitForSeconds(3f);
            tutorialText.gameObject.SetActive(false);
        }

        public void ExitTutorial()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        public bool NotifyShotFired()
        {
            if (currentStep == 3 && !stepCompleted[currentStep])
            {
                hasShot = true;
                CompleteStep();
            }
            if (currentStep == 4) { hasShot = true; }
            return hasShot;
        }

        private void CompleteStep()
        {
            if (!stepCompleted[currentStep])
            {
                stepCompleted[currentStep] = true;
                waitingForInput = false;

                StartCoroutine(ShowCompletionFeedback());
            }
        }

        private IEnumerator ShowCompletionFeedback()
        {
            string originalText = tutorialText.text;
            tutorialText.text = originalText + "\n<color=green>(Completed!)</color>";

            yield return new WaitForSeconds(1f);
            nextButton.gameObject.SetActive(true);

            tutorialText.text = originalText;
        }
        public void OnShootPerformed()
        {
            if (currentStep == 3 && !stepCompleted[currentStep])
            {
                hasShot = true;
                CompleteStep();
            }
        }
    }
}