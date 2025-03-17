using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System;

public class AuthController : MonoBehaviour
{
    [Header("UI Elements")]

    public TMP_Text responseText;

    private const string registerUrl = "https://185.155.93.105:17008/api/auth/register";
    private const string loginUrl = "https://185.155.93.105:17008/api/auth/login";
    private const string logoutUrl = "https://185.155.93.105:17008/api/auth/logout"; 
    private const string verifyUrl = "https://185.155.93.105:17008/api/auth/verify";
    private const string resendVerificationUrl = "https://185.155.93.105:17008/api/auth/resend-verification-code";
    private const string resetPasswordRequestUrl = "https://185.155.93.105:17008/api/auth/request-password-reset";
    private const string resetPasswordUrl = "https://185.155.93.105:17008/api/auth/reset-password";

    private const string userDataUrl = "https://185.155.93.105:17008/api/user/me";

    private static X509Certificate2 trustedCertificate;

    public bool IsRequestSuccessful { get; private set; }
    public UserData CurrentUser { get; private set; }

    private void Awake()
    {
        LoadCertificate();

    }

    private void LoadCertificate()
    {
        string certificatePath = "Assets/UI/certificat.pem";  
        if (File.Exists(certificatePath))
        {
            byte[] certificateBytes = File.ReadAllBytes(certificatePath);
            trustedCertificate = new X509Certificate2(certificateBytes);
            Debug.Log("Certificate successfully loaded.");
        }
        else
        {
            Debug.LogError("Certificate file not found! Ensure it's in the correct directory.");
        }
    }

    public static X509Certificate2 GetTrustedCertificate()
    {
        return trustedCertificate;
    }

    public IEnumerator Register(string username, string email, string password, string confirmPassword, string firstName, string lastName, string day, string month, string year)
    {
        yield return StartCoroutine(RegisterUser(username, email, password, confirmPassword, firstName, lastName, day, month, year));
    }

    public IEnumerator VerifyAccountButton(string email, string code)
    {
        yield return StartCoroutine(VerifyAccount(email, code));
    }

    public IEnumerator ResendVerificationCode(string email)
    {
        yield return StartCoroutine(ResendVerification(email));
    }

    public IEnumerator Login(string usernameOrEmail, string password)
    {
        yield return StartCoroutine(LoginUser(usernameOrEmail, password));
    }

    public IEnumerator User()
    {
        yield return StartCoroutine(GetCurrentUser());
    }

    public IEnumerator Logout()
    {
        yield return StartCoroutine(LogoutUser());
    }

    public IEnumerator RequestPasswordReset(string email)
    {
        yield return StartCoroutine(RequestPasswordResetCoroutine(email));
    }

    public IEnumerator ResetPassword(string email, string code, string newPassword, string confirmNewPassword)
    {
        yield return StartCoroutine(ResetPasswordCoroutine(email, code, newPassword, confirmNewPassword));
    }

