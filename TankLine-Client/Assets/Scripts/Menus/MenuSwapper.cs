using System.Collections;
using TMPro;
using UnityEngine;

public class MenuSwapper : MonoBehaviour {
    public static MenuSwapper Instance { get; private set; }
    private AuthController authController;


    public GameObject _canvas;
    private Transform Canvas;
    public GameObject ErrorPopup, MessagePopup;

    public GameObject CurrentPage;

    void Awake()
    {
        // ensure there is only one instance of this script
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Canvas = _canvas.transform;
    }

    private void Start() {
        authController = GetComponent<AuthController>();
        if (authController == null) {
            Debug.LogError("AuthController not found.");
            return;
        }
        AutoLogin();

        // load first page
        OpenPage(CurrentPage.name);

        //OpenPagePrincipal();
    }

    private void AutoLogin() {
        StartCoroutine(AutoLoginCoroutine());
    }

    /// <summary>
    /// Coroutine function to log in
    /// </summary>
    private IEnumerator AutoLoginCoroutine() {
        Debug.Log("Attempting auto-login...");

        // Request requiring an access token to check if the user is already logged in and set the current user 
        yield return authController.User();
        if (authController.IsRequestSuccessful)
        {
            Debug.Log("Auto-login successful. Skipping login page.");
            OpenPage("MainMenu"); // Go directly to the main page
        }
        else
        {
            Debug.Log("Auto-login failed. Redirecting to login page.");
            OpenPage("PagePrincipale"); // Connection page 
        }
    }

    /// <summary>
    /// Open the page "pageName" in the canvas, and close every other page </br>
    /// The page must be a direct child of the main canvas
    /// </summary>
    /// <param name="pageName">The name of the page</param>
    public void OpenPage(string pageName) {

        // disable old page
        CurrentPage.SetActive(false);

        // find new page and enable it
        CurrentPage = Canvas.Find(pageName).gameObject;

        if (CurrentPage == null) {
            Debug.LogError($"Page not found : {pageName}");
            return;
        }

        CurrentPage.SetActive(true);

        // if switch to mainMenu or play, load stats & name
        if (pageName == "MainMenu" || pageName == "Play") {   
            StartCoroutine(UpdateStatisticsCoroutine(CurrentPage.transform.Find("Badge")));
        }
    }

    public void OpenErr(string message) {
        ErrorPopup.SetActive(true);
        ErrorPopup.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = message; // change popup main text
    }
    public void CloseErr() {
        ErrorPopup.SetActive(false);
    }

    public void OpenMessage(string message) {
        MessagePopup.SetActive(true);
        MessagePopup.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = message; // change popup main text
    }
    public void CloseMessage() {
        MessagePopup.SetActive(false);
    }

    public void LoginUser() {
        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        Transform page = CurrentPage.name == "PagePrincipale" ? CurrentPage.transform : Canvas.Find("PagePrincipale").transform;
        string username = page.Find("LogUsername").GetComponent<TMP_InputField>().text;
        string password = page.Find("LogPassword").GetComponent<TMP_InputField>().text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            OpenErr("Both username and password fields must be filled.");
            return;
        }

