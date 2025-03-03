using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class BaseControll : MonoBehaviour
{
    public GameObject PagePrincipal,Reset_Your_Password,Page_sign_up_et1,Page_sign_up_et2,Page_sign_up_et3,Menu,Play,CreateRoom,JoinRoom, RoomNum,Err,credits,option,score,InGame;
    public TMP_InputField LogEmail,LogPW,ForMail,Month,Year,Day,Fname,Lname,Nname,Email,Mdp,Cmdp,code;
    public TMP_Text ErrText;
    public void OpenPagePrincipal()
    {
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
    }
    public void OpenReset_Your_Password()
    {
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
