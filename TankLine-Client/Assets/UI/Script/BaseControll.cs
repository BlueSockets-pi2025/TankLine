using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class BaseControll : MonoBehaviour
{
    public GameObject PagePrincipal,Reset_Your_Password,Page_sign_up_et1,Page_sign_up_et2,Page_sign_up_et3,Menu,Play,CreateRoom,JoinRoom, RoomNum,Err,credits,option,score,InGame,WaintingRoom,ColorChangingPannel,lose,win;
    public TMP_InputField LogEmail,LogPW,ForMail,Month,Year,Day,Fname,Lname,Nname,Email,Mdp,Cmdp,code;
    public TMP_Text ErrText;
    private void Start()
{
    AutoLogin();
}

private void AutoLogin()
{
    string savedEmail = PlayerPrefs.GetString("LoggedInEmail", "");
    string savedPassword = PlayerPrefs.GetString("LoggedInPassword", "");

    if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
    {
        UserDatabase userDB = UserDatabase.LoadUsers();
        if (userDB.ValidateUser(savedEmail, savedPassword))
        {
            OpenMenu(); 
            return;
        }
    }
    LogEmail.text = savedEmail;
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
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
        LogoutUser();
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
        Page_sign_up_et1.SetActive(false);
        Page_sign_up_et2.SetActive(false);
        Page_sign_up_et3.SetActive(false);
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
    public void CloseErr(){
        Err.SetActive(false);
    }
    public void LogoutUser()
{
    PlayerPrefs.DeleteKey("LoggedInEmail");
    PlayerPrefs.DeleteKey("LoggedInPassword");
    PlayerPrefs.Save();
}

    public void LoginUser()
{
    if (string.IsNullOrEmpty(LogEmail.text) || string.IsNullOrEmpty(LogPW.text))
    {
        OpenErr("Email or password cannot be empty.");
        return;
    }

    if (!IsValidEmail(LogEmail.text))
    {
        OpenErr("Invalid email format.");
        return;
    }

    UserDatabase userDB = UserDatabase.LoadUsers();
    if (!userDB.ValidateUser(LogEmail.text, LogPW.text))
    {
        OpenErr("Invalid email or password.");
        return;
    }

    // Save login data locally
    PlayerPrefs.SetString("LoggedInEmail", LogEmail.text);
    PlayerPrefs.SetString("LoggedInPassword", LogPW.text);
    PlayerPrefs.Save();

    OpenMenu();
}



    public void ResetPassword()
    {
        if (string.IsNullOrEmpty(ForMail.text))
        {
            OpenErr("Email field cannot be empty.");
            return;
        }

        if (!IsValidEmail(ForMail.text))
        {
            OpenErr("Invalid email format.");
            return;
        }
        OpenPagePrincipal();
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

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
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

    // Load user database
    UserDatabase userDB = UserDatabase.LoadUsers();

    // Check if email is already registered
    if (userDB.UserExists(Email.text))
    {
        OpenErr("Email already exists. Please log in.");
        return;
    }

    // Create new user and save to JSON
    User newUser = new User
    {
        firstName = Fname.text,
        lastName = Lname.text,
        nickname = Nname.text,
        email = Email.text,
        password = Mdp.text
    };

    userDB.AddUser(newUser);

    OpenPage_sign_up_et3();
}

    public void SigninUser3()
    {
        if (string.IsNullOrEmpty(code.text))
        {
            OpenErr("Code empty");
            return;
        }
        OpenPagePrincipal();
    }
    
}
