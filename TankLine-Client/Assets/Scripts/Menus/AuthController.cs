using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System;
using System.Timers;

using Heartbeat ; 



public class AuthController : MonoBehaviour
{
    [Header("UI Elements")]

    private const string registerUrl = "https://185.155.93.105:17008/api/auth/register";
    private const string loginUrl = "https://185.155.93.105:17008/api/auth/login";
    private const string logoutUrl = "https://185.155.93.105:17008/api/auth/logout"; 
    private const string verifyUrl = "https://185.155.93.105:17008/api/auth/verify";
    private const string resendVerificationUrl = "https://185.155.93.105:17008/api/auth/resend-verification-code";
    private const string resetPasswordRequestUrl = "https://185.155.93.105:17008/api/auth/request-password-reset";
    private const string resetPasswordUrl = "https://185.155.93.105:17008/api/auth/reset-password";

    private const string userDataUrl = "https://185.155.93.105:17008/api/user/me";
    private const string userStatisticsUrl = "https://185.155.93.105:17008/api/user/me/statistics";

    private const string refreshTokenUrl = "https://185.155.93.105:17008/api/auth/refresh-token";

    private string playedGameUrl = "https://185.155.93.105:17008/api/PlayedGames/summary"; 

    private string heartbeatUrl = "https://185.155.93.105:17008/api/auth/heartbeat" ; 


    private static AuthController instance;


    private static X509Certificate2 trustedCertificate;  

    public bool IsRequestSuccessful { get; private set; }
    public string ErrorResponse { get; private set; }
    public UserData CurrentUser { get; private set; }
    public UserStatistics CurrentUserStatistics { get; private set; }

    public PlayedGameStats CurrentPlayedGameStats { get; private set; }



    // Heartbeat (ping) to update is_logged_in 
    private float timeSinceLastHeartbeat = 0f;
    private float heartbeatInterval = 30f;
    private bool isLoggedIn = false;  

    // UI Elements (Leaderboard)
    public TMP_Text map;
    public TMP_Text victories;
    public TMP_Text username;
    public TMP_Text ratio;
    public TMP_Text tanks_destroyed;
    public TMP_Text nb_games_played;
    public TMP_Text victory_or_defeat;
    public TMP_Text rank;
    public TMP_Text date;

    
    private static X509Certificate2 staticTrustedCertificate;



    private void OnApplicationQuit()
    {
        
        StartCoroutine(LogoutUser());
    }

    



    private IEnumerator HeartbeatTimer()
    {
            Debug.Log("HEARTBEAT TRIGGERED...");
            while (true)
            {
                // Attend 5 secondes
                yield return new WaitForSeconds(5f);

                
                Debug.Log("HeartbeatTimer Is Logged In: " + isLoggedIn);
                
                if (isLoggedIn)
                {
                    Debug.Log("Starting SendHeartbeat coroutine...");
                    yield return StartCoroutine(SendHeartbeat());
                }
            }
        }





    private void Awake()
    {
        LoadCertificate();

    }
    
    private void Update()
    {
        // On vérifie si l'intervalle est écoulé
        timeSinceLastHeartbeat += Time.deltaTime;

        if (timeSinceLastHeartbeat >= heartbeatInterval)
        {
            // Réinitialiser le timer et appeler la coroutine
            timeSinceLastHeartbeat = 0f;

            // Lancer la coroutine pour envoyer un heartbeat
            StartCoroutine(SendHeartbeat());
        }
    }


