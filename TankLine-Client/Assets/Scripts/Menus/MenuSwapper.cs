using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Heartbeat; 
using UnityEngine.UI;

public class MenuSwapper : MonoBehaviour
{
    public static MenuSwapper Instance { get; private set; }
    private AuthController authController;

    public GameObject _canvas;
    private Transform Canvas;
    public GameObject ErrorPopup, MessagePopup;

    public GameObject CurrentPage;

    public GameObject EmailErrorText;
    public GameObject PasswordErrorText;
    public GameObject ConfirmationErrorText;

    public GameObject PasswordErrorTextReset;
    public GameObject ConfirmationErrorTextReset;

    public Toggle RememberMeToggle; // reference to the “Remember Me” checkbox

    // Testing : 
    public TMP_Text gamesPlayedInputField;
    public TMP_Text highestScoreInputField;
    public TMP_Text UserNameField;
    public TMP_Text UserRankField;

    public static bool isFirstLaunch = true ; 

    void Awake()
    {
        // Ensure there is only one instance of this script
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Canvas = _canvas.transform;       
    }

    private void Start()
    {
        authController = GetComponent<AuthController>();
        if (authController == null)
        {
            Debug.LogError("AuthController not found.");
            return;
        }

        // Load first page: 
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == "ConnectionMenu")
        {
            OpenPage("PagePrincipale");
            
            if (RememberMeToggle != null)
            {
                // Set the checkbox to true by default:
                RememberMeToggle.isOn = true;
            }
            // If the game is launched (connection page) for the first time, auto-login:
            if (isFirstLaunch)
            {
                AutoLogin();
                isFirstLaunch = false; 
                Debug.Log("FIRST LAUNCH");
            }
            else Debug.Log ("NOT FIRST LAUNCH");
        }

