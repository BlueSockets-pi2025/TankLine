using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Timers;

using Heartbeat;
using static TokenCrypto;


public class Bypass : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Always return true to bypass certificate validation
        return true;
    }
}
public class AuthController : MonoBehaviour
{
    [Header("UI Elements")]

    private string registerUrl;
    private string loginUrl;
    private string autologinUrl;
    private string logoutUrl;
    private string verifyUrl;
    private string resendVerificationUrl;
    private string resetPasswordRequestUrl;
    private string resetPasswordUrl;
    private string userDataUrl;
    private string userStatisticsUrl;
    private string refreshTokenUrl;
    private string playedGameUrl;
    private string heartbeatUrl;
    private string setRefreshCookieUrl;
    private string disconnectUrl;
    private string refreshExpirationUrl;
    private string refreshRefreshTokenUrl;
    private string leaderboardUrl;

    private int minExpirationRefreshToken = 24; // Minimum expiration time for refresh token in hours when checking in launch

    private static AuthController instance;

    public bool IsInitialized { get; private set; } = false; // Indicates if the AuthController is initialized (for other managers)

    private static X509Certificate2 trustedCertificate;

    public EnvVariables endpointsConfig; // Server configuration (database server endpoints)

    public bool IsRequestSuccessful { get; private set; }
    public string ErrorResponse { get; private set; }
    public UserData CurrentUser { get; private set; }
    public UserStatistics CurrentUserStatistics { get; private set; }

    public PlayedGameStats CurrentPlayedGameStats { get; private set; }

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

    public const string PathToEnvFile = "/.env";

    private static X509Certificate2 staticTrustedCertificate;

    private void OnApplicationQuit()
    {
        // Disconnect to enable other users to connect without deleting the refresh token: 
        DisconnectSynchronously();
    }
    /*
    public void OnApplicationQuitNative() // Mobile 
    {
        Debug.Log("Application is quitting (detected via native Android).");
        DisconnectSynchronously();
    }
    */
    private void Awake()
    {   
        StartCoroutine(Initialization());
    }