    private void LoadCertificate()
    {
        string certificatePath = Application.streamingAssetsPath + "/certificat.pem";
        if (File.Exists(certificatePath))
        {
            byte[] certificateBytes = File.ReadAllBytes(certificatePath);
            trustedCertificate = new X509Certificate2(certificateBytes);
            staticTrustedCertificate = trustedCertificate ; 
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

    private IEnumerator SendRequestWithAutoRefresh(
    string url,
    string method,
    Dictionary<string, string> headers,
    byte[] body,
    Action<UnityWebRequest> onSuccess,
    Action<UnityWebRequest> onError)
    {
        UnityWebRequest request = new UnityWebRequest(url, method);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (body != null && (method == "POST" || method == "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.uploadHandler.contentType = "application/json";
        }

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }

        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.responseCode == 401) // Unauthorized : when the access token is missing or expired 
        {
            Debug.Log("Access token expired or invalid. Attempting to refresh token...");

            UnityWebRequest refreshRequest = new UnityWebRequest(refreshTokenUrl, "POST");
            refreshRequest.downloadHandler = new DownloadHandlerBuffer();
            refreshRequest.SetRequestHeader("Content-Type", "application/json");
            refreshRequest.certificateHandler = new CustomCertificateHandler();

            yield return refreshRequest.SendWebRequest();

            if (refreshRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Token refreshed successfully. Retrying original request...");

                UnityWebRequest retryRequest = new UnityWebRequest(url, method);
                retryRequest.downloadHandler = new DownloadHandlerBuffer();

                if (body != null && (method == "POST" || method == "PUT"))
                {
                    retryRequest.uploadHandler = new UploadHandlerRaw(body);
                    retryRequest.uploadHandler.contentType = "application/json";
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        retryRequest.SetRequestHeader(header.Key, header.Value);
                    }
                }

                retryRequest.certificateHandler = new CustomCertificateHandler();

                yield return retryRequest.SendWebRequest();

                if (retryRequest.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(retryRequest);
                }
                else
                {
                    onError?.Invoke(retryRequest);
                }
            }
            else
            {
                // If the refresh token is missing or invalid, redirect without displaying an error 
                if (refreshRequest.responseCode == 400 || refreshRequest.responseCode == 401)
                {
                    Debug.Log("Refresh token is invalid or missing. Redirecting to login page...");
                    MenuSwapper.Instance.HandleSessionExpired(); // Redirects to login page
                }
                else
                {
                    onError?.Invoke(refreshRequest);
                }
            }
        }
        else if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request);
        }
        else
        {
            onError?.Invoke(request);
        }
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

    public IEnumerator UserStatistics()
    {
        yield return StartCoroutine(GetUserStatistics());
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


    public IEnumerator GamesPlayedStatistics()
    {
        // Attend la fin de la coroutine GetGamePlayedStats
        yield return StartCoroutine(GetGamePlayedStats());
        
        Debug.Log("GAME PLAYED !");
    }


    private IEnumerator RegisterUser(string username, string email, string password, string confirmPassword, string firstName, string lastName, string day, string month, string year)
    {
        if (password != confirmPassword)
        {
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
            Debug.Log("Registration successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.Log("Registration failed: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
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
            Debug.Log("Account verified successfully: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.Log("Verification failed: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
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
            Debug.Log("Verification code resent: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.Log("Error resending code: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
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
            Debug.Log("Login successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
            HeartbeatManager.Instance.SetLoggedIn(true);
        }
        else
        {
            Debug.Log("Login error: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
            IsRequestSuccessful = false;
        
        }
    }


    private IEnumerator LogoutUser()
    {

        yield return SendRequestWithAutoRefresh(
            logoutUrl,
            "POST",
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null, // no body for logout
            onSuccess: (response) =>
            {
                Debug.Log("Logout successful: " + response.downloadHandler.text);
                IsRequestSuccessful = true;
                HeartbeatManager.Instance.SetLoggedIn(false);
            },
            onError: (response) =>
            {
                Debug.LogError("Logout error: " + response.error);
                Debug.LogError("Details: " + response.downloadHandler.text);
                ErrorResponse = !string.IsNullOrEmpty(response.downloadHandler.text) ? response.downloadHandler.text : "An unknown error occurred.";
                IsRequestSuccessful = false;
            }
        );
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
            Debug.Log("Password reset code sent: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.Log("Password reset failed: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text); 
            // Updates ErrorResponse with the error message:
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
            IsRequestSuccessful = false;
        }
    }

    private IEnumerator ResetPasswordCoroutine(string email, string code, string newPassword, string confirmNewPassword)
    {
        if (newPassword != confirmNewPassword)
        {
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
            Debug.Log("Password reset successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.Log("Password reset error: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
            IsRequestSuccessful = false;
        }
    }

    public IEnumerator GetCurrentUser()
    {
        UnityWebRequest request = new UnityWebRequest(userDataUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();

        yield return SendRequestWithAutoRefresh(
            userDataUrl,
            "GET",
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null,
            onSuccess: (response) =>
            {
                Debug.Log("User Data: " + response.downloadHandler.text);

                // Parse user data
                try
                {
                    var userData = JsonUtility.FromJson<UserData>(response.downloadHandler.text);
                    CurrentUser = userData;
                    IsRequestSuccessful = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing user data: {ex.Message}");
                    IsRequestSuccessful = false;
                }
            },
            onError: (response) =>
            {
                Debug.LogError("Failed to retrieve user data: " + response.error);
                Debug.LogError("Details: " + response.downloadHandler.text);
                ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
                IsRequestSuccessful = false;
            }
        );
    }

    private IEnumerator GetUserStatistics()
    {
        UnityWebRequest request = new UnityWebRequest(userStatisticsUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();

        yield return SendRequestWithAutoRefresh(
            userStatisticsUrl,
            "GET",
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null, // No body for a GET request
            onSuccess: (response) =>
            {
                Debug.Log("User Statistics: " + response.downloadHandler.text);

                // Parse statistics data
                try
                {
                    var userStatistics = JsonUtility.FromJson<UserStatistics>(response.downloadHandler.text);
                    CurrentUserStatistics = userStatistics;
                    IsRequestSuccessful = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing user statistics: {ex.Message}");
                    IsRequestSuccessful = false;
                }
            },
            onError: (response) =>
            {
                Debug.LogError("Failed to retrieve user statistics: " + response.error);
                Debug.LogError("Details: " + response.downloadHandler.text);
                ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred."; 
                IsRequestSuccessful = false;
            }
        );
    }
private IEnumerator GetGamePlayedStats()
{
    UnityWebRequest request = new UnityWebRequest(playedGameUrl, "GET");

    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.certificateHandler = new CustomCertificateHandler();

    yield return SendRequestWithAutoRefresh(
        playedGameUrl,
        "GET",
        new Dictionary<string, string> { { "Content-Type", "application/json" } },
        null, // No body for GET
        onSuccess: (response) =>
        {
            string json = response.downloadHandler?.text; 

            if (json == null)
            {
                Debug.LogError("DownloadHandler text is null.");
                IsRequestSuccessful = false;
                return;
            }

            Debug.Log("Game Played Statistics (Raw JSON): " + json);

            try
            {
                if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                {
                    Debug.LogWarning("No game stats returned or content is 'null'.");
                    IsRequestSuccessful = false;
                    return;
                }

                var stats = JsonUtility.FromJson<PlayedGameStats>(json);

                if (stats == null)
                {
                    Debug.LogWarning("Deserialization failed: 'stats' is null.");
                    IsRequestSuccessful = false;
                    return;
                }

                if (stats.totalGames == 0 || stats.totalVictories < 0 || stats.tanksDestroyed < 0 || string.IsNullOrEmpty(stats.mapPlayed))
                {
                    Debug.LogWarning("Some critical game stats are missing or invalid.");
                    IsRequestSuccessful = false;
                    return;
                }

                CurrentPlayedGameStats = stats;
                IsRequestSuccessful = true;

                float victoryRatio = stats.totalGames > 0
                    ? (float)stats.totalVictories / stats.totalGames
                    : 0f;

                Debug.LogWarning("Updating UI with game stats.");
                
                if (username != null ) 
                {
                    username.text = stats.username.ToString();


                }
                if (nb_games_played != null)
                {
                    nb_games_played.text = stats.totalGames.ToString();
                    Debug.LogWarning("Updated nb_games_played.");
                }

                if (victories != null)
                {
                    victories.text = stats.totalVictories.ToString();
                    Debug.LogWarning("Updated victories.");
                }

                if (ratio != null)
                {
                    ratio.text = victoryRatio.ToString("P1");
                    Debug.LogWarning("Updated ratio.");
                }

                if (tanks_destroyed != null)
                {
                    tanks_destroyed.text = stats.tanksDestroyed.ToString();
                    Debug.LogWarning("Updated tanks_destroyed.");
                }

                if (map != null)
                {
                    map.text = stats.mapPlayed;
                    Debug.LogWarning("Updated map.");
                }

                if (rank != null)
                {
                    rank.text = stats.playerRank.ToString();
                    Debug.LogWarning("Updated rank.");
                }

                if (victory_or_defeat != null)
                {
                    victory_or_defeat.text = stats.gameWon ? "Victory" : "Defeat";
                    Debug.LogWarning("Updated victory_or_defeat.");
                }

                try
                {
                    DateTime parsedGameDate = DateTime.Parse(stats.gameDate);
                    if (date != null)
                    {
                        date.text = parsedGameDate.ToString("dd/MM/yyyy HH:mm");
                        Debug.LogWarning("Updated date.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to parse gameDate: " + ex.Message);
                }

            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while parsing or using game stats: {ex.Message}");
                IsRequestSuccessful = false;
            }
        },
        onError: (response) =>
        {
            long responseCode = response.responseCode;
            string responseText = response.downloadHandler != null ? response.downloadHandler.text : "";

            if (responseCode == 404)
            {
                Debug.LogWarning("No games found for the current user (404).");
            }
            else
            {
                Debug.LogError($"Failed to retrieve game statistics. HTTP {responseCode}: {response.error}");
                Debug.LogError("Details: " + responseText);
            }

            ErrorResponse = !string.IsNullOrEmpty(responseText)
                ? responseText
                : "An unknown error occurred.";
            IsRequestSuccessful = false;
        }
    );
}

    private IEnumerator SendHeartbeat()
    {
        Debug.Log("Is Logged In: " + isLoggedIn);

        if (!isLoggedIn) 
        {
            Debug.LogWarning("User is not logged in. Heartbeat request not sent.");
            yield break;  
        }
        else 
        {
             Debug.LogWarning("USER IS LOGGED IN");
        }

        Debug.LogWarning("SEND HEARTBEAT");
        yield return SendRequestWithAutoRefresh(
            heartbeatUrl,  
            "POST",  
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null,  
            onSuccess: (response) =>
            {
                Debug.LogWarning("Heartbeat SUCCESS");
            },
            onError: (response) =>
            {
                Debug.LogWarning("Heartbeat failed: " + response.error);
            }
        );

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
    public string firstName;
    public string lastName;
    public string createdAt;
    public string birthDate;
}

[System.Serializable]
public class UserStatistics
{
    public int gamesPlayed;
    public int highestScore;
    public int ranking;
}


[System.Serializable]
public class PlayedGameStats
{
    public string username;
    public int totalGames;
    public int totalVictories;
    public int tanksDestroyed;
    public string mapPlayed;
    public int playerRank;
    public bool gameWon;
    public string gameDate;  
}