    private IEnumerator RegisterUser(string username, string email, string password, string confirmPassword, string firstName, string lastName, string day, string month, string year)
    {
        Debug.Log(" REGISTERRRR");

        if (password != confirmPassword)
        {
            responseText.text = "Error: Passwords do not match.";
            Debug.LogError("Passwords do not match.");
            IsRequestSuccessful = false;
            yield break; 
        }

        string birthDate = FormatDateForApi(day, month, year);

        var requestData = new RegisterRequest
        {
            Username = username,
            Email = email,
            passwordHash = password,
            ConfirmPassword = confirmPassword, 
            FirstName = firstName,           
            LastName = lastName,              
            BirthDate = birthDate
        };

        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest(registerUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Registration SUCCESSFUL!";
            Debug.Log("Registration successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"Error: {request.error}";
            Debug.LogError("Registration FAILED: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private string FormatDateForApi(string day, string month, string year)
    {
        if (int.TryParse(day, out int dayInt) && int.TryParse(month, out int monthInt) && int.TryParse(year, out int yearInt))
        {
            DateTime parsedDate = new DateTime(yearInt, monthInt, dayInt);
            return parsedDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        else
        {
            Debug.LogError("Invalid date format.");
            return null;
        }
    }

    private IEnumerator VerifyAccount(string email, string code)
    {
        var requestData = new VerificationRequest
        {
            Email = email,
            Code = code
        };

        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest(verifyUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Verification SUCCESSFUL!";
            Debug.Log("Account verified successfully: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"Error: {request.error}";
            Debug.LogError("Verification failed: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator ResendVerification(string email)
    {
        var resendRequest = new ResendVerificationRequest
        {
            Email = email
        };

        string json = JsonUtility.ToJson(resendRequest);

        UnityWebRequest request = new UnityWebRequest(resendVerificationUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Verification code resent!";
            Debug.Log("Verification code resent: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"Error: {request.error}";
            Debug.LogError("Error resending code: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator LoginUser(string usernameOrEmail, string password)
    {
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        };

        string json = JsonUtility.ToJson(loginRequest);

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Login SUCCESSFUL!";
            Debug.Log("Login successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"ERROR: {request.error}";
            Debug.LogError("Login error: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator LogoutUser()
    {
        UnityWebRequest request = new UnityWebRequest(logoutUrl, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Logout SUCCESSFUL!";
            Debug.Log("Logout successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"ERROR: {request.error}";
            Debug.LogError("Logout error: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator RequestPasswordResetCoroutine(string email)
    {
        var resetRequest = new PasswordResetRequest
        {
            Email = email
        };

        string json = JsonUtility.ToJson(resetRequest);

        UnityWebRequest request = new UnityWebRequest(resetPasswordRequestUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Password reset code sent!";
            Debug.Log("Password reset code sent: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"Error: {request.error}";
            Debug.LogError("Password reset failed: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator ResetPasswordCoroutine(string email, string code, string newPassword, string confirmNewPassword)
    {
        if (newPassword != confirmNewPassword)
        {
            responseText.text = "Error: New passwords do not match.";
            Debug.LogError("New passwords do not match.");
            IsRequestSuccessful = false;
            yield break;
        }

        var resetRequest = new ResetPasswordRequest
        {
            Email = email,
            Code = code ,
            ConfirmPassword = confirmNewPassword,
            NewPassword = newPassword
        };

        string json = JsonUtility.ToJson(resetRequest);

        UnityWebRequest request = new UnityWebRequest(resetPasswordUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "Password reset successful!";
            Debug.Log("Password reset successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"Error: {request.error}";
            Debug.LogError("Password reset error: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator GetCurrentUser()
    {
        UnityWebRequest request = new UnityWebRequest(userDataUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            responseText.text = "User data retrieved successfully!";
            Debug.Log("User Data: " + request.downloadHandler.text);
            // Parse user data
            var userData = JsonUtility.FromJson<UserData>(request.downloadHandler.text);
            CurrentUser = userData; 

            IsRequestSuccessful = true;
        }
        else
        {
            responseText.text = $"Error: {request.error}";
            Debug.LogError("Failed to retrieve user data: " + request.error);
            Debug.LogError("Details: " + request.downloadHandler.text);
            IsRequestSuccessful = false;
        }
    }
}

public class CustomCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        X509Certificate2 trustedCert = AuthController.GetTrustedCertificate();
        if (trustedCert == null)
        {
            Debug.LogError("No trusted certificate loaded.");
            return false;
        }

        X509Certificate2 receivedCertificate = new X509Certificate2(certificateData);
        return receivedCertificate.Equals(trustedCert);
    }
}

[System.Serializable]
public class RegisterRequest
{
    public string Username;
    public string Email;
    public string passwordHash; 
    public string ConfirmPassword;
    public string FirstName;
    public string LastName;
    public string BirthDate;
}

[System.Serializable]
public class LoginRequest
{
    public string UsernameOrEmail;
    public string Password;
}

[System.Serializable]
public class PasswordResetRequest
{
    public string Email;
}

[System.Serializable]
public class ResetPasswordRequest
{
    public string Email;
    public string Code;
    public string NewPassword;
    public string ConfirmPassword;

}

[System.Serializable]
public class VerificationRequest
{
    public string Email;
    public string Code;
}

[Serializable]
public class ResendVerificationRequest
{
    public string Email;
}

[System.Serializable]
public class UserData
{
    public string username;
    public string email;
    public string passwordHash;
    public string createdAt;
    public string isVerified;
    public string firstName;
    public string lastName;
    public string birthDate;
    public string passwordResetToken;
    public string passwordResetExpiration;
}
