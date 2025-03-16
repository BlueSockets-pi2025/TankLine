using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class BaseControll : MonoBehaviour
{
    public GameObject PagePrincipal, Reset_Your_Password, Reset_Your_Password2, Page_sign_up_et1, Page_sign_up_et2, Page_sign_up_et3, Menu, Play, CreateRoom, JoinRoom, RoomNum, Err, Msg, credits, option, score, InGame, WaintingRoom, ColorChangingPannel, lose, win;
    public TMP_InputField LogUsername, LogEmail, LogPW, ForMail, Month, Year, Day, Fname, Lname, Nname, Email, Mdp, Cmdp, code;
    public TMP_InputField Re_Mdp, Re_Cmdp, Re_code;
    public TMP_Text ErrText;
    public TMP_Text MsgText;
    public TMP_Text nicknameText, MenuBadge;

    private AuthController authController;

    private void Start()
    {
        authController = GetComponent<AuthController>();
        if (authController == null)
        {
            Debug.LogError("AuthController is not attached to the same GameObject.");
            return;
        }
        // AutoLogin();
    }

    private void AutoLogin()
    {
        /*string savedEmail = PlayerPrefs.GetString("LoggedInEmail", "");
        string savedPassword = PlayerPrefs.GetString("LoggedInPassword", "");

        if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
        {
            UserDatabase userDB = UserDatabase.LoadUsers();
            if (userDB.ValidateUser(savedEmail, savedPassword))
            {
                nicknameText.text = userDB.GetNickname(savedEmail);
                MenuBadge.text = nicknameText.text;
                OpenMenu();
                return;
            }
        }
        LogEmail.text = savedEmail;
        */
    }

    public void OpenPagePrincipal()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(true);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenReset_Your_Password()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(true);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenReset_Your_Password2()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(true);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenPage_sign_up_et1()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(true);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenPage_sign_up_et2()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(true);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenPage_sign_up_et3()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(true);
    }

    public void OpenMenu()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(true);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);

        if (authController != null && authController.CurrentUser != null)
        {
            nicknameText.text = authController.CurrentUser.username;
            MenuBadge.text = authController.CurrentUser.username;
        }
    }

    public void OpenPlay()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(true);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenCreateRoom()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(true);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenJoinRoom()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(true);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenCredits()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(true);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenOption()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(true);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenScore()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(true);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenInGame()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(false);
        InGame.SetActive(true);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenWaitingRoom()
    {
        win.SetActive(false);
        lose.SetActive(false);
        WaintingRoom.SetActive(true);
        InGame.SetActive(false);
        option.SetActive(false);
        score.SetActive(false);
        credits.SetActive(false);
        JoinRoom.SetActive(false);
        CreateRoom.SetActive(false);
        Menu.SetActive(false);
        Play.SetActive(false);
        PagePrincipal.SetActive(false);
        Reset_Your_Password.SetActive(false);
        Reset_Your_Password2.SetActive(false);
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
    }

    public void OpenColorChanger()
    {
        ColorChangingPannel.SetActive(true);
    }

    public void CloseColorChanger()
    {
        ColorChangingPannel.SetActive(false);
    }

    public void OpenRoomNumber()
    {
        RoomNum.SetActive(true);
    }

    public void CloseRoomNumber()
    {
        RoomNum.SetActive(false);
    }

    public void OpenErr(string message)
    {
        ErrText.text = message;
        Err.SetActive(true);
    }

    public void CloseErr()
    {
        Err.SetActive(false);
    }

    public void OpenMessage(string message)
    {
        MsgText.text = message;
        Msg.SetActive(true);
    }

    public void CloseMessage()
    {
        Msg.SetActive(false);
    }


    public void LoginUser()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(LoginUserCoroutine());
    }

    private IEnumerator LoginUserCoroutine()
    {
        yield return authController.Login(LogUsername.text, LogPW.text);
        if (authController.IsRequestSuccessful)
        {
            yield return authController.User();
            if (authController.IsRequestSuccessful)
            {
                OpenMenu();
            }
            else
            {
                OpenErr("Failed to retrieve user data.");
            }
        }
        else
        {
            OpenErr("Login failed.");
        }
    }

    public void LogoutUser()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        authController.Logout();
    }

    public void ResetPassword()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(ResetPasswordCoroutine());
    }

    private IEnumerator ResetPasswordCoroutine()
    {
        yield return authController.RequestPasswordReset(ForMail.text);
        if (authController.IsRequestSuccessful)
        {
            OpenReset_Your_Password2();
            OpenMessage("Password reset code sent to your email.");
        }
        else
        {
            OpenErr("Password reset request failed.");
        }
    }

    public void ResetPassword2()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(ResetPassword2Coroutine());
    }

    private IEnumerator ResetPassword2Coroutine()
    {
        yield return authController.ResetPassword(ForMail.text, Re_code.text, Re_Mdp.text, Re_Cmdp.text);
        if (authController.IsRequestSuccessful)
        {
            OpenPagePrincipal();
            OpenMessage("Password reset successfully.");
        }
        else
        {
            OpenErr("Password reset failed.");
        }
    }

    public void SigninUser1()
    {
        if (string.IsNullOrEmpty(Month.text) || string.IsNullOrEmpty(Year.text) || string.IsNullOrEmpty(Day.text))
        {
            OpenErr("Date empty");
            return;
        }
        OpenPage_sign_up_et2();
    }

    public void SigninUser2()
    {
        if (string.IsNullOrEmpty(Fname.text) || string.IsNullOrEmpty(Lname.text) || string.IsNullOrEmpty(Nname.text) ||
            string.IsNullOrEmpty(Email.text) || string.IsNullOrEmpty(Mdp.text) || string.IsNullOrEmpty(Cmdp.text))
        {
            OpenErr("All fields are required.");
            return;
        }

        if (!IsValidEmail(Email.text))
        {
            OpenErr("Invalid email format.");
            return;
        }

        if (Mdp.text != Cmdp.text)
        {
            OpenErr("Passwords do not match.");
            return;
        }

        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(SigninUser2Coroutine());
    }

    private IEnumerator SigninUser2Coroutine()
    {
        yield return authController.Register(Nname.text, Email.text, Mdp.text, Cmdp.text, Fname.text, Lname.text, Day.text, Month.text, Year.text);
        if (authController.IsRequestSuccessful)
        {
            OpenPage_sign_up_et3();
            OpenMessage("Account created successfully. Please check your email for verification code.");
        }
        else
        {
            OpenErr("Registration failed.");
        }
    }

    public void SigninUser3()
    {
        if (string.IsNullOrEmpty(code.text))
        {
            OpenErr("Code empty");
            return;
        }

        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(SigninUser3Coroutine());
    }

    private IEnumerator SigninUser3Coroutine()
    {
        yield return authController.VerifyAccountButton(Email.text, code.text);
        if (authController.IsRequestSuccessful)
        {
            OpenPagePrincipal();
            OpenMessage("Account verified successfully.");
        }
        else
        {
            OpenErr("Account verification failed.");
        }
    }

    public void ResendVerificationCode()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(ResendVerificationCodeCoroutine());
    }

    private IEnumerator ResendVerificationCodeCoroutine()
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

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }
}