        if (CurrentPage.name == "PagePrincipale")
            StartCoroutine(LoginUserCoroutine(CurrentPage.transform));
        else 
            StartCoroutine(LoginUserCoroutine(Canvas.Find("PagePrincipale").transform));

    }

    private IEnumerator LoginUserCoroutine(Transform PagePrincipale) {

        // Find username and password input field
        yield return authController.Login(PagePrincipale.Find("LogUsername").GetComponent<TMP_InputField>().text, 
                                        PagePrincipale.Find("LogPassword").GetComponent<TMP_InputField>().text);

        if (authController.IsRequestSuccessful) {
            yield return authController.User();

            if (authController.IsRequestSuccessful) {
                yield return authController.UserStatistics(); 

                if (authController.IsRequestSuccessful) {
                    OpenPage("MainMenu");
                } else {
                    OpenErr($"Failed to retrieve user statistics:\n {authController.ErrorResponse}");
                }
            } else {
                OpenErr($"Failed to retrieve user data: \n {authController.ErrorResponse}");
            }
        } else {
            OpenErr($"Login failed: \n {authController.ErrorResponse}");
        }
    }

    public void ResetPassword() {
        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        Transform resetPasswordPage = CurrentPage.name == "ResetPasswordStep1" ? CurrentPage.transform : Canvas.Find("ResetPasswordStep1").transform;
        string email =  resetPasswordPage.Find("MailInputField").GetComponent<TMP_InputField>().text;

        if (!InputCheckers.IsValidEmail(email)) {
            OpenErr("Invalid email format.");
            return;
        }

        if (CurrentPage.name == "ResetPasswordStep1")
            StartCoroutine(ResetPasswordCoroutine(CurrentPage.transform));
        else
            StartCoroutine(ResetPasswordCoroutine(Canvas.Find("ResetPasswordStep1").transform));
    }

    private IEnumerator ResetPasswordCoroutine(Transform ResetPasswordPage) {
        yield return authController.RequestPasswordReset(ResetPasswordPage.Find("MailInputField").GetComponent<TMP_InputField>().text);

        if (authController.IsRequestSuccessful) {
            OpenPage("ResetPasswordStep2");
            OpenMessage("Password reset code sent to your email.");
        } else {
            OpenErr($"Password reset request failed: \n {authController.ErrorResponse}");
        }
    }

    public void ResetPassword2() {
        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        if (CurrentPage.name != "ResetPasswordStep2")
            OpenPage("ResetPasswordStep2");

        TMP_InputField Password = CurrentPage.transform.Find("PasswordInputField1").GetComponent<TMP_InputField>();
        TMP_InputField ConfirmPassword = CurrentPage.transform.Find("PasswordInputField2").GetComponent<TMP_InputField>();

        ValidatePasswordField(Password);
        ValidatePasswordField(ConfirmPassword);
        ValidateConfirmPasswordField(Password, ConfirmPassword);

        if (!InputCheckers.IsValidPassword(Password.text)) {
            OpenErr("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            return;
        }

        if (Password.text != ConfirmPassword.text)
        {
            OpenErr("Passwords do not match.");
            return;
        }

        StartCoroutine(ResetPassword2Coroutine(CurrentPage.transform));
    }

    private IEnumerator ResetPassword2Coroutine(Transform ResetPasswordPage2)
    {
        yield return authController.ResetPassword(Canvas.Find("ResetPasswordStep1").Find("MailInputField").GetComponent<TMP_InputField>().text,
                                                ResetPasswordPage2.Find("CodeInputField").GetComponent<TMP_InputField>().text,
                                                ResetPasswordPage2.Find("PasswordInputField1").GetComponent<TMP_InputField>().text,
                                                ResetPasswordPage2.Find("PasswordInputField2").GetComponent<TMP_InputField>().text);
        if (authController.IsRequestSuccessful) {
            OpenPage("PagePrincipale");
            OpenMessage("Password reset successfully.");
        }
        else
        {
            OpenErr($"Password reset failed: \n {authController.ErrorResponse}");
        }
    }

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
        ValidatePasswordField(Password);
        ValidatePasswordField(ConfirmPassword);
        ValidateConfirmPasswordField(Password, ConfirmPassword);

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
            OpenErr("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
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

      public void ValidateEmailField(TMP_InputField emailField) {
        var outline = emailField.GetComponent<UnityEngine.UI.Outline>();

        if (outline != null) {
            if (string.IsNullOrEmpty(emailField.text) || !InputCheckers.IsValidEmail(emailField.text)) {
                outline.enabled = true; 
            } else {
                outline.enabled = false;
            }
        } else {
            Debug.LogWarning("Outline component not found on the input field.");
        }
    }
        
    public void ValidatePasswordField(TMP_InputField passwordField) {
        var outline = passwordField.GetComponent<UnityEngine.UI.Outline>();

        if (outline != null) {
            if (string.IsNullOrEmpty(passwordField.text) || !InputCheckers.IsValidPassword(passwordField.text)) {
                outline.enabled = true; 
            } else {
                outline.enabled = false; 
            }
        } else {
            Debug.LogWarning("Outline component not found on the input field.");
        }
    }

    public void ValidateConfirmPasswordField(TMP_InputField passwordField, TMP_InputField confirmPasswordField) {
        var outline = confirmPasswordField.GetComponent<UnityEngine.UI.Outline>();

        if (outline != null) {
            if (passwordField.text != confirmPasswordField.text) {
                outline.enabled = true;
            } else {
                outline.enabled = false; 
            }
        } else {
            Debug.LogWarning("Outline component not found on the confirm password field.");
        }
    }

    public void ValidateConfirmPasswordRegistration() {
        Transform signUpStep2Page = Canvas.Find("SignUpStep2");

        if (signUpStep2Page == null) {
            Debug.LogError("Page 'SignUpStep2' not found.");
            return;
        }

        TMP_InputField passwordField = signUpStep2Page.Find("PasswordInputField")?.GetComponent<TMP_InputField>();
        TMP_InputField confirmPasswordField = signUpStep2Page.Find("ConfirmPasswordInputField")?.GetComponent<TMP_InputField>();

        if (passwordField == null || confirmPasswordField == null) {
            Debug.LogError("PasswordInputField or ConfirmPasswordInputField not found in 'SignUpStep2'.");
            return;
        }

        ValidateConfirmPasswordField(passwordField, confirmPasswordField);
    }

    public void ValidateConfirmPasswordReset() {
        Transform signUpStep2Page = Canvas.Find("ResetPasswordStep2");

        if (signUpStep2Page == null) {
            Debug.LogError("Page 'ResetPasswordStep2' not found.");
            return;
        }

        TMP_InputField passwordField = signUpStep2Page.Find("PasswordInputField1")?.GetComponent<TMP_InputField>();
        TMP_InputField confirmPasswordField = signUpStep2Page.Find("PasswordInputField2")?.GetComponent<TMP_InputField>();

        if (passwordField == null || confirmPasswordField == null) {
            Debug.LogError("PasswordInputField1 or PasswordInputField1 not found in 'ResetPasswordStep2'.");
            return;
        }

        ValidateConfirmPasswordField(passwordField, confirmPasswordField);
    }

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

    public void SignUpUser3() {
        // Check if on correct page
        if (CurrentPage.name != "SignUpStep3")
            OpenPage("SignUpStep3");
        
        TMP_InputField CheckCode = CurrentPage.transform.Find("CheckCodeInputField").GetComponent<TMP_InputField>();

        if (string.IsNullOrEmpty(CheckCode.text)) {
            OpenErr("Code empty");
            return;
        }

        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(SignUpUser3Coroutine(CheckCode, Canvas.Find("SignUpStep2").Find("EmailInputField").GetComponent<TMP_InputField>()));
    }

    private IEnumerator SignUpUser3Coroutine(TMP_InputField CheckCode, TMP_InputField Email) {
        yield return authController.VerifyAccountButton(Email.text, CheckCode.text);
        if (authController.IsRequestSuccessful) {
            OpenPage("PagePrincipale");
            OpenMessage("Account verified successfully.");
        } else {
            OpenErr($"Account verification failed: \n {authController.ErrorResponse}");
        }
    }

    public void ResendVerificationCode() {
        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(ResendVerificationCodeCoroutine(Canvas.Find("SignUpStep2").Find("EmailInputField").GetComponent<TMP_InputField>()));
    }

    private IEnumerator ResendVerificationCodeCoroutine(TMP_InputField Email) {
        yield return authController.ResendVerificationCode(Email.text);
        if (authController.IsRequestSuccessful) {
            OpenMessage("Verification code resent successfully.");
        } else {
            OpenErr("Failed to resend verification code.");
        }
    }

    public void LogoutUser() {
        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine() {
        yield return authController.Logout();

        if (authController.IsRequestSuccessful) {
            OpenMessage("You have been logged out successfully.");
        } else {
            OpenErr($"Logout failed. Please try again: \n {authController.ErrorResponse}");
        }
    }

    public void HandleSessionExpired() {
        Debug.Log("Redirecting to login page...");
        OpenPage("PagePrincipale"); // Redirects to login page
    }

    private IEnumerator UpdateStatisticsCoroutine(Transform badge) {
        if (authController == null) {
            Debug.LogError("AuthController is not initialized.");
            yield break;
        }

        yield return authController.UserStatistics();

        if (authController.IsRequestSuccessful && authController.CurrentUserStatistics != null) {
            if (badge != null) {
                badge.Find("GamePlayed").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.gamesPlayed.ToString();
                badge.Find("HighScore").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.highestScore.ToString();
                badge.Find("Rank").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.ranking.ToString();
                badge.Find("UserName").GetComponent<TMP_Text>().text = authController.CurrentUser.username;

            } else {
                Debug.LogWarning("Badge undefined");
            }
        } else {
            OpenErr($"Failed to retrieve user statistics: \n {authController.ErrorResponse}");
        }
    }

    public void ConnectToRandomWaitingRoom() {
        ConnectToWaitingRoom();
    }

    public void ConnectToWaitingRoom() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingRoom");
    }

    // Recursive search for a GameObject by name
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


    public void GamesPlayed()
    {
        StartCoroutine(GamesPlayedAndNavigate());
    }    
    
    private IEnumerator GamesPlayedAndNavigate() {
        yield return authController.GamesPlayedStatistics();

        if (authController.IsRequestSuccessful) {
            OpenPage("ACHIEVEMENTS"); 
        } else {
            OpenMessage("You have not played a game yet. Start your epic journey now !"); 
        }
    }




}