    private IEnumerator Initialization()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Platform is Android.");
        }
        else
        {
            Debug.Log("Platform is not Android.");
        }
        // Load the trusted certificate from the specified path:
        yield return StartCoroutine(LoadCertificate());

        // Load the server configuration (database server endpoints) from the `.env` file:
        yield return StartCoroutine(LoadEndpointsConfig());

        // Load the encryption configuration (password and salt) from the `.env` file:
        yield return StartCoroutine(TokenCrypto.LoadEncryptionConfig(this)); // "this" for the monoBehaviour instance

        // Load environment variables for the database server:         
        string serverIp = endpointsConfig.DATABASE_SERVER_IP;
        string serverPort = endpointsConfig.DATABASE_SERVER_PORT;

        if (string.IsNullOrEmpty(serverIp) || string.IsNullOrEmpty(serverPort))
        {
            Debug.LogError("Database server IP or port is not set in the environment variables.");
            yield break;
        }

        // Build URLs dynamically: 
        string baseUrl = $"https://{serverIp}:{serverPort}/api";

        registerUrl = $"{baseUrl}/auth/register";
        loginUrl = $"{baseUrl}/auth/login";
        autologinUrl = $"{baseUrl}/auth/autologin";
        logoutUrl = $"{baseUrl}/auth/logout";
        verifyUrl = $"{baseUrl}/auth/verify";
        resendVerificationUrl = $"{baseUrl}/auth/resend-verification-code";
        resetPasswordRequestUrl = $"{baseUrl}/auth/request-password-reset";
        resetPasswordUrl = $"{baseUrl}/auth/reset-password";
        userDataUrl = $"{baseUrl}/user/me";
        userStatisticsUrl = $"{baseUrl}/user/me/statistics";
        refreshTokenUrl = $"{baseUrl}/auth/refresh-token";
        playedGameUrl = $"{baseUrl}/PlayedGames/summary";
        heartbeatUrl = $"{baseUrl}/auth/heartbeat";
        setRefreshCookieUrl = $"{baseUrl}/auth/set-refresh-cookie";
        disconnectUrl = $"{baseUrl}/auth/disconnect";
        refreshExpirationUrl = $"{baseUrl}/auth/refresh-expiration";
        refreshRefreshTokenUrl = $"{baseUrl}/auth/refresh-refresh-token";
        leaderboardUrl = $"{baseUrl}/leaderboard";

        Debug.Log("Initialization complete.");
        IsInitialized = true; // Indique que l'initialisation est terminée

    }

    /// <summary>
    /// Loads the trusted certificate from the specified path. <br/>
    /// This method is called in the Awake method.
    /// </summary>
    private IEnumerator LoadCertificate()
    {
        string certificatePath = Application.streamingAssetsPath + "/certificat.pem";

        Debug.Log("Loading trusted certificate from: " + certificatePath);

        //Debug.Log("Certificate path: " + certificatePath);
        Debug.Log("Current platform: " + Application.platform);

#if UNITY_ANDROID
            Debug.Log("Running on Android. Attempting to load certificate using UnityWebRequest.");
            yield return StartCoroutine(LoadCertificateForAndroid(certificatePath));
#else
        Debug.Log("Running on a non-Android platform. Attempting to load certificate directly.");
        if (File.Exists(certificatePath))
        {
            byte[] certificateBytes = File.ReadAllBytes(certificatePath);
            trustedCertificate = new X509Certificate2(certificateBytes);
            staticTrustedCertificate = trustedCertificate;
            Debug.Log("Certificate successfully loaded.");
        }
        else
        {
            Debug.LogError("Certificate file not found! Ensure it's in the correct directory.");
        }
        yield return null; // Guarantee that the coroutine ends
#endif
    }

    /// <summary>
    /// Loads the trusted certificate from the specified path for Android. <br/>
    /// </summary>
    /// <param name="certificatePath">The path to the certificate file.</param>
    private IEnumerator LoadCertificateForAndroid(string certificatePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(certificatePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] certificateBytes = request.downloadHandler.data;
            trustedCertificate = new X509Certificate2(certificateBytes);
            staticTrustedCertificate = trustedCertificate;
            Debug.Log("Certificate successfully loaded on Android.");
        }
        else
        {
            Debug.LogError("Failed to load certificate on Android: " + request.error);
        }
    }

    

    /// <summary>
    /// Load the server configuration (database server endpoints) from the `.env` file and store it in the variable `endpointsConfig`
    /// </summary>
    private IEnumerator LoadEndpointsConfig()
    {
        string envFilePath;

#if UNITY_ANDROID
            Debug.Log("Running on Android. Attempting to load .env file using UnityWebRequest.");
            envFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, ".env");
            yield return StartCoroutine(LoadEndpointsConfigForAndroid(envFilePath));
#else
        envFilePath = Application.streamingAssetsPath + "./env";
        if (File.Exists(envFilePath))
        {
            string jsonEnv = File.ReadAllText(envFilePath);
            endpointsConfig = JsonUtility.FromJson<EnvVariables>(jsonEnv);
            Debug.Log("Endpoints configuration successfully loaded.");
        }
        else
        {
            Debug.LogError("Env file not found! Ensure it's in the correct directory.");
        }
        yield return null;