        else if (currentScene == "MainMenu")
        {
            OpenPage("MainMenu");
            //AutoLogin();
        }
    }

    private void AutoLogin()
    {
        StartCoroutine(AutoLoginCoroutine());
    }

    /// <summary>
    /// Coroutine function for auto login 
    /// </summary>
    private IEnumerator AutoLoginCoroutine()
    {
        Debug.Log("Attempting auto-login...");

        // Restore refresh token in HTTP-ONLY cookies (from local persistent file between sessions):
        yield return authController.RestoreRefreshCookie();

        if (!authController.IsRequestSuccessful) // If remember me hasn't been activated, for example, or if you're connecting for the first time 
        {
            Debug.Log("Failed to restore refresh token. Redirecting to login page.");
            OpenPage("PagePrincipale");
            yield break;
        }

        // Attempt auto-login using the refresh token:
        yield return authController.AutoLogin();
        
        if (authController.IsRequestSuccessful)
        {
            Debug.Log("Auto-login successful. Skipping login page.");

            // Check if the refresh token is still valid for 1 day and rotate it if necessary (must re-login the next time it is used as the file will not be updated): 
            StartCoroutine(CheckAndRotateRefreshTokenCoroutine());

            // Set User informations:
            yield return authController.User();

            if (authController.IsRequestSuccessful)
            {
                Debug.Log("User data retrieved successfully.");

                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentScene == "ConnectionMenu")
                {
                    // Load the main menu scene:
                    MainMenuScene();
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve user data: {authController.ErrorResponse}");
                OpenErr($"Failed to retrieve user data: {authController.ErrorResponse}");
            }
        }
        else
        {
            Debug.Log("Auto-login failed. Redirecting to login page.");
            OpenErr($"Auto-login failed: {authController.ErrorResponse}");
            OpenPage("PagePrincipale"); // Connection page 
        }
    }

    /// <summary>
    /// Open the page "pageName" in the canvas, and close every other page </br>
    /// The page must be a direct child of the main canvas
    /// </summary>
    /// <param name="pageName">The name of the page</param>
    public void OpenPage(string pageName)
    {

        // disable old page
        CurrentPage.SetActive(false);

        // find new page and enable it
        CurrentPage = Canvas.Find(pageName).gameObject;

        if (CurrentPage == null)
        {
            Debug.LogError($"Page not found : {pageName}");
            return;
        }

        CurrentPage.SetActive(true);

        // if (pageName == "Score")
        // {
        //     Debug.Log("Opening Score page. Updating leaderboard...");
        //     UpdateLeaderboard();
        // }

        // if switch to mainMenu or play, load stats & name
        if (pageName == "MainMenu" || pageName == "Play")
        {
            StartCoroutine(UpdateStatisticsCoroutine(CurrentPage.transform.Find("Badge")));
        }

        if (pageName == "MainMenu")
        {
            GameManager.Instance?.UpdateGameState(GameState.Menu);
            // StartCoroutine(CheckAndRotateRefreshTokenCoroutine());
        }
    }

    // Popup display and close function: 

    public void OpenErr(string message)
    {
        ErrorPopup.SetActive(true);
        ErrorPopup.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = message; // change popup main text
    }
    public void CloseErr()
    {
        ErrorPopup.SetActive(false);
    }
    public void OpenMessage(string message)
    {
        MessagePopup.SetActive(true);
        MessagePopup.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = message; // change popup main text
    }
    public void CloseMessage()
    {
        MessagePopup.SetActive(false);
    }

    /// <summary>
    /// Login the user with the username and password in the input fields </br>
    /// </summary>
    public void LoginUser()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        Transform page = CurrentPage.name == "PagePrincipale" ? CurrentPage.transform : Canvas.Find("PagePrincipale").transform;
        string username = page.Find("LogUsername").GetComponent<TMP_InputField>().text;
        string password = page.Find("LogPassword").GetComponent<TMP_InputField>().text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            OpenErr("Both username and password fields must be filled.");
            return;
        }

        if (CurrentPage.name == "PagePrincipale")
            StartCoroutine(LoginUserCoroutine(CurrentPage.transform));
        else
            StartCoroutine(LoginUserCoroutine(Canvas.Find("PagePrincipale").transform));

    }

    /// <summary>
    /// Coroutine function for login the user </br>
    /// </summary>
    /// <param name="PagePrincipale">The page where the username and password are</param>
    private IEnumerator LoginUserCoroutine(Transform PagePrincipale)
    {

        // Find username and password input field:
        yield return authController.Login(PagePrincipale.Find("LogUsername").GetComponent<TMP_InputField>().text,
                                        PagePrincipale.Find("LogPassword").GetComponent<TMP_InputField>().text);

        if (authController.IsRequestSuccessful)
        {
            // Load the user data:
            yield return authController.User();

            if (authController.IsRequestSuccessful)
            {
                Debug.Log("DEBUG"); 
                Debug.Log(authController.CurrentUser.username); 
                yield return authController.UserStatistics();

                if (authController.IsRequestSuccessful)
                {
                    MainMenuScene();
                }
                else
                {
                    OpenErr($"Failed to retrieve user statistics:\n {authController.ErrorResponse}");
                }
            }
            else
            {
                OpenErr($"Failed to retrieve user data: \n {authController.ErrorResponse}");
            }
        }
        else
        {
            OpenErr($"Login failed: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Set email to reset user password </br>
    /// </summary>
    public void ResetPassword()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        Transform resetPasswordPage = CurrentPage.name == "ResetPasswordStep1" ? CurrentPage.transform : Canvas.Find("ResetPasswordStep1").transform;
        string email = resetPasswordPage.Find("MailInputField").GetComponent<TMP_InputField>().text;

        if (!InputCheckers.IsValidEmail(email))
        {
            OpenErr("Invalid email format.");
            return;
        }

        if (CurrentPage.name == "ResetPasswordStep1")
            StartCoroutine(ResetPasswordCoroutine(CurrentPage.transform));
        else
            StartCoroutine(ResetPasswordCoroutine(Canvas.Find("ResetPasswordStep1").transform));
    }

    /// <summary>
    /// Coroutine function to send the reset code by email </br>
    /// </summary>
    /// <param name="ResetPasswordPage">The page where the email is</param>
    private IEnumerator ResetPasswordCoroutine(Transform ResetPasswordPage)
    {
        yield return authController.RequestPasswordReset(ResetPasswordPage.Find("MailInputField").GetComponent<TMP_InputField>().text);

        if (authController.IsRequestSuccessful)
        {
            OpenPage("ResetPasswordStep2");
            OpenMessage("Password reset code sent to your email.");
        }
        else
        {
            OpenErr($"Password reset request failed: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Reset the password with the code sent by email (check fields) </br>
    /// </summary>
    public void ResetPassword2()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        if (CurrentPage.name != "ResetPasswordStep2")
            OpenPage("ResetPasswordStep2");

        TMP_InputField Password = CurrentPage.transform.Find("PasswordInputField1").GetComponent<TMP_InputField>();
        TMP_InputField ConfirmPassword = CurrentPage.transform.Find("PasswordInputField2").GetComponent<TMP_InputField>();

        FieldsValidation.ValidatePasswordField(Password, PasswordErrorTextReset);
        FieldsValidation.ValidateConfirmPasswordField(Password, ConfirmPassword, ConfirmationErrorTextReset);

        if (!InputCheckers.IsValidPassword(Password.text))
        {
            OpenErr("Password must contain at least 8 characters, 1 uppercase, 1 lowercase, 1 digit, and 1 special character.");
            return;
        }

        if (Password.text != ConfirmPassword.text)
        {
            OpenErr("Passwords do not match.");
            return;
        }

        StartCoroutine(ResetPassword2Coroutine(CurrentPage.transform));
    }

    /// <summary>
    /// Coroutine function to reset the password with the code sent by email </br>
    /// </summary>
    /// <param name="ResetPasswordPage2">The page where the code and new password are</param>
    private IEnumerator ResetPassword2Coroutine(Transform ResetPasswordPage2)
    {
        yield return authController.ResetPassword(Canvas.Find("ResetPasswordStep1").Find("MailInputField").GetComponent<TMP_InputField>().text,
                                                ResetPasswordPage2.Find("CodeInputField").GetComponent<TMP_InputField>().text,
                                                ResetPasswordPage2.Find("PasswordInputField1").GetComponent<TMP_InputField>().text,
                                                ResetPasswordPage2.Find("PasswordInputField2").GetComponent<TMP_InputField>().text);
        if (authController.IsRequestSuccessful)
        {
            OpenPage("PagePrincipale");
            OpenMessage("Password reset successfully.");
        }
        else
        {
            OpenErr($"Password reset failed: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Set the date of birth of the user who wants to register </br>
    /// </summary>
    public void SignUpUser1()
    {
        if (CurrentPage.name != "SignUpStep1")
            OpenPage("SignUpStep1");

        TMP_InputField DateInputField = FindDeepChild(CurrentPage.transform, "DateInputField")?.GetComponent<TMP_InputField>();
        if (DateInputField == null)
        {
            Debug.LogError("DateInputField not found in the hierarchy of SignUpStep1.");
            return;
        }

        if (string.IsNullOrEmpty(DateInputField.text))
        {
            OpenErr("Date is empty.");
            return;
        }

        string[] dateParts = DateInputField.text.Split('-');
        if (dateParts.Length != 3)
        {
            OpenErr("Invalid date format. Please use YYYY-MM-DD.");
            return;
        }

        string year = dateParts[0];
        string month = dateParts[1];
        string day = dateParts[2];

        if (!InputCheckers.IsValidDate(day, month, year))
        {
            OpenErr("Invalid date.");
            return;
        }

        if (!InputCheckers.IsValidAge(day, month, year))
        {
            OpenErr("You must be at least 13 years old to register.");
            return;
        }

        OpenPage("SignUpStep2");
    }

    /// <summary>
    /// Register the user with the username, email and password in the input fields (check fields) </br>
    /// </summary>
    public void SignUpUser2()
    {
        if (CurrentPage.name != "SignUpStep2")
            OpenPage("SignUpStep2");

        TMP_InputField FirstName = CurrentPage.transform.Find("FirstNameInputField").GetComponent<TMP_InputField>();
        TMP_InputField LastName = CurrentPage.transform.Find("LastNameInputField").GetComponent<TMP_InputField>();
        TMP_InputField UserName = CurrentPage.transform.Find("UserNameInputField").GetComponent<TMP_InputField>();
        TMP_InputField Email = CurrentPage.transform.Find("EmailInputField").GetComponent<TMP_InputField>();
        TMP_InputField Password = CurrentPage.transform.Find("PasswordInputField").GetComponent<TMP_InputField>();
        TMP_InputField ConfirmPassword = CurrentPage.transform.Find("ConfirmPasswordInputField").GetComponent<TMP_InputField>();

        ValidateEmailField(Email);
        FieldsValidation.ValidatePasswordField(Password, PasswordErrorText);
        FieldsValidation.ValidateConfirmPasswordField(Password, ConfirmPassword, ConfirmationErrorText);

        TMP_InputField DateInputField = FindDeepChild(Canvas.Find("SignUpStep1"), "DateInputField")?.GetComponent<TMP_InputField>();
        if (DateInputField == null)
        {
            Debug.LogError("DateInputField not found in the hierarchy of SignUpStep1.");
            return;
        }

        if (string.IsNullOrEmpty(FirstName.text) || string.IsNullOrEmpty(LastName.text) || string.IsNullOrEmpty(UserName.text) ||
            string.IsNullOrEmpty(Email.text) || string.IsNullOrEmpty(Password.text) || string.IsNullOrEmpty(ConfirmPassword.text) ||
            string.IsNullOrEmpty(DateInputField.text))
        {
            OpenErr("All fields are required.");
            return;
        }

        if (!InputCheckers.IsValidEmail(Email.text))
        {
            OpenErr("Invalid email format.");
            return;
        }

        if (Password.text != ConfirmPassword.text)
        {
            OpenErr("Passwords do not match.");
            return;
        }

        if (!InputCheckers.IsValidPassword(Password.text))
        {
            OpenErr("Password must contain at least 8 characters, 1 uppercase, 1 lowercase, 1 digit, and 1 special character.");
            return;
        }

        string[] dateParts = DateInputField.text.Split('-');
        if (dateParts.Length != 3)
        {
            OpenErr("Invalid date format. Please use YYYY-MM-DD.");
            return;
        }

        string year = dateParts[0];
        string month = dateParts[1];
        string day = dateParts[2];

        Debug.Log($"Parsed Date - Year: {year}, Month: {month}, Day: {day}");

        StartCoroutine(SignUpUser2Coroutine(FirstName, LastName, UserName, Email, Password, ConfirmPassword, day, month, year));
    }

    /// <summary>
    /// Coroutine function to register the user with the username, email and password in the input fields </br>
    /// </summary>
    /// <param name="FirstName">The first name of the user</param>, <param name="LastName">The last name of the user</param>,
    /// <param name="UserName">The username of the user</param>, <param name="Email">The email of the user</param>,
    /// <param name="Password">The password of the user</param>, <param name="ConfirmPassword">The confirmation password of the user</param>,
    /// <param name="day">The day of birth of the user</param>, <param name="month">The month of birth of the user</param>, <param name="year">The year of birth of the user</param>
    private IEnumerator SignUpUser2Coroutine(TMP_InputField FirstName, TMP_InputField LastName, TMP_InputField UserName,
                                            TMP_InputField Email, TMP_InputField Password, TMP_InputField ConfirmPassword,
                                            string day, string month, string year)
    {
        yield return authController.Register(UserName.text, Email.text, Password.text, ConfirmPassword.text,
                                            FirstName.text, LastName.text, day, month, year);
        if (authController.IsRequestSuccessful)
        {
            OpenPage("SignUpStep3");
            OpenMessage("Account created successfully. Please check your email for verification code.");
        }
        else
        {
            OpenErr($"Registration failed: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Verify the account with the code sent by email (check fields) </br>
    /// </summary>
    public void SignUpUser3()
    {
        // Check if on correct page
        if (CurrentPage.name != "SignUpStep3")
            OpenPage("SignUpStep3");

        TMP_InputField CheckCode = CurrentPage.transform.Find("CheckCodeInputField").GetComponent<TMP_InputField>();

        if (string.IsNullOrEmpty(CheckCode.text))
        {
            OpenErr("Code empty");
            return;
        }

        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(SignUpUser3Coroutine(CheckCode, Canvas.Find("SignUpStep2").Find("EmailInputField").GetComponent<TMP_InputField>()));
    }

    /// <summary>
    /// Coroutine function to verify the account with the code sent by email </br>
    /// </summary>
    /// <param name="CheckCode">The code sent by email</param>, <param name="Email">The email of the user</param>
    private IEnumerator SignUpUser3Coroutine(TMP_InputField CheckCode, TMP_InputField Email)
    {
        yield return authController.VerifyAccountButton(Email.text, CheckCode.text);
        if (authController.IsRequestSuccessful)
        {
            OpenPage("PagePrincipale");
            OpenMessage("Account verified successfully.");
        }
        else
        {
            OpenErr($"Account verification failed: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Resend the verification code to the email of the user </br>
    /// </summary>
    public void ResendVerificationCode()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(ResendVerificationCodeCoroutine(Canvas.Find("SignUpStep2").Find("EmailInputField").GetComponent<TMP_InputField>()));
    }

    /// <summary>
    /// Coroutine function to resend the verification code to the email of the user </br>
    /// </summary>
    /// <param name="Email">The email of the user</param>
    private IEnumerator ResendVerificationCodeCoroutine(TMP_InputField Email)
    {
        yield return authController.ResendVerificationCode(Email.text);
        if (authController.IsRequestSuccessful)
        {
            OpenMessage("Verification code resent successfully.");
        }
        else
        {
            OpenErr("Failed to resend verification code.");
        }
    }

    /// <summary>
    /// Call the logout coroutine </br>
    /// </summary>
    public void LogoutUser()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(LogoutCoroutine());
        
    }

    /// <summary>
    /// Coroutine function to logout the user </br>
    /// </summary>
    private IEnumerator LogoutCoroutine()
    {
        Debug.Log("Logging out...");
        yield return authController.Logout();

        if (authController.IsRequestSuccessful)
        {
            OpenMessage("You have been logged out successfully.");

            // Wait for popup to close:
            yield return new WaitUntil(() => !MessagePopup.activeSelf);
            
            // Load scene after closing popup:
            UnityEngine.SceneManagement.SceneManager.LoadScene("ConnectionMenu");
        }
        else
        {
            OpenErr($"Logout failed. Please try again: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Session expiry management: return to login page </br>
    /// </summary>
    public void HandleSessionExpired()
    {
        Debug.Log("Redirecting to login page...");
        OpenPage("PagePrincipale"); // Redirects to login page
    }

    /// <summary>
    /// Coroutine for calling the token expiration check function </br>
    /// </summary>
    private IEnumerator CheckAndRotateRefreshTokenCoroutine()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            yield break;
        }

        // Check if the refresh token is still valid for 1 day and rotate it if necessary (must re-login the next time it is used as the file will not be updated):
        yield return authController.CheckAndRotateRefreshToken();

        if (authController.IsRequestSuccessful)
        {
            Debug.Log("Refresh token rotated or maintained successfully.");
        }
        else
        {
            Debug.LogWarning($"Failed to rotate or maintained refresh token: {authController.ErrorResponse}");
            OpenErr($"Failed to rotate refresh token: {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Update the user statistics (games played, highest score, rank) </br>
    /// </summary>
    /// <param name="badge">The badge to update</param>
    private IEnumerator UpdateStatisticsCoroutine(Transform badge)
    {

        string found_badge  ; 
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            yield break;
        }

        yield return authController.UserStatistics();

        if (authController.IsRequestSuccessful && authController.CurrentUserStatistics != null)
        {

            if (badge != null)
            {
                badge.Find("GamePlayed").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.gamesPlayed.ToString();
                badge.Find("HighScore").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.highestScore.ToString();
                badge.Find("Rank").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.ranking.ToString();    
                badge.Find("UserName").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.username ;
            }

            else
            {
                Debug.LogWarning("Badge undefined");
            }
        }
        else
        {
            OpenErr($"Failed to retrieve user statistics: \n {authController.ErrorResponse}");
        }
    }

    /// <summary>
    /// Password field verification function on registration 
    /// </summary>
    public void ValidatePasswordRegistration()
    {
        Transform signUpStep2Page = Canvas.Find("SignUpStep2");
        if (signUpStep2Page == null)
        {
            Debug.LogError("Page 'SignUpStep2' not found.");
            return;
        }
        TMP_InputField passwordField = signUpStep2Page.Find("PasswordInputField")?.GetComponent<TMP_InputField>();
        if (passwordField == null)
        {
            Debug.LogError("PasswordInputField not found in 'SignUpStep2'.");
            return;
        }
        FieldsValidation.ValidatePasswordField(passwordField, PasswordErrorText);
    }

    /// <summary>
    /// Password field verification function on reset password
    /// </summary>
    public void ValidatePasswordReset()
    {
        Transform resetPasswordStep2Page = Canvas.Find("ResetPasswordStep2");
        if (resetPasswordStep2Page == null)
        {
            Debug.LogError("Page 'ResetPasswordStep2' not found.");
            return;
        }
        TMP_InputField passwordField = resetPasswordStep2Page.Find("PasswordInputField1")?.GetComponent<TMP_InputField>();
        if (passwordField == null)
        {
            Debug.LogError("PasswordInputField1 not found in 'ResetPasswordStep2'.");
            return;
        }
        FieldsValidation.ValidatePasswordField(passwordField, PasswordErrorTextReset);
    }

    /// <summary>
    /// Confirm password field verification function on registration
    /// </summary>
    public void ValidateConfirmPasswordRegistration()
    {
        Transform signUpStep2Page = Canvas.Find("SignUpStep2");
        if (signUpStep2Page == null)
        {
            Debug.LogError("Page 'SignUpStep2' not found.");
            return;
        }
        TMP_InputField passwordField = signUpStep2Page.Find("PasswordInputField")?.GetComponent<TMP_InputField>();
        TMP_InputField confirmPasswordField = signUpStep2Page.Find("ConfirmPasswordInputField")?.GetComponent<TMP_InputField>();
        if (passwordField == null || confirmPasswordField == null)
        {
            Debug.LogError("PasswordInputField or ConfirmPasswordInputField not found in 'SignUpStep2'.");
            return;
        }
        FieldsValidation.ValidateConfirmPasswordField(passwordField, confirmPasswordField, ConfirmationErrorText);
    }

    /// <summary>
    /// Confirm password field verification function on reset password
    /// </summary>
    public void ValidateConfirmPasswordReset()
    {
        Transform resetPasswordStep2Page = Canvas.Find("ResetPasswordStep2");
        if (resetPasswordStep2Page == null)
        {
            Debug.LogError("Page 'ResetPasswordStep2' not found.");
            return;
        }
        TMP_InputField passwordField = resetPasswordStep2Page.Find("PasswordInputField1")?.GetComponent<TMP_InputField>();
        TMP_InputField confirmPasswordField = resetPasswordStep2Page.Find("PasswordInputField2")?.GetComponent<TMP_InputField>();
        if (passwordField == null || confirmPasswordField == null)
        {
            Debug.LogError("PasswordInputField1 or PasswordInputField1 not found in 'ResetPasswordStep2'.");
            return;
        }
        FieldsValidation.ValidateConfirmPasswordField(passwordField, confirmPasswordField, ConfirmationErrorTextReset);
    }

    /// <summary>
    /// Email field verification function on registration
    /// </summary>
    public void ValidateEmailField(TMP_InputField emailField)
    {
        var outline = emailField.GetComponent<UnityEngine.UI.Outline>();

        if (outline != null)
        {
            if (string.IsNullOrEmpty(emailField.text) || !InputCheckers.IsValidEmail(emailField.text))
            {
                outline.enabled = true;
                EmailErrorText.SetActive(true);
            }
            else
            {
                outline.enabled = false;
                EmailErrorText.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Outline component not found on the input field.");
        }
    }

    /// <summary>
    /// Load the connection scene </br>
    /// </summary>
    public void ConnectionScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ConnectionMenu");
    }

    /// <summary>
    /// Load the main menu scene </br>
    /// </summary>
    public void MainMenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Load a random waiting room scene </br>
    /// </summary>
    public void ConnectToRandomWaitingRoom() {
        ConnectToWaitingRoom();
    }

    /// <summary>
    /// Load the waiting room scene </br>
    /// </summary>
    public void ConnectToWaitingRoom() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingRoom");
    }

    /// <summary>
    /// Rcursive search for a GameObject in the hierarchy by name </br>
    /// </summary>
    /// <param name="parent">The parent transform to search in</param>, <param name="childName">The name of the child to find</param>
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Show the tooltip for field validation </br>
    /// </summary>
    /// <param name="tooltip">The tooltip GameObject to show or hide</param>
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

    /// <summary>
    /// Hide the tooltip for field validation </br>
    /// </summary>
    /// <param name="tooltip">The tooltip GameObject to show or hide</param>
    public static void HideTooltip(GameObject tooltip)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false); // Hide the tooltip
        }
        else
        {
            Debug.LogWarning("Tooltip is not assigned in the inspector.");
        }
    }

    /// <summary>
    /// Call the coroutine to get the games played statistics </br>
    /// </summary>
    public void GamesPlayed()
    {
        StartCoroutine(GamesPlayedAndNavigate());
    }    
    
    /// <summary>
    /// Coroutine function to get the games played statistics </br>
    /// </summary>
    private IEnumerator GamesPlayedAndNavigate() {
        yield return authController.GamesPlayedStatistics();

        if (authController.IsRequestSuccessful) {
            OpenPage("ACHIEVEMENTS"); 
        } else {
            OpenMessage("You have not played a game yet. Start your epic journey now !"); 
        }
    }
}
