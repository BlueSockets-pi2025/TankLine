using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class BaseControll : MonoBehaviour
{
    public GameObject PagePrincipal, Reset_Your_Password, Reset_Your_Password2, Page_sign_up_et1, Page_sign_up_et2, Page_sign_up_et3, Menu, Play, CreateRoom, JoinRoom, RoomNum, Err, Msg, credits, option, score, InGame, WaintingRoom, ColorChangingPannel, lose, win;
    public TMP_InputField LogUsername, LogPW, ForMail, Month, Year, Day, Fname, Lname, Nname, Email, Mdp, Cmdp, code;
    public TMP_InputField Re_Mdp, Re_Cmdp, Re_code;
    public TMP_Text ErrText;
    public TMP_Text MsgText;
    public TMP_Text nicknameText, MenuBadge, GPMText, HSMText, RKMText, GPPText, HSPText, RKPText ;

    private AuthController authController;

    private void Start()
    {
        OpenPagePrincipal();
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
        /*
        TO DO 
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

        if (authController != null && authController.CurrentUser != null && authController.CurrentUserStatistics != null)
        {
            MenuBadge.text = authController.CurrentUser.username;   
        }

        StartCoroutine(UpdateStatisticsCoroutine("Menu"));
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

        if (authController != null && authController.CurrentUser != null && authController.CurrentUserStatistics != null)
        {
            nicknameText.text = authController.CurrentUser.username;
        }
        
        StartCoroutine(UpdateStatisticsCoroutine("Play"));
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
                yield return authController.UserStatistics(); 
                if (authController.IsRequestSuccessful)
                {
                    OpenMenu();
                }
                else
                {
                    OpenErr("Failed to retrieve user statistics.");
                }
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

        if (!IsValidPassword(Re_Mdp.text))
        {
            OpenErr("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
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

        if (!IsValidDate(Day.text, Month.text, Year.text))
        {
            OpenErr("Invalid date.");
            return;
        }

        if (!IsValidAge(Day.text, Month.text, Year.text))
        {
            OpenErr("You must be at least 12 years old to register.");
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
    
        if (!IsValidPassword(Mdp.text))
        {
            OpenErr("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
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

    private bool IsValidPassword(string password)
    {
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        return Regex.IsMatch(password, pattern);
    }

    private bool IsValidDate(string day, string month, string year)
    {
        int dayInt, monthInt, yearInt;
        if (int.TryParse(day, out dayInt) && int.TryParse(month, out monthInt) && int.TryParse(year, out yearInt))
        {
            try
            {
                DateTime date = new DateTime(yearInt, monthInt, dayInt);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    private bool IsValidAge(string day, string month, string year)
    {
        int dayInt, monthInt, yearInt;
        if (int.TryParse(day, out dayInt) && int.TryParse(month, out monthInt) && int.TryParse(year, out yearInt))
        {
            try
            {
                DateTime birthDate = new DateTime(yearInt, monthInt, dayInt);
                DateTime today = DateTime.Today;
                int age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;
                return age >= 12;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    private IEnumerator UpdateStatisticsCoroutine(string context)
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            yield break;
        }

        yield return authController.UserStatistics();

        if (authController.IsRequestSuccessful && authController.CurrentUserStatistics != null)
        {
            // Met à jour les statistiques en fonction du contexte
            if (context == "Play")
            {
                GPPText.text = authController.CurrentUserStatistics.gamesPlayed.ToString();
                HSPText.text = authController.CurrentUserStatistics.highestScore.ToString();
                RKPText.text = authController.CurrentUserStatistics.ranking.ToString();
            }
            else if (context == "Menu")
            {
                GPMText.text = authController.CurrentUserStatistics.gamesPlayed.ToString();
                HSMText.text = authController.CurrentUserStatistics.highestScore.ToString();
                RKMText.text = authController.CurrentUserStatistics.ranking.ToString();
            }
            else
            {
                Debug.LogWarning("Unknown context provided to UpdateStatisticsCoroutine.");
            }
        }
        else
        {
            OpenErr("Failed to retrieve user statistics.");
        }
    }

    public void LogoutUser()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        yield return authController.Logout();

        if (authController.IsRequestSuccessful)
        {
            OpenMessage("You have been logged out successfully.");
        }
        else
        {
            OpenErr("Logout failed. Please try again.");
        }
    }
}