#endif
    }

    private IEnumerator LoadEndpointsConfigForAndroid(string envFilePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(envFilePath);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string envContent = request.downloadHandler.text;
            endpointsConfig = JsonUtility.FromJson<EnvVariables>(envContent);

            Debug.Log("Endpoints configuration successfully loaded on Android.");
            Debug.Log("Env return: " + envContent);
            Debug.Log("EndpointsConfig: " + JsonUtility.ToJson(endpointsConfig));

        }
        else
        {
            Debug.LogError("Failed to load .env file on Android: " + request.error);
        }
    }

    /// <summary>
    /// Get the URL for the heartbeat request (used in HeartbeatManager).
    /// </summary>
    public string GetHeartbeatUrl()
    {
        return heartbeatUrl;
    }

    /// <summary>
    /// Returns the trusted certificate.
    /// </summary>
    /// <returns>The trusted certificate.</returns>
    public static X509Certificate2 GetTrustedCertificate()
    {
        return trustedCertificate;
    }

    /// <summary>
    /// Send a web request to the server with automatic access-token refresh if needed.
    /// </summary>
    public IEnumerator SendRequestWithAutoRefresh(
    string url,
    string method,
    Dictionary<string, string> headers,
    byte[] body,
    Action<UnityWebRequest> onSuccess,
    Action<UnityWebRequest> onError)
    {
        // First request to the server:
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
        // request.certificateHandler = new Bypass();


        yield return request.SendWebRequest();

        // Check if the request was successful:
        if (request.responseCode == 401) // Unauthorized : when the access token is missing or expired 
        {
            Debug.Log("Access token expired or invalid. Attempting to refresh token...");

            // Attempt to refresh the token (using the refresh token):
            UnityWebRequest refreshRequest = new UnityWebRequest(refreshTokenUrl, "POST");
            refreshRequest.downloadHandler = new DownloadHandlerBuffer();
            refreshRequest.SetRequestHeader("Content-Type", "application/json");
            refreshRequest.certificateHandler = new CustomCertificateHandler();
            // refreshRequest.certificateHandler = new Bypass();

            yield return refreshRequest.SendWebRequest();

            // Check if the refresh token request was successful:
            if (refreshRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Token refreshed successfully. Retrying original request...");

                // Retry the original request with the new access token:
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
                // retryRequest.certificateHandler = new Bypass();

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
                // If the refresh token is missing or invalid, redirect without displaying an error:
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

    /// <summary>
    /// Register a new user.
    /// </summary>
    public IEnumerator Register(string username, string email, string password, string confirmPassword, string firstName, string lastName, string day, string month, string year)
    {
        yield return StartCoroutine(RegisterUser(username, email, password, confirmPassword, firstName, lastName, day, month, year));
    }

    /// <summary>
    /// Verify the account using the email and verification code.
    /// </summary>
    /// <param name="email">The email address of the user.</param>, <param name="code">The verification code sent to the user's email.</param>
    public IEnumerator VerifyAccountButton(string email, string code)
    {
        yield return StartCoroutine(VerifyAccount(email, code));
    }

    /// <summary>
    /// Resend the verification code to the user's email.
    /// </summary>
    /// <param name="email">Email of the user.</param>
    public IEnumerator ResendVerificationCode(string email)
    {
        yield return StartCoroutine(ResendVerification(email));
    }

    /// <summary>
    /// Login the user using username or email and password.
    /// </summary>
    /// <param name="usernameOrEmail">Username or email of the user.</param>, <param name="password">Password of the user.</param>
    public IEnumerator Login(string usernameOrEmail, string password)
    {
        yield return StartCoroutine(LoginUser(usernameOrEmail, password));
    }

    /// <summary>
    /// Get the current user data.
    /// </summary>
    public IEnumerator User()
    {
        yield return StartCoroutine(GetCurrentUser());
    }

    /// <summary>
    /// Auto-login the user using the refresh token.
    /// </summary>
    public IEnumerator AutoLogin()
    {
        yield return StartCoroutine(AutoLoginUser());
    }

    /// <summary>
    /// Get the statistics of the current user.
    /// </summary>
    public IEnumerator UserStatistics()
    {
        yield return StartCoroutine(GetUserStatistics());
    }

    /// <summary>
    /// Logout the user.
    /// </summary>
    public IEnumerator Logout()
    {
        yield return StartCoroutine(LogoutUser());
    }

    /// <summary>
    /// Restore the refresh token in the cookie.
    /// </summary>
    public IEnumerator RestoreRefreshCookie()
    {
        yield return StartCoroutine(RestoreRefreshTokenInCookie());
    }

    /// <summary>
    /// Check and rotate the refresh token if it's about to expire.
    /// </summary>
    public IEnumerator CheckAndRotateRefreshToken()
    {
        yield return StartCoroutine(CheckAndRotateRefreshTokenCoroutine());
    }

    /// <summary>
    /// Request a password reset using the user's email.
    /// </summary>
    /// <param name="email">Email of the user.</param>
    public IEnumerator RequestPasswordReset(string email)
    {
        yield return StartCoroutine(RequestPasswordResetCoroutine(email));
    }

    /// <summary>
    /// Reset the password using the email, verification code, new password, and confirmation password.
    /// </summary>
    /// <param name="email">Email of the user.</param>, <param name="code">Verification code sent to the user's email.</param>, 
    /// <param name="newPassword">New password for the user.</param>, <param name="confirmNewPassword">Confirmation of the new password.</param>
    public IEnumerator ResetPassword(string email, string code, string newPassword, string confirmNewPassword)
    {
        yield return StartCoroutine(ResetPasswordCoroutine(email, code, newPassword, confirmNewPassword));
    }

    /// <summary>
    /// Get the statistics of the played games.
    /// </summary>
    public IEnumerator GamesPlayedStatistics()
    {
        // Attend la fin de la coroutine GetGamePlayedStats
        yield return StartCoroutine(GetGamePlayedStats());

        Debug.Log("GAME PLAYED !");
    }

    /// <summary>
    /// Coroutine function to send user registration server request.
    /// </summary>
    /// <param name="username">Username of the user.</param>, <param name="email">Email of the user.</param>,
    /// <param name="password">Password of the user.</param>, <param name="confirmPassword">Confirmation of the password.</param>,
    /// <param name="firstName">First name of the user.</param>, <param name="lastName">Last name of the user.</param>,
    /// <param name="day">Day of birth.</param>, <param name="month">Month of birth.</param>, <param name="year">Year of birth.</param>
    private IEnumerator RegisterUser(string username, string email, string password, string confirmPassword, string firstName, string lastName, string day, string month, string year)
    {
        if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match.");
            IsRequestSuccessful = false;
            yield break;
        }

        string birthDate = FormatDateForApi(day, month, year);

        // Prepare the request data:
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

        // Convert the request data to JSON:
        string json = JsonUtility.ToJson(requestData);

        // Create the UnityWebRequest:
        UnityWebRequest request = new UnityWebRequest(registerUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();
        //request.certificateHandler = new Bypass();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Registration successful: " + request.downloadHandler.text);
            // Set IsRequestSuccessful to true to indicate success to MenuSwapper:
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.Log("Registration failed: " + request.error);
            Debug.Log("Details: " + request.downloadHandler.text);
            // Updates ErrorResponse with the error message (popup):
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred.";
            // Set IsRequestSuccessful to false to indicate failure to MenuSwapper:
            IsRequestSuccessful = false;
        }
    }

    /// <summary>
    /// Format the date for API request.
    /// </summary>
    /// <param name="day">Day of birth.</param>, <param name="month">Month of birth.</param>, <param name="year">Year of birth.</param>
    /// <returns>Formatted date string.</returns>
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

    /// <summary>
    /// Coroutine to send the account verification server request.
    /// </summary>
    /// <param name="email">Email of the user.</param>, <param name="code">Verification code sent to the user's email.</param>
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
        //request.certificateHandler = new Bypass();

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

    /// <summary>
    /// Coroutine to resend the verification code to the user's email.
    /// </summary>
    /// <param name="email">Email of the user.</param>
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
        //request.certificateHandler = new Bypass();

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

    /// <summary>
    /// Coroutine to send the login server request.
    /// </summary>
    /// <param name="usernameOrEmail">Username or email of the user.</param>, <param name="password">Password of the user.</param>
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
        // request.certificateHandler = new Bypass();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login successful: " + request.downloadHandler.text);
            IsRequestSuccessful = true;

            // Read the JSON response (with refresh-token):
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            // Local file path to save the refresh token:
            string tokenPath = Path.Combine(Application.persistentDataPath, "refresh.token");
            Debug.Log(tokenPath);

            // Check if “Remember Me” is enabled to activate autologin and save the refresh token: 
            if (MenuSwapper.Instance.RememberMeToggle.isOn)
            {
                try
                {
                    // Encrypt the refresh token: 
                    string encryptedToken = TokenCrypto.Encrypt(loginResponse.refreshToken);

                    // Save to:
                    File.WriteAllText(tokenPath, encryptedToken);

                    Debug.Log("Refresh token saved securely.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to save refresh token securely: " + ex.Message);
                }
            }
            else
            {
                Debug.Log("Remember Me is not enabled. Refresh token will not be saved.");
                // Delete the refresh token file if it exists:
                if (File.Exists(tokenPath))
                {
                    File.Delete(tokenPath);
                    Debug.Log("Refresh token file deleted.");
                }
                else
                {
                    Debug.Log("No refresh token file found to delete.");
                }
            }

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

    /// <summary>
    /// Coroutine to enable autologin at launch 
    /// </summary>
    private IEnumerator AutoLoginUser()
    {
        // Autologin to set the access token in the cookie (with refresh token) and to set the logged-in status to true (server-side):
        UnityWebRequest request = new UnityWebRequest(autologinUrl, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(null); // empty body
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Successful autologin.");
            HeartbeatManager.Instance.SetLoggedIn(true); // Set logged in status to true
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.LogWarning("Failed autologin: " + request.error);
            Debug.LogWarning("Details: " + request.downloadHandler.text);
            MenuSwapper.Instance.HandleSessionExpired(); // Redirects to login page
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred.";
            IsRequestSuccessful = false;
        }
    }

    /// <summary>
    /// Coroutine to send the logout server request.
    /// </summary>
    private IEnumerator LogoutUser()
    {
        // Logout to delete the tokens in the cookie (with refresh-token) and the refresh-token file and to set the logged-in status to false (server-side):
        yield return SendRequestWithAutoRefresh(
            logoutUrl,
            "POST",
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null, // no body for logout
            onSuccess: (response) =>
            {
                Debug.Log("Logout successful: " + response.downloadHandler.text);
                IsRequestSuccessful = true;
                // Delete the refresh-token file:
                string tokenPath = Path.Combine(Application.persistentDataPath, "refresh.token");
                if (File.Exists(tokenPath))
                {
                    File.Delete(tokenPath);
                    Debug.Log("Refresh token file deleted.");
                }
                else
                {
                    Debug.LogWarning("No refresh token file found to delete.");
                }
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

    /// <summary>
    /// Coroutine to send the request for password reset.
    /// </summary>
    /// <param name="email">Email of the user.</param>
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
        // request.certificateHandler = new Bypass();

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

    /// <summary>
    /// Coroutine to send the password reset server request.
    /// </summary>
    /// <param name="email">Email of the user.</param>, <param name="code">Verification code sent to the user's email.</param>,
    /// <param name="newPassword">New password for the user.</param>, <param name="confirmNewPassword">Confirmation of the new password.</param>
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
            Code = code,
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
        //request.certificateHandler = new Bypass();

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

    /// <summary>
    /// Coroutine to send the request for current user data.
    /// </summary>
    public IEnumerator GetCurrentUser()
    {
        UnityWebRequest request = new UnityWebRequest(userDataUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();
        // request.certificateHandler = new Bypass();

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

    /// <summary>
    /// Coroutine to send the request for user statistics.
    /// </summary>
    private IEnumerator GetUserStatistics()
    {
        UnityWebRequest request = new UnityWebRequest(userStatisticsUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();
        // request.certificateHandler = new Bypass();

        yield return SendRequestWithAutoRefresh(
            userStatisticsUrl,
            "GET",
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null, // No body for a GET request
            onSuccess: (response) =>
            {
                Debug.Log("User Statistics: " + response.downloadHandler.text);

                // Parse statistics data:
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

    /// <summary>
    /// Coroutine to restore the refresh token in the cookie after autologin.
    /// </summary>
    private IEnumerator RestoreRefreshTokenInCookie()
    {
        // Path to refresh token file:
        string tokenPath = Path.Combine(Application.persistentDataPath, "refresh.token");

        Debug.Log("Token path: " + tokenPath);

        // Check if the file containing the refresh token exists:
        if (!File.Exists(tokenPath))
        {
            Debug.Log("No saved refresh token.");
            yield break;
        }

        // Read encrypted token from file: 
        string encryptedToken = File.ReadAllText(tokenPath);
        string refreshToken;

        try
        {
            // Decrypting the token:
            refreshToken = TokenCrypto.Decrypt(encryptedToken);
        }
        catch
        {
            // If the operation fails, delete the token file and stop the process: 
            Debug.LogWarning("Failed to decrypt refresh token. Deleting.");
            File.Delete(tokenPath);
            yield break;
        }

        // Create the payload for the token renewal request: 
        var requestData = new RestoreCookieRequest
        {
            RefreshToken = refreshToken
        };
        string json = JsonUtility.ToJson(requestData);

        // Prepare token renewal request: 
        UnityWebRequest request = new UnityWebRequest(setRefreshCookieUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.certificateHandler = new CustomCertificateHandler();

        // Wait for request response: 
        yield return request.SendWebRequest();

        // Check request response: 
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Refresh token restored successfully.");
            IsRequestSuccessful = true;
        }
        else
        {
            // On failure, display error and delete token file: 
            Debug.LogWarning("Failed to restore refresh token: " + request.error);
            Debug.LogWarning("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred.";
            // Delete the refresh token file if there is an error: 
            File.Delete(tokenPath);

            IsRequestSuccessful = false;

        }
    }

    /// <summary>
    /// Disconnect the user synchronously before leaving the game .
    /// </summary>
    private void DisconnectSynchronously()
    {
        UnityWebRequest request = new UnityWebRequest(disconnectUrl, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(null); // empty body
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();

        // Send the request synchronously:
        request.SendWebRequest();

        while (!request.isDone)
        {
            // Wait for the request to complete
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Disconnection successful");
        }
        else
        {
            Debug.LogWarning("Failed disconnection: " + request.error);
            Debug.LogWarning("Details: " + request.downloadHandler.text);
        }
    }

    /// <summary>
    /// Coroutine to check and rotate the refresh token if it's about to expire (minExpirationRefreshToken).
    /// </summary>
    private IEnumerator CheckAndRotateRefreshTokenCoroutine()
    {
        UnityWebRequest request = new UnityWebRequest(refreshExpirationUrl, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var json = request.downloadHandler.text;
            var expiration = JsonUtility.FromJson<RefreshTokenExpirationResponse>(json);
            DateTime expiresAt = DateTime.Parse(expiration.expiresAt);

            // Check if the refresh token is about to expire by comparing the expiration time (server-side) with the current time:
            if ((expiresAt - DateTime.UtcNow).TotalHours < minExpirationRefreshToken)
            {
                Debug.Log("Refresh token expiring soon. Rotating.");

                // Call the refresh token endpoint to rotate the refresh token if it's about to expire:
                yield return RefreshRefreshToken();

                if (IsRequestSuccessful)
                {
                    Debug.Log("Refresh token rotated successfully.");
                    IsRequestSuccessful = true;
                }
                else
                {
                    Debug.LogWarning("Failed to rotate refresh token.");
                    ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred.";
                    IsRequestSuccessful = false;
                }
            }
            else
            {
                Debug.Log("Refresh token is valid long enough.");
                IsRequestSuccessful = true;
            }
        }
        else
        {
            Debug.LogWarning("Could not check refresh token expiration.");
            Debug.LogWarning("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred.";
            MenuSwapper.Instance.HandleSessionExpired(); // Redirects to login page
            IsRequestSuccessful = false;
        }
    }

    /// <summary>
    /// Coroutine to refresh the refresh token.
    /// </summary>
    private IEnumerator RefreshRefreshToken()
    {
        UnityWebRequest request = new UnityWebRequest(refreshRefreshTokenUrl, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(null); // empty body
        request.SetRequestHeader("Content-Type", "application/json");
        request.certificateHandler = new CustomCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Successful refresh of refresh token");
            IsRequestSuccessful = true;
        }
        else
        {
            Debug.LogWarning("Failed refresh of refresh token: " + request.error);
            Debug.LogWarning("Details: " + request.downloadHandler.text);
            ErrorResponse = !string.IsNullOrEmpty(request.downloadHandler.text) ? request.downloadHandler.text : "An unknown error occurred.";
            IsRequestSuccessful = false;
        }
    }

    /// <summary>
    /// Coroutine to send the request for leaderboard data.
    /// </summary>
    /// <param name="onSuccess">Callback for successful response.</param>, <param name="onError">Callback for error response.</param>
    public IEnumerator GetLeaderboard(Action<List<LeaderboardEntry>> onSuccess, Action<string> onError)
    {
        yield return SendRequestWithAutoRefresh(
            leaderboardUrl,
            "GET",
            new Dictionary<string, string> { { "Content-Type", "application/json" } },
            null, // No body for a GET request
            onSuccess: (response) =>
            {
                // Get and parse the leaderboard data:
                try
                {
                    string jsonResponse = response.downloadHandler.text;
                    List<LeaderboardEntry> leaderboardEntries = JsonUtility.FromJson<LeaderboardList>($"{{\"entries\":{jsonResponse}}}").entries;
                    onSuccess?.Invoke(leaderboardEntries);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing leaderboard data: {ex.Message}");
                    onError?.Invoke("Failed to parse leaderboard data.");
                }
            },
            onError: (response) =>
            {
                string errorMessage = !string.IsNullOrEmpty(response.downloadHandler.text)
                    ? response.downloadHandler.text
                    : "An unknown error occurred.";
                Debug.LogError($"Failed to fetch leaderboard: {errorMessage}");
                onError?.Invoke(errorMessage);
            }
        );
    }

    /// <summary>
    /// Coroutine to get the statistics of the played games.
    /// </summary>  
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

                    if (username != null)
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
}


/// Custom certificate handler class to validate the server's SSL certificate.
public class CustomCertificateHandler : CertificateHandler
{
    // This method is called to validate the certificate received from the server
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Retrieve the trusted certificate that was preloaded into Unity's assets
        X509Certificate2 trustedCert = AuthController.GetTrustedCertificate();

        // If no trusted certificate is loaded, log an error and return false to reject the certificate
        if (trustedCert == null)
        {
            Debug.LogError("No trusted certificate loaded.");
            return false;  // If no trusted certificate is found, the certificate validation fails
        }

        // Create an X509Certificate2 object from the certificate data received from the server
        X509Certificate2 receivedCertificate = new X509Certificate2(certificateData);

        // Compare the thumbprints (unique identifiers) of the two certificates
        bool isMatch = string.Equals(
            receivedCertificate.Thumbprint,    // Thumbprint of the certificate received from the server
            trustedCert.Thumbprint,            // Thumbprint of the preloaded trusted certificate
            StringComparison.OrdinalIgnoreCase  // Perform a case-insensitive comparison
        );

        // If the thumbprints do not match, log a warning
        if (!isMatch)
        {
            Debug.LogWarning("Server certificate does not match trusted certificate.");
        }

        // Return whether the thumbprints match
        return isMatch;
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

[System.Serializable]
public class ResendVerificationRequest
{
    public string Email;
}

[System.Serializable]
public class RestoreCookieRequest
{
    public string RefreshToken;
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
    public string username;
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

[System.Serializable]
public class LoginResponse
{
    public string refreshToken;
}

[System.Serializable]
public class SaveTokenResponse
{
    public string refreshToken;
}

[System.Serializable]
public class RefreshTokenExpirationResponse
{
    public string expiresAt;
}

[System.Serializable]
public class LeaderboardEntry
{
    public string username;
    public int highestScore;
    public int ranking;
    public int gamesPlayed;
}

[System.Serializable]
public class LeaderboardList
{
    public List<LeaderboardEntry> entries;
}