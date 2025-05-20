using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace Heartbeat
{
    public class HeartbeatManager : MonoBehaviour
    {
        public static HeartbeatManager Instance { get; private set; }

        [SerializeField] private string heartbeatUrl;
        [SerializeField] private float heartbeatInterval = 5f; //30f

        private float timeSinceLastHeartbeat = 0f;
        private bool isLoggedIn = false;

        private AuthController authController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator Start()
        {
            authController = FindObjectOfType<AuthController>(); 
            if (authController == null)
            {
                Debug.LogError("AuthController not found in the scene.");
                yield break;
            }

            while (!authController.IsInitialized)
            {
                Debug.Log("Waiting for AuthController initialization...");
                yield return null;
            }
            
            // Get the heartbeat URL from AuthController:
            heartbeatUrl = authController.GetHeartbeatUrl();

            // Start the heartbeat timer:
            StartCoroutine(HeartbeatTimer());  
        }

        /// <summary>
        /// Coroutine that sends a heartbeat request every heartbeatInterval seconds if the user is logged in.
        /// </summary>
        private IEnumerator HeartbeatTimer()
        {
                Debug.Log("HEARTBEAT TRIGGERED...");
                while (true)
                {
                    // Wait 5 seconds
                    yield return new WaitForSeconds(heartbeatInterval);
                    
                    Debug.Log("HeartbeatTimer Is Logged In: " + isLoggedIn);
                    
                    if (isLoggedIn)
                    {
                        Debug.Log("Starting SendHeartbeat coroutine...");
                        yield return StartCoroutine(SendHeartbeat());
                    }
                }
            }

        /// <summary>
        /// Sends a heartbeat request to the server.
        /// This is called every heartbeatInterval seconds if the user is logged in.
        /// </summary>
        private IEnumerator SendHeartbeat()
        {
            Debug.Log("Is Logged In: " + isLoggedIn);

            if (!isLoggedIn)
            {
                Debug.LogWarning("User is not logged in. Heartbeat request not sent.");
                yield break;
            }

            Debug.LogWarning("SEND HEARTBEAT");

            // Call SendRequestWithAutoRefresh from AuthController allows the access token to be refreshed when it expires:
            yield return authController.SendRequestWithAutoRefresh(
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

        /// <summary>
        /// Sets the logged-in status of the user.
        /// This is used to determine whether to send heartbeat requests.
        /// </summary>
        /// <param name="loggedIn">True if the user is logged in, false otherwise.</param>
        public void SetLoggedIn(bool loggedIn) 
        {
            isLoggedIn = loggedIn ; 
        }
    }
}
